using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using GameLovers.UiService;

// ReSharper disable once CheckNamespace

namespace GameLoversEditor.UiService
{
	/// <summary>
	/// Editor window displaying active UI service hierarchy with quick controls
	/// </summary>
	public class UiServiceHierarchyWindow : EditorWindow
	{
		private Vector2 _scrollPosition;
		private bool _autoRefresh = true;
		private double _lastRefreshTime;
		private const double RefreshInterval = 0.5; // seconds
		
		private UiPresenter[] _activePresenters;
		private Dictionary<Type, bool> _foldoutStates = new Dictionary<Type, bool>();

		[MenuItem("Tools/UI Service/Hierarchy Window")]
		public static void ShowWindow()
		{
			var window = GetWindow<UiServiceHierarchyWindow>("UI Service Hierarchy");
			window.minSize = new Vector2(350, 200);
			window.Show();
		}

		private void OnEnable()
		{
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			RefreshPresenters();
		}

		private void OnDisable()
		{
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		}

		private void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			RefreshPresenters();
		}

		private void OnGUI()
		{
			DrawHeader();
			
			if (!Application.isPlaying)
			{
				DrawNotPlayingMessage();
				return;
			}

			if (_autoRefresh && EditorApplication.timeSinceStartup - _lastRefreshTime > RefreshInterval)
			{
				RefreshPresenters();
				_lastRefreshTime = EditorApplication.timeSinceStartup;
				Repaint();
			}

			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
			
			if (_activePresenters == null || _activePresenters.Length == 0)
			{
				DrawNoPresentersMessage();
			}
			else
			{
				DrawPresenterHierarchy();
			}
			
			EditorGUILayout.EndScrollView();
			
			DrawFooter();
		}

		private void DrawHeader()
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			EditorGUILayout.LabelField("Active UI Hierarchy", EditorStyles.boldLabel);
			
			GUILayout.FlexibleSpace();
			
			_autoRefresh = GUILayout.Toggle(_autoRefresh, "Auto Refresh", EditorStyles.toolbarButton, GUILayout.Width(100));
			
			if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
			{
				RefreshPresenters();
			}
			
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space(5);
		}

		private void DrawNotPlayingMessage()
		{
			EditorGUILayout.HelpBox(
				"UI Service Hierarchy is only available in Play Mode.\n\n" +
				"Enter Play Mode to see active UI presenters and control them.",
				MessageType.Info);
		}

		private void DrawNoPresentersMessage()
		{
			EditorGUILayout.HelpBox("No active UI presenters found in the scene.", MessageType.Info);
		}

		private void DrawPresenterHierarchy()
		{
			EditorGUILayout.LabelField($"Total Presenters: {_activePresenters.Length}", EditorStyles.boldLabel);
			EditorGUILayout.Space(5);
			
			// Group by parent (layer)
			var layerGroups = _activePresenters
				.GroupBy(p => p.transform.parent != null ? p.transform.parent.gameObject : null)
				.OrderBy(g => g.Key != null ? g.Key.name : "Root");

			foreach (var group in layerGroups)
			{
				DrawLayerGroup(group.Key, group.ToArray());
			}
		}

		private void DrawLayerGroup(GameObject parent, UiPresenter[] presenters)
		{
			var layerName = parent != null ? parent.name : "Root";
			
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			
			// Layer header
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField($"📁 {layerName}", EditorStyles.boldLabel);
			EditorGUILayout.LabelField($"({presenters.Length})", EditorStyles.miniLabel, GUILayout.Width(30));
			EditorGUILayout.EndHorizontal();
			
			EditorGUI.indentLevel++;
			
			foreach (var presenter in presenters)
			{
				DrawPresenterItem(presenter);
			}
			
			EditorGUI.indentLevel--;
			
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space(5);
		}

		private void DrawPresenterItem(UiPresenter presenter)
		{
			var type = presenter.GetType();
			
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			
			// Header with foldout
			EditorGUILayout.BeginHorizontal();
			
			if (!_foldoutStates.ContainsKey(type))
			{
				_foldoutStates[type] = false;
			}
			
			_foldoutStates[type] = EditorGUILayout.Foldout(_foldoutStates[type], "", true);
			
			// Status indicator
			var statusColor = presenter.IsOpen ? "🟢" : "🔴";
			EditorGUILayout.LabelField(statusColor, GUILayout.Width(20));
			
			// Presenter name
			EditorGUILayout.LabelField(type.Name, EditorStyles.boldLabel);
			
			GUILayout.FlexibleSpace();
			
			// Quick open/close button
			var buttonLabel = presenter.IsOpen ? "Close" : "Open";
			if (GUILayout.Button(buttonLabel, EditorStyles.miniButton, GUILayout.Width(50)))
			{
				presenter.gameObject.SetActive(!presenter.IsOpen);
			}
			
			EditorGUILayout.EndHorizontal();
			
			// Expanded details
			if (_foldoutStates[type])
			{
				EditorGUI.indentLevel++;
				
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Type:", GUILayout.Width(80));
				EditorGUILayout.LabelField(type.Name);
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Status:", GUILayout.Width(80));
				EditorGUILayout.LabelField(presenter.IsOpen ? "Open" : "Closed");
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("GameObject:", GUILayout.Width(80));
				EditorGUILayout.ObjectField(presenter.gameObject, typeof(GameObject), true);
				EditorGUILayout.EndHorizontal();
				
				// Action buttons
				EditorGUILayout.BeginHorizontal();
				
				if (GUILayout.Button("Select in Hierarchy"))
				{
					Selection.activeGameObject = presenter.gameObject;
					EditorGUIUtility.PingObject(presenter.gameObject);
				}
				
				if (GUILayout.Button("Close & Destroy"))
				{
					if (EditorUtility.DisplayDialog("Destroy UI",
						$"Are you sure you want to destroy {type.Name}?", "Yes", "Cancel"))
					{
						DestroyImmediate(presenter.gameObject);
						RefreshPresenters();
					}
				}
				
				EditorGUILayout.EndHorizontal();
				
				EditorGUI.indentLevel--;
			}
			
			EditorGUILayout.EndVertical();
		}

		private void DrawFooter()
		{
			EditorGUILayout.Space(5);
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			
			if (_activePresenters != null)
			{
				var openCount = _activePresenters.Count(p => p.IsOpen);
				EditorGUILayout.LabelField($"Open: {openCount} | Closed: {_activePresenters.Length - openCount}");
			}
			
			GUILayout.FlexibleSpace();
			
			if (GUILayout.Button("Close All", EditorStyles.toolbarButton))
			{
				CloseAllPresenters();
			}
			
			EditorGUILayout.EndHorizontal();
		}

		private void RefreshPresenters()
		{
			_activePresenters = FindObjectsOfType<UiPresenter>();
		}

		private void CloseAllPresenters()
		{
			if (_activePresenters == null) return;
			
			foreach (var presenter in _activePresenters)
			{
				if (presenter.IsOpen)
				{
					presenter.gameObject.SetActive(false);
				}
			}
		}
	}
}

