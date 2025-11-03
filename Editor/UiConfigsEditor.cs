using System;
using System.Collections.Generic;
using System.Linq;
using GameLovers.UiService;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace

namespace GameLoversEditor.UiService
{
	/// <summary>
	/// Helps selecting the <see cref="UiConfigs"/> asset file in the Editor
	/// </summary>
	public static class UiConfigsMenuItems
	{
		[MenuItem("Tools/UI Service/Select UiConfigs")]
		private static void SelectUiConfigs()
		{
			var assets = AssetDatabase.FindAssets($"t:{nameof(UiConfigs)}");
			var scriptableObject = assets.Length > 0 ? 
				AssetDatabase.LoadAssetAtPath<UiConfigs>(AssetDatabase.GUIDToAssetPath(assets[0])) :
				ScriptableObject.CreateInstance<UiConfigs>();

			if (assets.Length == 0)
			{
				AssetDatabase.CreateAsset(scriptableObject, $"Assets/{nameof(UiConfigs)}.asset");
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}

			Selection.activeObject = scriptableObject;
			FocusInspectorWindow();
		}

		[MenuItem("Tools/UI Service/Layer Visualizer")]
		public static void ShowLayerVisualizer()
		{
			// Set the pref BEFORE selecting so OnEnable reads the correct value
			EditorPrefs.SetBool("UiConfigsEditor_ShowVisualizer", true);
			
			SelectUiConfigs();

			// Force inspector refresh to rebuild the UI
			ActiveEditorTracker.sharedTracker.ForceRebuild();
		}
		
		private static void FocusInspectorWindow()
		{
			// Get the Inspector window type using reflection
			var inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
			if (inspectorType != null)
			{
				// Focus or create the Inspector window
				EditorWindow.GetWindow(inspectorType);
			}
		}
	}

	/// <summary>
	/// Improves the inspector visualization for the <see cref="UiConfigs"/> scriptable object
	/// </summary>
	/// <remarks>
	/// This abstract class should be implemented in game project being used.
	/// 
	/// Ex:
	/// [CustomEditor(typeof(UiConfigs))]
	/// public class GameUiConfigsEditor : UiConfigsEditor<![CDATA[<UiSetId>]]>
	/// {
	/// }
	/// </remarks>
	/// <typeparam name="TSet">The enum type of the <see cref="UiSetConfig"/> id </typeparam>
	public abstract class UiConfigsEditor<TSet> : Editor
		where TSet : Enum
	{
		private const string UiConfigExplanation = 
			"UI Presenter Configurations\n\n" +
			"Lists all Addressable UI Presenter prefabs in the game with their sorting layer values. " +
			"The Layer field controls the rendering order - higher values appear closer to the camera. " +
			"For presenters with Canvas or UIDocument components, this value directly maps to the UI sorting order.";

		private const string UiSetExplanation =
			"UI Set Configurations\n\n" +
			"UI Sets group multiple presenter instances that should be displayed together. " +
			"When a set is activated via UiService, all its presenters are loaded and shown simultaneously. " +
			"Presenters are loaded in the order listed (top to bottom).\n\n" +
			"Each UI's instance address is automatically set to its Addressable address from the config.";

		private const string VisualizerPrefsKey = "UiConfigsEditor_ShowVisualizer";

		private Dictionary<string, string> _assetPathLookup;
		private List<string> _uiConfigsAddress;
		private Dictionary<string, Type> _uiTypesByAddress; // Maps addressable address to Type
		private UiConfigs _scriptableObject;
		private SerializedProperty _configsProperty;
		private SerializedProperty _setsProperty;
		private bool _showVisualizer;
		private VisualElement _visualizerContainer;
		private string _visualizerSearchFilter = "";

		private void OnEnable()
		{
			_scriptableObject = target as UiConfigs;
			if (_scriptableObject == null)
			{
				return;
			}

			SyncConfigsWithAddressables();
			
			// Ensure sets array matches enum size
			_scriptableObject.SetSetsSize(Enum.GetNames(typeof(TSet)).Length);
			
			// Update the serializedObject to reflect the changes
			serializedObject.Update();
			
			_configsProperty = serializedObject.FindProperty("_configs");
			_setsProperty = serializedObject.FindProperty("_sets");
			
			// Load visualizer visibility state
			_showVisualizer = EditorPrefs.GetBool(VisualizerPrefsKey, false);
		}
		
		/// <summary>
		/// Public method to show the visualizer, used by menu items
		/// </summary>
		public static void ShowVisualizerForConfigs(UiConfigs configs)
		{
			if (configs == null) return;
			
			Selection.activeObject = configs;
			EditorPrefs.SetBool(VisualizerPrefsKey, true);
		}

		/// <inheritdoc />
		public override VisualElement CreateInspectorGUI()
		{
			var root = new VisualElement();
			root.style.paddingTop = 5;
			root.style.paddingBottom = 5;
			root.style.paddingLeft = 3;
			root.style.paddingRight = 3;

			// Section 0: Layer Visualizer (collapsible)
			var visualizerSection = CreateVisualizerSection();
			root.Add(visualizerSection);

			// Section 1: UI Config Explanation
			var configHelpBox = new HelpBox(UiConfigExplanation, HelpBoxMessageType.Info);
			configHelpBox.style.marginBottom = 10;
			root.Add(configHelpBox);

			// Section 2: UI Configs List
			var configsListView = CreateConfigsListView();
			root.Add(configsListView);

			// Section 3: Separator
			var separator = new VisualElement();
			separator.style.height = 1;
			separator.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
			separator.style.marginTop = 15;
			separator.style.marginBottom = 15;
			root.Add(separator);

			// Section 4: UI Set Explanation
			var setHelpBox = new HelpBox(UiSetExplanation, HelpBoxMessageType.Info);
			setHelpBox.style.marginBottom = 10;
			root.Add(setHelpBox);

			// Section 5: UI Sets List
			var setsContainer = CreateSetsContainer();
			root.Add(setsContainer);

			return root;
		}

		private ListView CreateConfigsListView()
		{
			var listView = new ListView
			{
				showBorder = true,
				showFoldoutHeader = true,
				headerTitle = "UI Presenter Configs",
				showAddRemoveFooter = false,
				showBoundCollectionSize = false,
				reorderable = false,
				virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
				fixedItemHeight = 22
			};

			listView.style.minHeight = 20;
			listView.style.marginBottom = 5;

			listView.BindProperty(_configsProperty);

			listView.makeItem = CreateConfigElement;
			listView.bindItem = BindConfigElement;

			return listView;
		}

		private void BindConfigElement(VisualElement element, int index)
		{
			if (index >= _configsProperty.arraySize)
				return;

			var itemProperty = _configsProperty.GetArrayElementAtIndex(index);
			var addressProperty = itemProperty.FindPropertyRelative(nameof(UiConfigs.UiConfigSerializable.AddressableAddress));
			var layerProperty = itemProperty.FindPropertyRelative(nameof(UiConfigs.UiConfigSerializable.Layer));

			var label = element.Q<Label>();
			var layerField = element.Q<IntegerField>();

			label.text = addressProperty.stringValue;
			
			// Unbind to remove previous event handlers
			layerField.Unbind();
			
			// Bind to property for automatic serialization
			layerField.BindProperty(layerProperty);
			
			// Register custom callback for prefab sync (using userData to store address for later)
			layerField.userData = addressProperty.stringValue;
			layerField.RegisterValueChangedCallback(OnLayerChanged);
		}

		private void OnLayerChanged(ChangeEvent<int> evt)
		{
			if (evt.newValue == evt.previousValue)
				return;

			var layerField = evt.target as IntegerField;
			if (layerField?.userData is string address)
			{
				// Sync with Canvas/UIDocument sorting order
				SyncLayerToPrefab(address, evt.newValue);
			}
		}

		private VisualElement CreateSetsContainer()
		{
			var container = new VisualElement();
			var enumNames = Enum.GetNames(typeof(TSet));

			for (int setIndex = 0; setIndex < enumNames.Length; setIndex++)
			{
				var setElement = CreateSetElement(enumNames[setIndex], setIndex);
				container.Add(setElement);
			}

			return container;
		}

		private VisualElement CreateConfigElement()
		{
			var container = new VisualElement();
			container.style.flexDirection = FlexDirection.Row;
			container.style.alignItems = Align.Center;
			container.style.paddingTop = 2;
			container.style.paddingBottom = 2;

			// UiPresenter Addressable Address
			var label = new Label();
			label.style.flexGrow = 1;
			label.style.paddingLeft = 5;
			label.style.unityTextAlign = TextAnchor.MiddleLeft;
			container.Add(label);

			// Layer field
			var layerField = new IntegerField();
			layerField.style.width = 80;
			layerField.style.marginRight = 5;
			container.Add(layerField);

			return container;
		}

		private VisualElement CreateSetPresenterElement()
		{
			var container = new VisualElement();
			container.style.flexDirection = FlexDirection.Row;
			container.style.alignItems = Align.Center;
			
			// Drag handle for reordering
			var dragHandle = new Label("☰");
			dragHandle.style.width = 20;
			dragHandle.style.unityTextAlign = TextAnchor.MiddleCenter;
			dragHandle.style.marginLeft = 3;
			dragHandle.style.marginRight = 5;
			dragHandle.style.fontSize = 16;
			dragHandle.style.color = new Color(0.7f, 0.7f, 0.7f);
			dragHandle.tooltip = "Drag to reorder";
			container.Add(dragHandle);
			
			// Dropdown for selecting UI presenter type
			var dropdown = new DropdownField();
			dropdown.choices = new List<string>(_uiConfigsAddress ?? new List<string>());
			dropdown.style.flexGrow = 1;
			dropdown.style.paddingTop = 3;
			dropdown.style.paddingBottom = 3;
			dropdown.name = "ui-type-dropdown";
			container.Add(dropdown);
			
			// Delete button
			var deleteButton = new Button { text = "×" };
			deleteButton.style.width = 25;
			deleteButton.style.height = 20;
			deleteButton.style.marginLeft = 5;
			deleteButton.style.marginRight = 3;
			deleteButton.style.fontSize = 18;
			deleteButton.style.unityFontStyleAndWeight = FontStyle.Bold;
			deleteButton.tooltip = "Remove from set";
			deleteButton.name = "delete-button";
			container.Add(deleteButton);
			
			return container;
		}

		private VisualElement CreateSetElement(string setName, int setIndex)
		{
			var setContainer = new VisualElement();
			setContainer.style.paddingLeft = 5;
			setContainer.style.paddingRight = 5;
			setContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.2f);
			setContainer.style.borderBottomLeftRadius = 4;
			setContainer.style.borderBottomRightRadius = 4;
			setContainer.style.borderTopLeftRadius = 4;
			setContainer.style.borderTopRightRadius = 4;

			// Header
			var header = new Label($"{setName} Set");
			header.style.unityFontStyleAndWeight = FontStyle.Bold;
			header.style.fontSize = 13;
			header.style.marginBottom = 5;
			setContainer.Add(header);

			// Get the property for this set's UI entries
			var setProperty = _setsProperty.GetArrayElementAtIndex(setIndex);
			var uiEntriesProperty = setProperty.FindPropertyRelative(nameof(UiSetConfigSerializable.UiEntries));

			// ListView for presenters in this set
			var presenterListView = new ListView
			{
				showBorder = true,
				showAddRemoveFooter = true,
				reorderable = true,
				showBoundCollectionSize = false,
				virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
				fixedItemHeight = 28
			};

			presenterListView.BindProperty(uiEntriesProperty);

			presenterListView.makeItem = CreateSetPresenterElement;
			presenterListView.bindItem = (element, index) => BindSetPresenterElement(element, index, uiEntriesProperty, presenterListView);
			
			// Register callbacks to save changes when items are added, removed, or reordered
			presenterListView.itemsAdded += indices => OnPresenterItemsAdded(indices, uiEntriesProperty);
			presenterListView.itemsRemoved += _ => SaveSetChanges();
			presenterListView.itemIndexChanged += (_, _) => SaveSetChanges();

			setContainer.Add(presenterListView);

			return setContainer;
		}

		private void BindSetPresenterElement(VisualElement element, int index, SerializedProperty uiEntriesProperty, ListView listView)
		{
			if (index >= uiEntriesProperty.arraySize)
				return;

			var dropdown = element.Q<DropdownField>("ui-type-dropdown");
			if (dropdown == null)
				return;

			var entryProperty = uiEntriesProperty.GetArrayElementAtIndex(index);
			var typeNameProperty = entryProperty.FindPropertyRelative(nameof(UiSetEntry.UiTypeName));
			var instanceAddressProperty = entryProperty.FindPropertyRelative(nameof(UiSetEntry.InstanceAddress));
			
			// Find the matching address for this type
			var currentTypeName = typeNameProperty.stringValue;
			Type currentType = string.IsNullOrEmpty(currentTypeName) ? null : Type.GetType(currentTypeName);
			
			// Find the address that matches this type
			string matchingAddress = null;
			if (currentType != null && _uiTypesByAddress != null)
			{
				foreach (var kvp in _uiTypesByAddress)
				{
					if (kvp.Value == currentType)
					{
						matchingAddress = kvp.Key;
						break;
					}
				}
			}
			
			var selectedIndex = string.IsNullOrEmpty(matchingAddress) ? 0 : 
				_uiConfigsAddress.FindIndex(address => address == matchingAddress);
			
			if (selectedIndex < 0) 
				selectedIndex = 0;

			if (_uiConfigsAddress != null && _uiConfigsAddress.Count > 0)
			{
				// Unbind to prevent stale property references
				dropdown.Unbind();
				
				// Set the current values
				dropdown.index = selectedIndex;
				
				// Register callback to store type and addressable address when changed
				dropdown.RegisterValueChangedCallback(_ =>
				{
					var newIndex = dropdown.index;
					if (newIndex >= 0 && newIndex < _uiConfigsAddress.Count)
					{
						var selectedAddress = _uiConfigsAddress[newIndex];
						if (_uiTypesByAddress.TryGetValue(selectedAddress, out var selectedType))
						{
							typeNameProperty.stringValue = selectedType.AssemblyQualifiedName;
							// Use the addressable address as the instance address
							instanceAddressProperty.stringValue = selectedAddress;
							SaveSetChanges();
						}
					}
				});
				
				// Set initial value if property is empty
				if (string.IsNullOrEmpty(typeNameProperty.stringValue) && selectedIndex < _uiConfigsAddress.Count)
				{
					var address = _uiConfigsAddress[selectedIndex];
					if (_uiTypesByAddress.TryGetValue(address, out var type))
					{
						typeNameProperty.stringValue = type.AssemblyQualifiedName;
						instanceAddressProperty.stringValue = address;
						serializedObject.ApplyModifiedProperties();
					}
				}
			}

			// Setup delete button to remove this item from the set
			var deleteButton = element.Q<Button>("delete-button");
			if (deleteButton != null)
			{
				// Store the click handler in userData to unregister it later if needed
				if (deleteButton.userData is EventCallback<ClickEvent> previousCallback)
				{
					deleteButton.UnregisterCallback(previousCallback);
				}

				EventCallback<ClickEvent> clickHandler = _ =>
				{
					uiEntriesProperty.DeleteArrayElementAtIndex(index);
					SaveSetChanges();
				};

				deleteButton.userData = clickHandler;
				deleteButton.RegisterCallback(clickHandler);
		}
	}

		private void OnPresenterItemsAdded(IEnumerable<int> indices, SerializedProperty uiEntriesProperty)
		{
			if (_uiConfigsAddress == null || _uiConfigsAddress.Count == 0 || _uiTypesByAddress == null)
			{
				return;
			}

			var defaultAddress = _uiConfigsAddress[0];
			Type defaultType = _uiTypesByAddress.TryGetValue(defaultAddress, out var type) ? type : null;
			
			foreach (var index in indices)
			{
				if (index < uiEntriesProperty.arraySize)
				{
					var entryProperty = uiEntriesProperty.GetArrayElementAtIndex(index);
					var typeNameProperty = entryProperty.FindPropertyRelative(nameof(UiSetEntry.UiTypeName));
					var instanceAddressProperty = entryProperty.FindPropertyRelative(nameof(UiSetEntry.InstanceAddress));
					
					typeNameProperty.stringValue = defaultType?.AssemblyQualifiedName ?? string.Empty;
					// Use the addressable address as the instance address
					instanceAddressProperty.stringValue = defaultAddress;
				}
			}
		
			SaveSetChanges();
		}

		private void SaveSetChanges()
		{
			serializedObject.ApplyModifiedProperties();
			
			if (_scriptableObject != null)
			{
				EditorUtility.SetDirty(_scriptableObject);
				AssetDatabase.SaveAssets();
			}
		}

		private void SyncConfigsWithAddressables()
		{
			var assetList = GetAssetList();
			var configs = new List<UiConfig>();
			var uiConfigsAddress = new List<string>();
			var assetPathLookup = new Dictionary<string, string>();
			var existingConfigs = _scriptableObject.Configs;

			foreach (var asset in assetList)
			{
				// Only process GameObjects
				if (AssetDatabase.GetMainAssetTypeAtPath(asset.AssetPath) != typeof(GameObject))
					continue;

				var uiPresenter = AssetDatabase.LoadAssetAtPath<UiPresenter>(asset.AssetPath);
				if (uiPresenter == null)
					continue;

				// Get sorting order from Canvas or UIDocument
				var sortingOrder = GetSortingOrder(uiPresenter);
				
				// Check if config already exists to preserve custom layer values
				var existingConfigIndex = existingConfigs.FindIndex(c => c.AddressableAddress == asset.address);
				var presenterType = uiPresenter.GetType();
				
				var config = new UiConfig
				{
					AddressableAddress = asset.address,
					Layer = existingConfigIndex >= 0 && sortingOrder < 0 ? existingConfigs[existingConfigIndex].Layer : 
					        sortingOrder < 0 ? 0 : sortingOrder,
					UiType = presenterType,
					LoadSynchronously = Attribute.IsDefined(presenterType, typeof(LoadSynchronouslyAttribute))
				};

				configs.Add(config);
				uiConfigsAddress.Add(asset.address);
				assetPathLookup[asset.address] = asset.AssetPath;
			}

			_scriptableObject.Configs = configs;
			_uiConfigsAddress = uiConfigsAddress;
			_assetPathLookup = assetPathLookup;
			
			// Build Type lookup dictionary
			_uiTypesByAddress = new Dictionary<string, Type>();
			foreach (var config in configs)
			{
				if (!string.IsNullOrEmpty(config.AddressableAddress) && config.UiType != null)
				{
					_uiTypesByAddress[config.AddressableAddress] = config.UiType;
				}
			}

			EditorUtility.SetDirty(_scriptableObject);
			AssetDatabase.SaveAssets();
		}

		private int GetSortingOrder(UiPresenter presenter)
		{
			if (presenter.TryGetComponent<Canvas>(out var canvas))
			{
				return canvas.sortingOrder;
			}
			
			if (presenter.TryGetComponent<UIDocument>(out var document))
			{
				return (int)document.sortingOrder;
			}
			
			return -1;
		}

		private void SyncLayerToPrefab(string address, int newLayer)
		{
			if (_assetPathLookup == null || !_assetPathLookup.TryGetValue(address, out var assetPath))
				return;

			var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
			if (prefab == null)
				return;

			bool changed = false;

			if (prefab.TryGetComponent<Canvas>(out var canvas))
			{
				canvas.sortingOrder = newLayer;
				changed = true;
			}
			else if (prefab.TryGetComponent<UIDocument>(out var document))
			{
				document.sortingOrder = newLayer;
				changed = true;
			}

			if (changed)
			{
				EditorUtility.SetDirty(prefab);
				AssetDatabase.SaveAssets();
			}
		}
		
		private VisualElement CreateVisualizerSection()
		{
			var section = new VisualElement();
			section.style.marginBottom = 15;
			
			// Create header with toggle button
			var header = new VisualElement();
			header.style.flexDirection = FlexDirection.Row;
			header.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
			header.style.paddingTop = 5;
			header.style.paddingBottom = 5;
			header.style.paddingLeft = 5;
			header.style.paddingRight = 5;
			header.style.borderTopLeftRadius = 4;
			header.style.borderTopRightRadius = 4;
			
			var toggleButton = new Button(() => ToggleVisualizer())
			{
				text = _showVisualizer ? "▼" : "▶"
			};
			toggleButton.style.width = 25;
			toggleButton.style.marginRight = 5;
			
			var titleLabel = new Label("UI Layer Hierarchy Visualizer");
			titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
			titleLabel.style.flexGrow = 1;
			
			header.Add(toggleButton);
			header.Add(titleLabel);
			section.Add(header);
			
			// Create visualizer container
			_visualizerContainer = new VisualElement();
			_visualizerContainer.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
			_visualizerContainer.style.paddingTop = 10;
			_visualizerContainer.style.paddingBottom = 10;
			_visualizerContainer.style.paddingLeft = 5;
			_visualizerContainer.style.paddingRight = 5;
			_visualizerContainer.style.borderBottomLeftRadius = 4;
			_visualizerContainer.style.borderBottomRightRadius = 4;
			_visualizerContainer.style.display = _showVisualizer ? DisplayStyle.Flex : DisplayStyle.None;
			
			// Add search toolbar
			var searchToolbar = CreateVisualizerSearchToolbar();
			_visualizerContainer.Add(searchToolbar);
			
			// Add visualizer content
			var content = CreateVisualizerContent();
			_visualizerContainer.Add(content);
			
			section.Add(_visualizerContainer);
			
			return section;
		}
		
		private void ToggleVisualizer()
		{
			_showVisualizer = !_showVisualizer;
			EditorPrefs.SetBool(VisualizerPrefsKey, _showVisualizer);
			
			if (_visualizerContainer != null)
			{
				_visualizerContainer.style.display = _showVisualizer ? DisplayStyle.Flex : DisplayStyle.None;
				
				// Update toggle button text
				var toggleButton = _visualizerContainer.parent.Q<Button>();
				if (toggleButton != null)
				{
					toggleButton.text = _showVisualizer ? "▼" : "▶";
				}
				
				// Refresh content if showing
				if (_showVisualizer)
				{
					RefreshVisualizerContent();
				}
			}
		}
		
		private VisualElement CreateVisualizerSearchToolbar()
		{
			var toolbar = new VisualElement();
			toolbar.style.flexDirection = FlexDirection.Row;
			toolbar.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
			toolbar.style.paddingTop = 3;
			toolbar.style.paddingBottom = 3;
			toolbar.style.paddingLeft = 5;
			toolbar.style.paddingRight = 5;
			toolbar.style.marginBottom = 10;
			toolbar.style.borderTopLeftRadius = 3;
			toolbar.style.borderTopRightRadius = 3;
			toolbar.style.borderBottomLeftRadius = 3;
			toolbar.style.borderBottomRightRadius = 3;
			
			var spacer = new VisualElement();
			spacer.style.flexGrow = 1;
			toolbar.Add(spacer);
			
			var searchLabel = new Label("Search:");
			searchLabel.style.marginRight = 5;
			searchLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
			toolbar.Add(searchLabel);
			
			var searchField = new ToolbarSearchField();
			searchField.style.width = 200;
			searchField.value = _visualizerSearchFilter;
			searchField.RegisterValueChangedCallback(evt =>
			{
				_visualizerSearchFilter = evt.newValue;
				RefreshVisualizerContent();
			});
			toolbar.Add(searchField);
			
			return toolbar;
		}
		
		private ScrollView CreateVisualizerContent()
		{
			var scrollView = new ScrollView();
			scrollView.style.maxHeight = 500;
			scrollView.style.marginTop = 5;
			
			BuildVisualizerLayerHierarchy(scrollView);
			
			return scrollView;
		}
		
		private void RefreshVisualizerContent()
		{
			if (_visualizerContainer == null)
				return;
				
			var scrollView = _visualizerContainer.Q<ScrollView>();
			if (scrollView != null)
			{
				scrollView.Clear();
				BuildVisualizerLayerHierarchy(scrollView);
			}
		}
		
		private void BuildVisualizerLayerHierarchy(ScrollView scrollView)
		{
			var configs = _scriptableObject?.Configs;
			if (configs == null || configs.Count == 0)
			{
				var infoBox = new HelpBox("No UI configurations found", HelpBoxMessageType.Info);
				scrollView.Add(infoBox);
				return;
			}
			
			// Filter configs
			var filteredConfigs = FilterVisualizerConfigs(configs);
			
			// Group by layer
			var layerGroups = filteredConfigs
				.GroupBy(c => c.Layer)
				.OrderBy(g => g.Key)
				.ToList();
			
			var layersContainer = new VisualElement();
			layersContainer.style.marginLeft = 5;
			layersContainer.style.marginRight = 5;
			
			foreach (var layerGroup in layerGroups)
			{
				var layerElement = CreateVisualizerLayerElement(layerGroup.Key, layerGroup.ToList());
				layersContainer.Add(layerElement);
			}
			
			scrollView.Add(layersContainer);
			
			// Statistics
			var statsElement = CreateVisualizerStatistics(filteredConfigs);
			scrollView.Add(statsElement);
		}
		
		private List<UiConfig> FilterVisualizerConfigs(List<UiConfig> configs)
		{
			if (string.IsNullOrEmpty(_visualizerSearchFilter))
			{
				return configs;
			}
			
			return configs.Where(c =>
				c.UiType.Name.ToLower().Contains(_visualizerSearchFilter.ToLower()) ||
				c.AddressableAddress.ToLower().Contains(_visualizerSearchFilter.ToLower())
			).ToList();
		}
		
		private VisualElement CreateVisualizerLayerElement(int layer, List<UiConfig> configs)
		{
			var layerColor = GetVisualizerLayerColor(layer);
			
			var container = new VisualElement();
			container.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
			container.style.borderTopLeftRadius = 4;
			container.style.borderTopRightRadius = 4;
			container.style.borderBottomLeftRadius = 4;
			container.style.borderBottomRightRadius = 4;
			container.style.marginBottom = 5;
			container.style.paddingTop = 5;
			container.style.paddingBottom = 5;
			
			// Layer header
			var header = new VisualElement();
			header.style.flexDirection = FlexDirection.Row;
			header.style.backgroundColor = layerColor;
			header.style.paddingTop = 5;
			header.style.paddingBottom = 5;
			header.style.paddingLeft = 10;
			header.style.paddingRight = 10;
			header.style.marginBottom = 5;
			header.style.alignItems = Align.Center;
			
			// Create text container with shadow for better readability
			var textContainer = new VisualElement();
			textContainer.style.flexDirection = FlexDirection.Row;
			textContainer.style.backgroundColor = new Color(0, 0, 0, 0.5f);
			textContainer.style.paddingLeft = 8;
			textContainer.style.paddingRight = 8;
			textContainer.style.paddingTop = 3;
			textContainer.style.paddingBottom = 3;
			textContainer.style.borderTopLeftRadius = 3;
			textContainer.style.borderTopRightRadius = 3;
			textContainer.style.borderBottomLeftRadius = 3;
			textContainer.style.borderBottomRightRadius = 3;
			
			var layerLabel = new Label($"Layer {layer}");
			layerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
			layerLabel.style.fontSize = 13;
			layerLabel.style.color = Color.white;
			layerLabel.style.textShadow = new TextShadow
			{
				offset = new Vector2(1, 1),
				blurRadius = 2,
				color = new Color(0, 0, 0, 0.8f)
			};
			textContainer.Add(layerLabel);
			
			var countLabel = new Label($"({configs.Count} UI{(configs.Count > 1 ? "s" : "")})");
			countLabel.style.fontSize = 11;
			countLabel.style.color = new Color(1f, 1f, 1f);
			countLabel.style.marginLeft = 5;
			countLabel.style.textShadow = new TextShadow
			{
				offset = new Vector2(1, 1),
				blurRadius = 2,
				color = new Color(0, 0, 0, 0.8f)
			};
			textContainer.Add(countLabel);
			
			header.Add(textContainer);
			container.Add(header);
			
			// UI items in this layer
			foreach (var config in configs)
			{
				var configItem = CreateVisualizerUiConfigItem(config);
				container.Add(configItem);
			}
			
			return container;
		}
		
		private VisualElement CreateVisualizerUiConfigItem(UiConfig config)
		{
			var item = new VisualElement();
			item.style.flexDirection = FlexDirection.Row;
			item.style.paddingLeft = 20;
			item.style.paddingRight = 10;
			item.style.paddingTop = 2;
			item.style.paddingBottom = 2;
			
			// Type name
			var typeLabel = new Label(config.UiType.Name);
			typeLabel.style.width = 200;
			item.Add(typeLabel);
			
			// Address
			var addressLabel = new Label(config.AddressableAddress);
			addressLabel.style.fontSize = 10;
			addressLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
			addressLabel.style.flexGrow = 1;
			item.Add(addressLabel);
			
			// Sync indicator
			if (config.LoadSynchronously)
			{
				var syncLabel = new Label("[SYNC]");
				syncLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
				syncLabel.style.width = 50;
				syncLabel.style.color = new Color(1, 0.8f, 0);
				item.Add(syncLabel);
			}
			
			return item;
		}
		
		private VisualElement CreateVisualizerStatistics(List<UiConfig> configs)
		{
			var container = new VisualElement();
			container.style.marginTop = 10;
			container.style.marginLeft = 5;
			container.style.marginBottom = 10;
			
			var titleLabel = new Label("Statistics");
			titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
			titleLabel.style.marginBottom = 5;
			container.Add(titleLabel);
			
			var statsContainer = new VisualElement();
			statsContainer.style.marginLeft = 15;
			
			statsContainer.Add(new Label($"Total UIs: {configs.Count}"));
			statsContainer.Add(new Label($"Layers Used: {configs.Select(c => c.Layer).Distinct().Count()}"));
			statsContainer.Add(new Label($"Synchronous Loads: {configs.Count(c => c.LoadSynchronously)}"));
			statsContainer.Add(new Label($"Async Loads: {configs.Count(c => !c.LoadSynchronously)}"));
			
			container.Add(statsContainer);
			
			return container;
		}
		
		private Color GetVisualizerLayerColor(int layer)
		{
			if (layer < 0) return new Color(0.8f, 0.2f, 0.2f);
			if (layer == 0) return new Color(0.3f, 0.3f, 0.3f);
			
			var hue = (layer * 0.1f) % 1f;
			return Color.HSVToRGB(hue, 0.5f, 0.9f);
		}
		
		private static List<AddressableAssetEntry> GetAssetList()
		{
			var assetList = new List<AddressableAssetEntry>();
			var assetsSettings = AddressableAssetSettingsDefaultObject.Settings;

			foreach (var settingsGroup in assetsSettings.groups)
			{
				if (settingsGroup.ReadOnly)
				{
					continue;
				}

				settingsGroup.GatherAllAssets(assetList, true, true, true);
			}

			return assetList;
		}
	}
}

