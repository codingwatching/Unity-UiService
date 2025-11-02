using System;
using System.Collections.Generic;
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
	public static class UiConfigsSelect
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
			"UI Sets group multiple presenters that should be displayed together. " +
			"When a set is activated via UiService, all its presenters are loaded and shown simultaneously. " +
			"Presenters are loaded in the order listed (top to bottom).";

		private Dictionary<string, string> _assetPathLookup;
		private List<string> _uiConfigsAddress;
		private UiConfigs _scriptableObject;
		private SerializedProperty _configsProperty;
		private SerializedProperty _setsProperty;

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
		}

		/// <inheritdoc />
		public override VisualElement CreateInspectorGUI()
		{
			var root = new VisualElement();
			root.style.paddingTop = 5;
			root.style.paddingBottom = 5;
			root.style.paddingLeft = 3;
			root.style.paddingRight = 3;

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

			var label = new Label();
			label.style.flexGrow = 1;
			label.style.paddingLeft = 5;
			label.style.unityTextAlign = TextAnchor.MiddleLeft;
			container.Add(label);

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
			
			// Dropdown for selecting UI presenter
			var dropdown = new DropdownField();
			dropdown.choices = new List<string>(_uiConfigsAddress ?? new List<string>());
			dropdown.style.flexGrow = 1;
			dropdown.style.paddingTop = 3;
			dropdown.style.paddingBottom = 3;
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

			// Get the property for this set's UI configs
			var setProperty = _setsProperty.GetArrayElementAtIndex(setIndex);
			var uiConfigsAddressProperty = setProperty.FindPropertyRelative(nameof(UiSetConfigSerializable.UiConfigsAddress));

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

			presenterListView.BindProperty(uiConfigsAddressProperty);

			presenterListView.makeItem = CreateSetPresenterElement;
			presenterListView.bindItem = (element, index) => BindSetPresenterElement(element, index, uiConfigsAddressProperty, presenterListView);
			
			// Register callbacks to save changes when items are added, removed, or reordered
			presenterListView.itemsAdded += indices => OnPresenterItemsAdded(indices, uiConfigsAddressProperty);
			presenterListView.itemsRemoved += _ => SaveSetChanges();
			presenterListView.itemIndexChanged += (_, _) => SaveSetChanges();

			setContainer.Add(presenterListView);

			return setContainer;
		}

		private void BindSetPresenterElement(VisualElement element, int index, SerializedProperty uiConfigsAddressProperty, ListView listView)
		{
			if (index >= uiConfigsAddressProperty.arraySize)
				return;

			var dropdown = element.Q<DropdownField>();
			if (dropdown == null)
				return;

			var itemProperty = uiConfigsAddressProperty.GetArrayElementAtIndex(index);
			
			// Find the index in our address list
			var currentAddress = itemProperty.stringValue;
			var selectedIndex = string.IsNullOrEmpty(currentAddress) ? 0 : 
				_uiConfigsAddress.FindIndex(address => address == currentAddress);
			
			if (selectedIndex < 0) 
				selectedIndex = 0;

			if (_uiConfigsAddress != null && _uiConfigsAddress.Count > 0)
			{
				// Unbind to prevent stale property references
				dropdown.Unbind();
				
				// Set the current value
				dropdown.index = selectedIndex;
				
				// Register callback to store address when changed
				dropdown.RegisterValueChangedCallback(evt =>
				{
					var newIndex = dropdown.index;
					if (newIndex >= 0 && newIndex < _uiConfigsAddress.Count)
					{
						itemProperty.stringValue = _uiConfigsAddress[newIndex];
						SaveSetChanges();
					}
				});
				
				// Set initial value if property is empty
				if (string.IsNullOrEmpty(itemProperty.stringValue) && selectedIndex < _uiConfigsAddress.Count)
				{
					itemProperty.stringValue = _uiConfigsAddress[selectedIndex];
					serializedObject.ApplyModifiedProperties();
				}
			}
		}

		private void OnPresenterItemsAdded(IEnumerable<int> indices, SerializedProperty uiConfigsAddressProperty)
		{
			if (_uiConfigsAddress == null || _uiConfigsAddress.Count == 0)
			{
				return;
			}

			var defaultAddress = _uiConfigsAddress[0];
			
			foreach (var index in indices)
			{
				if (index < uiConfigsAddressProperty.arraySize)
				{
					var itemProperty = uiConfigsAddressProperty.GetArrayElementAtIndex(index);
					itemProperty.stringValue = defaultAddress;
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

