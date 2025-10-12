using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using GameLovers.UiService;

// ReSharper disable once CheckNamespace

namespace GameLoversEditor.UiService
{
	/// <summary>
	/// Editor window for visualizing UI layer hierarchy
	/// </summary>
	public class UiLayerVisualizerWindow : EditorWindow
	{
		private UiConfigs _selectedConfigs;
		private string _searchFilter = "";
		
		private ScrollView _scrollView;
		private ObjectField _configsField;
		private ToolbarSearchField _searchField;

		[MenuItem("Tools/UI Service/Layer Visualizer")]
		public static void ShowWindow()
		{
			var window = GetWindow<UiLayerVisualizerWindow>("UI Layer Visualizer");
			window.minSize = new Vector2(400, 300);
			window.Show();
		}

		private void OnEnable()
		{
			// Try to find UiConfigs asset
			AutoFindUiConfigs();
		}

		private void CreateGUI()
		{
			var root = rootVisualElement;
			root.Clear();
			
			// Header
			var header = CreateHeader();
			root.Add(header);
			
			// Toolbar
			var toolbar = CreateToolbar();
			root.Add(toolbar);
			
			// Scroll view
			_scrollView = new ScrollView();
			_scrollView.style.flexGrow = 1;
			root.Add(_scrollView);
			
			// Update content
			UpdateContent();
		}

		private VisualElement CreateHeader()
		{
			var headerContainer = new VisualElement();
			headerContainer.style.marginBottom = 5;
			
			// Title bar
			var titleBar = new VisualElement();
			titleBar.style.flexDirection = FlexDirection.Row;
			titleBar.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
			titleBar.style.paddingTop = 5;
			titleBar.style.paddingBottom = 5;
			titleBar.style.paddingLeft = 5;
			titleBar.style.paddingRight = 5;
			
			var titleLabel = new Label("UI Layer Hierarchy Visualizer");
			titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
			titleLabel.style.flexGrow = 1;
			titleBar.Add(titleLabel);
			
			var refreshButton = new Button(() =>
			{
				AutoFindUiConfigs();
				UpdateContent();
			}) { text = "Refresh" };
			refreshButton.style.width = 60;
			titleBar.Add(refreshButton);
			
			headerContainer.Add(titleBar);
			
			// Config selector
			var configRow = new VisualElement();
			configRow.style.flexDirection = FlexDirection.Row;
			configRow.style.paddingTop = 5;
			configRow.style.paddingBottom = 5;
			configRow.style.paddingLeft = 5;
			configRow.style.alignItems = Align.Center;
			
			var configLabel = new Label("UI Configs:");
			configLabel.style.width = 80;
			configRow.Add(configLabel);
			
			_configsField = new ObjectField();
			_configsField.objectType = typeof(UiConfigs);
			_configsField.allowSceneObjects = false;
			_configsField.value = _selectedConfigs;
			_configsField.style.flexGrow = 1;
			_configsField.RegisterValueChangedCallback(evt =>
			{
				_selectedConfigs = evt.newValue as UiConfigs;
				UpdateContent();
			});
			configRow.Add(_configsField);
			
			headerContainer.Add(configRow);
			
			return headerContainer;
		}

		private VisualElement CreateToolbar()
		{
			var toolbar = new VisualElement();
			toolbar.style.flexDirection = FlexDirection.Row;
			toolbar.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);
			toolbar.style.paddingTop = 3;
			toolbar.style.paddingBottom = 3;
			toolbar.style.paddingLeft = 5;
			toolbar.style.paddingRight = 5;
			toolbar.style.marginBottom = 5;
			
			var spacer = new VisualElement();
			spacer.style.flexGrow = 1;
			toolbar.Add(spacer);
			
			var searchLabel = new Label("Search:");
			searchLabel.style.marginRight = 5;
			searchLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
			toolbar.Add(searchLabel);
			
			_searchField = new ToolbarSearchField();
			_searchField.style.width = 200;
			_searchField.value = _searchFilter;
			_searchField.RegisterValueChangedCallback(evt =>
			{
				_searchFilter = evt.newValue;
				UpdateContent();
			});
			toolbar.Add(_searchField);
			
			return toolbar;
		}

		private void UpdateContent()
		{
			if (_scrollView == null)
				return;
			
			_scrollView.Clear();
			
			if (_selectedConfigs == null)
			{
				var warningBox = new HelpBox("No UiConfigs asset found. Please create one or assign it above.", HelpBoxMessageType.Warning);
				_scrollView.Add(warningBox);
				
				var createButton = new Button(() => CreateNewUiConfigs()) { text = "Create New UiConfigs" };
				createButton.style.marginLeft = 5;
				createButton.style.marginRight = 5;
				createButton.style.marginTop = 5;
				_scrollView.Add(createButton);
				return;
			}

			BuildLayerHierarchy();
		}

		private void BuildLayerHierarchy()
		{
			var titleLabel = new Label("Layer Hierarchy");
			titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
			titleLabel.style.marginLeft = 5;
			titleLabel.style.marginTop = 5;
			titleLabel.style.marginBottom = 5;
			_scrollView.Add(titleLabel);
			
			var configs = _selectedConfigs.Configs;
			if (configs == null || configs.Count == 0)
			{
				var infoBox = new HelpBox("No UI configurations found", HelpBoxMessageType.Info);
				_scrollView.Add(infoBox);
				return;
			}

			// Filter configs
			var filteredConfigs = FilterConfigs(configs);
			
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
				var layerElement = CreateLayerElement(layerGroup.Key, layerGroup.ToList());
				layersContainer.Add(layerElement);
			}
			
			_scrollView.Add(layersContainer);
			
			// Statistics
			var statsElement = CreateStatistics(filteredConfigs);
			_scrollView.Add(statsElement);
		}

		private List<UiConfig> FilterConfigs(List<UiConfig> configs)
		{
			if (string.IsNullOrEmpty(_searchFilter))
			{
				return configs;
			}

			return configs.Where(c => 
				c.UiType.Name.ToLower().Contains(_searchFilter.ToLower()) ||
				c.AddressableAddress.ToLower().Contains(_searchFilter.ToLower())
			).ToList();
		}

		private VisualElement CreateLayerElement(int layer, List<UiConfig> configs)
		{
			var layerColor = GetLayerColor(layer);
			
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
			textContainer.style.backgroundColor = new Color(0, 0, 0, 0.5f); // Semi-transparent black background
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
				var configItem = CreateUiConfigItem(config);
				container.Add(configItem);
			}
			
			return container;
		}

		private VisualElement CreateUiConfigItem(UiConfig config)
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

		private VisualElement CreateStatistics(List<UiConfig> configs)
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

		private void AutoFindUiConfigs()
		{
			var assets = AssetDatabase.FindAssets($"t:{nameof(UiConfigs)}");
			if (assets.Length > 0)
			{
				_selectedConfigs = AssetDatabase.LoadAssetAtPath<UiConfigs>(AssetDatabase.GUIDToAssetPath(assets[0]));
			}
		}

		private void CreateNewUiConfigs()
		{
			var config = CreateInstance<UiConfigs>();
			AssetDatabase.CreateAsset(config, $"Assets/{nameof(UiConfigs)}.asset");
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			_selectedConfigs = config;
		}

		private Color GetLayerColor(int layer)
		{
			if (layer < 0) return new Color(0.8f, 0.2f, 0.2f);
			if (layer == 0) return new Color(0.3f, 0.3f, 0.3f);
			
			var hue = (layer * 0.1f) % 1f;
			return Color.HSVToRGB(hue, 0.5f, 0.9f);
		}
	}
}

