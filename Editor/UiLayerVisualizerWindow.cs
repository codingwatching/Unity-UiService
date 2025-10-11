using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using GameLovers.UiService;

// ReSharper disable once CheckNamespace

namespace GameLoversEditor.UiService
{
	/// <summary>
	/// Editor window for visualizing UI layer hierarchy
	/// </summary>
	public class UiLayerVisualizerWindow : EditorWindow
	{
		private Vector2 _scrollPosition;
		private UiConfigs _selectedConfigs;
		private bool _showLoadingSpinner = true;
		private bool _showLayers = true;
		private string _searchFilter = "";

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

		private void OnGUI()
		{
			DrawHeader();
			DrawToolbar();
			
			if (_selectedConfigs == null)
			{
				DrawNoConfigsMessage();
				return;
			}

			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
			
			if (_showLoadingSpinner)
			{
				DrawLoadingSpinner();
			}
			
			if (_showLayers)
			{
				DrawLayerHierarchy();
			}
			
			EditorGUILayout.EndScrollView();
		}

		private void DrawHeader()
		{
			EditorGUILayout.Space(5);
			
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			EditorGUILayout.LabelField("UI Layer Hierarchy Visualizer", EditorStyles.boldLabel);
			
			if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
			{
				AutoFindUiConfigs();
			}
			
			EditorGUILayout.EndHorizontal();
			
			// Config selector
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("UI Configs:", GUILayout.Width(80));
			var newConfigs = (UiConfigs)EditorGUILayout.ObjectField(_selectedConfigs, typeof(UiConfigs), false);
			if (newConfigs != _selectedConfigs)
			{
				_selectedConfigs = newConfigs;
			}
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.Space(5);
		}

		private void DrawToolbar()
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			
			_showLoadingSpinner = GUILayout.Toggle(_showLoadingSpinner, "Loading Spinner", EditorStyles.toolbarButton);
			_showLayers = GUILayout.Toggle(_showLayers, "Layer Hierarchy", EditorStyles.toolbarButton);
			
			GUILayout.FlexibleSpace();
			
			EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
			_searchFilter = EditorGUILayout.TextField(_searchFilter, EditorStyles.toolbarSearchField, GUILayout.Width(200));
			
			EditorGUILayout.EndHorizontal();
		}

		private void DrawNoConfigsMessage()
		{
			EditorGUILayout.HelpBox("No UiConfigs asset found. Please create one or assign it above.", MessageType.Warning);
			
			if (GUILayout.Button("Create New UiConfigs"))
			{
				CreateNewUiConfigs();
			}
		}

		private void DrawLoadingSpinner()
		{
			EditorGUILayout.LabelField("Loading Spinner", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			
			var spinnerType = _selectedConfigs.LoadingSpinnerType;
			if (spinnerType != null)
			{
				EditorGUILayout.LabelField("Type:", spinnerType.Name);
				EditorGUILayout.LabelField("Full Name:", spinnerType.FullName);
			}
			else
			{
				EditorGUILayout.HelpBox("No loading spinner configured", MessageType.Info);
			}
			
			EditorGUI.indentLevel--;
			EditorGUILayout.Space(10);
		}

		private void DrawLayerHierarchy()
		{
			EditorGUILayout.LabelField("Layer Hierarchy", EditorStyles.boldLabel);
			
			var configs = _selectedConfigs.Configs;
			if (configs == null || configs.Count == 0)
			{
				EditorGUILayout.HelpBox("No UI configurations found", MessageType.Info);
				return;
			}

			// Filter configs
			var filteredConfigs = FilterConfigs(configs);
			
			// Group by layer
			var layerGroups = filteredConfigs
				.GroupBy(c => c.Layer)
				.OrderBy(g => g.Key)
				.ToList();

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			
			foreach (var layerGroup in layerGroups)
			{
				DrawLayer(layerGroup.Key, layerGroup.ToList());
			}
			
			EditorGUILayout.EndVertical();
			
			// Statistics
			EditorGUILayout.Space(10);
			DrawStatistics(filteredConfigs);
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

		private void DrawLayer(int layer, List<UiConfig> configs)
		{
			var layerColor = GetLayerColor(layer);
			var backgroundColor = GUI.backgroundColor;
			
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			
			// Layer header
			GUI.backgroundColor = layerColor;
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			EditorGUILayout.LabelField($"Layer {layer}", EditorStyles.whiteLargeLabel, GUILayout.Width(100));
			EditorGUILayout.LabelField($"({configs.Count} UI{(configs.Count > 1 ? "s" : "")})", EditorStyles.miniLabel);
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			GUI.backgroundColor = backgroundColor;

			// UI items in this layer
			EditorGUI.indentLevel++;
			foreach (var config in configs)
			{
				DrawUiConfigItem(config);
			}
			EditorGUI.indentLevel--;
			
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space(5);
		}

		private void DrawUiConfigItem(UiConfig config)
		{
			EditorGUILayout.BeginHorizontal();
			
			// Type name
			EditorGUILayout.LabelField(config.UiType.Name, GUILayout.Width(200));
			
			// Address
			EditorGUILayout.LabelField(config.AddressableAddress, EditorStyles.miniLabel);
			
			// Sync indicator
			if (config.LoadSynchronously)
			{
				EditorGUILayout.LabelField("[SYNC]", EditorStyles.boldLabel, GUILayout.Width(50));
			}
			
			EditorGUILayout.EndHorizontal();
		}

		private void DrawStatistics(List<UiConfig> configs)
		{
			EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
			
			EditorGUI.indentLevel++;
			EditorGUILayout.LabelField($"Total UIs: {configs.Count}");
			EditorGUILayout.LabelField($"Layers Used: {configs.Select(c => c.Layer).Distinct().Count()}");
			EditorGUILayout.LabelField($"Synchronous Loads: {configs.Count(c => c.LoadSynchronously)}");
			EditorGUILayout.LabelField($"Async Loads: {configs.Count(c => !c.LoadSynchronously)}");
			EditorGUI.indentLevel--;
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

