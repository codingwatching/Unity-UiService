using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using GameLovers.UiService;

// ReSharper disable once CheckNamespace

namespace GameLoversEditor.UiService
{
	/// <summary>
	/// Editor window displaying active UI service hierarchy with quick controls
	/// </summary>
	public class UiServiceHierarchyWindow : EditorWindow
	{
		private bool _autoRefresh = true;
		private double _lastRefreshTime;
		private const double RefreshInterval = 0.5; // seconds
		
		private UiPresenter[] _activePresenters;
		private Dictionary<Type, bool> _foldoutStates = new Dictionary<Type, bool>();
		
		private ScrollView _scrollView;
		private Label _footerLabel;

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
			if (rootVisualElement != null)
			{
				UpdateContent();
			}
		}

		private void CreateGUI()
		{
			var root = rootVisualElement;
			root.Clear();
			
			// Header
			var header = CreateHeader();
			root.Add(header);
			
			// Scroll view
			_scrollView = new ScrollView();
			_scrollView.style.flexGrow = 1;
			root.Add(_scrollView);
			
			// Footer
			var footer = CreateFooter();
			root.Add(footer);
			
			// Update content
			UpdateContent();
			
			// Schedule periodic updates
			root.schedule.Execute(() =>
			{
				if (_autoRefresh && Application.isPlaying && EditorApplication.timeSinceStartup - _lastRefreshTime > RefreshInterval)
				{
					RefreshPresenters();
					_lastRefreshTime = EditorApplication.timeSinceStartup;
					UpdateContent();
				}
			}).Every(100);
		}

		private VisualElement CreateHeader()
		{
			var header = new VisualElement();
			header.style.flexDirection = FlexDirection.Row;
			header.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
			header.style.paddingTop = 5;
			header.style.paddingBottom = 5;
			header.style.paddingLeft = 5;
			header.style.paddingRight = 5;
			header.style.marginBottom = 5;
			
			var titleLabel = new Label("Active UI Hierarchy");
			titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
			titleLabel.style.flexGrow = 1;
			header.Add(titleLabel);
			
			// Auto refresh toggle
			var autoRefreshToggle = new Toggle("Auto Refresh") { value = _autoRefresh };
			autoRefreshToggle.RegisterValueChangedCallback(evt => _autoRefresh = evt.newValue);
			autoRefreshToggle.style.width = 110;
			header.Add(autoRefreshToggle);
			
			// Refresh button
			var refreshButton = new Button(() =>
			{
				RefreshPresenters();
				UpdateContent();
			}) { text = "Refresh" };
			refreshButton.style.width = 60;
			refreshButton.style.marginLeft = 5;
			header.Add(refreshButton);
			
			return header;
		}

		private void UpdateContent()
		{
			if (_scrollView == null)
				return;
			
			_scrollView.Clear();
			
			if (!Application.isPlaying)
			{
				var helpBox = new HelpBox(
					"UI Service Hierarchy is only available in Play Mode.\n\n" +
					"Enter Play Mode to see active UI presenters and control them.",
					HelpBoxMessageType.Info);
				_scrollView.Add(helpBox);
				UpdateFooter();
				return;
			}

			if (_activePresenters == null || _activePresenters.Length == 0)
			{
				var infoBox = new HelpBox("No active UI presenters found in the scene.", HelpBoxMessageType.Info);
				_scrollView.Add(infoBox);
				UpdateFooter();
				return;
			}

			BuildPresenterHierarchy();
			UpdateFooter();
		}

		private void BuildPresenterHierarchy()
		{
			var titleLabel = new Label($"Total Presenters: {_activePresenters.Length}");
			titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
			titleLabel.style.marginLeft = 5;
			titleLabel.style.marginTop = 5;
			titleLabel.style.marginBottom = 5;
			_scrollView.Add(titleLabel);
			
			// Group by parent (layer)
			var layerGroups = _activePresenters
				.GroupBy(p => p.transform.parent != null ? p.transform.parent.gameObject : null)
				.OrderBy(g => g.Key != null ? g.Key.name : "Root");

			foreach (var group in layerGroups)
			{
				var layerElement = CreateLayerGroup(group.Key, group.ToArray());
				_scrollView.Add(layerElement);
			}
		}

		private VisualElement CreateLayerGroup(GameObject parent, UiPresenter[] presenters)
		{
			var layerName = parent != null ? parent.name : "Root";
			
			var container = new VisualElement();
			container.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
			container.style.borderTopLeftRadius = 4;
			container.style.borderTopRightRadius = 4;
			container.style.borderBottomLeftRadius = 4;
			container.style.borderBottomRightRadius = 4;
			container.style.marginLeft = 5;
			container.style.marginRight = 5;
			container.style.marginBottom = 5;
			container.style.paddingTop = 5;
			container.style.paddingBottom = 5;
			
			// Layer header
			var header = new VisualElement();
			header.style.flexDirection = FlexDirection.Row;
			header.style.paddingLeft = 10;
			header.style.paddingRight = 10;
			header.style.marginBottom = 5;
			
			// Folder icon (using VisualElement as colored box)
			var folderIcon = new VisualElement();
			folderIcon.style.width = 16;
			folderIcon.style.height = 16;
			folderIcon.style.backgroundColor = new Color(0.8f, 0.7f, 0.3f);
			folderIcon.style.borderTopLeftRadius = 2;
			folderIcon.style.borderTopRightRadius = 2;
			folderIcon.style.borderBottomLeftRadius = 2;
			folderIcon.style.borderBottomRightRadius = 2;
			folderIcon.style.marginRight = 5;
			header.Add(folderIcon);
			
			var layerLabel = new Label(layerName);
			layerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
			header.Add(layerLabel);
			
			var countLabel = new Label($"({presenters.Length})");
			countLabel.style.fontSize = 11;
			countLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
			countLabel.style.marginLeft = 5;
			header.Add(countLabel);
			
			container.Add(header);
			
			// Presenters
			foreach (var presenter in presenters)
			{
				var presenterElement = CreatePresenterItem(presenter);
				container.Add(presenterElement);
			}
			
			return container;
		}

		private VisualElement CreatePresenterItem(UiPresenter presenter)
		{
			var type = presenter.GetType();
			
			if (!_foldoutStates.ContainsKey(type))
			{
				_foldoutStates[type] = false;
			}
			
			var container = new VisualElement();
			container.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.5f);
			container.style.borderTopLeftRadius = 3;
			container.style.borderTopRightRadius = 3;
			container.style.borderBottomLeftRadius = 3;
			container.style.borderBottomRightRadius = 3;
			container.style.marginLeft = 10;
			container.style.marginRight = 10;
			container.style.marginBottom = 3;
			container.style.paddingTop = 5;
			container.style.paddingBottom = 5;
			container.style.paddingLeft = 5;
			container.style.paddingRight = 5;
			
			// Header with foldout
			var headerRow = new VisualElement();
			headerRow.style.flexDirection = FlexDirection.Row;
			headerRow.style.alignItems = Align.Center;
			
			var foldout = new Foldout { value = _foldoutStates[type], text = "" };
			foldout.RegisterValueChangedCallback(evt => _foldoutStates[type] = evt.newValue);
			foldout.style.width = 20;
			foldout.style.marginRight = 5;
			headerRow.Add(foldout);
			
			// Status indicator (colored circle)
			var statusIndicator = new VisualElement();
			statusIndicator.style.width = 12;
			statusIndicator.style.height = 12;
			statusIndicator.style.borderTopLeftRadius = 6;
			statusIndicator.style.borderTopRightRadius = 6;
			statusIndicator.style.borderBottomLeftRadius = 6;
			statusIndicator.style.borderBottomRightRadius = 6;
			statusIndicator.style.backgroundColor = presenter.IsOpen ? new Color(0, 1, 0) : new Color(1, 0, 0);
			statusIndicator.style.marginRight = 5;
			headerRow.Add(statusIndicator);
			
			// Presenter name
			var nameLabel = new Label(type.Name);
			nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
			nameLabel.style.flexGrow = 1;
			headerRow.Add(nameLabel);
			
			// Quick open/close button
			var toggleButton = new Button(() =>
			{
				presenter.gameObject.SetActive(!presenter.IsOpen);
				UpdateContent();
			}) { text = presenter.IsOpen ? "Close" : "Open" };
			toggleButton.style.width = 50;
			headerRow.Add(toggleButton);
			
			container.Add(headerRow);
			
			// Expanded details
			if (_foldoutStates[type])
			{
				var detailsContainer = new VisualElement();
				detailsContainer.style.marginLeft = 25;
				detailsContainer.style.marginTop = 5;
				
				detailsContainer.Add(CreateDetailRow("Type:", type.Name));
				detailsContainer.Add(CreateDetailRow("Status:", presenter.IsOpen ? "Open" : "Closed"));
				
				// GameObject field
				var gameObjectRow = new VisualElement();
				gameObjectRow.style.flexDirection = FlexDirection.Row;
				gameObjectRow.style.marginBottom = 2;
				
				var goLabel = new Label("GameObject:");
				goLabel.style.width = 90;
				gameObjectRow.Add(goLabel);
				
				var objectField = new UnityEditor.UIElements.ObjectField();
				objectField.objectType = typeof(GameObject);
				objectField.value = presenter.gameObject;
				objectField.SetEnabled(false);
				objectField.style.flexGrow = 1;
				gameObjectRow.Add(objectField);
				
				detailsContainer.Add(gameObjectRow);
				
				// Action buttons
				var buttonsRow = new VisualElement();
				buttonsRow.style.flexDirection = FlexDirection.Row;
				buttonsRow.style.marginTop = 5;
				
				var selectButton = new Button(() =>
				{
					Selection.activeGameObject = presenter.gameObject;
					EditorGUIUtility.PingObject(presenter.gameObject);
				}) { text = "Select in Hierarchy" };
				selectButton.style.flexGrow = 1;
				buttonsRow.Add(selectButton);
				
				var destroyButton = new Button(() =>
				{
					if (EditorUtility.DisplayDialog("Destroy UI",
						$"Are you sure you want to destroy {type.Name}?", "Yes", "Cancel"))
					{
						DestroyImmediate(presenter.gameObject);
						RefreshPresenters();
						UpdateContent();
					}
				}) { text = "Close & Destroy" };
				destroyButton.style.flexGrow = 1;
				destroyButton.style.marginLeft = 5;
				buttonsRow.Add(destroyButton);
				
				detailsContainer.Add(buttonsRow);
				
				container.Add(detailsContainer);
			}
			
			return container;
		}

		private VisualElement CreateDetailRow(string label, string value)
		{
			var row = new VisualElement();
			row.style.flexDirection = FlexDirection.Row;
			row.style.marginBottom = 2;
			
			var labelElement = new Label(label);
			labelElement.style.width = 90;
			row.Add(labelElement);
			
			var valueElement = new Label(value);
			row.Add(valueElement);
			
			return row;
		}

		private VisualElement CreateFooter()
		{
			var footer = new VisualElement();
			footer.style.flexDirection = FlexDirection.Row;
			footer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
			footer.style.paddingTop = 5;
			footer.style.paddingBottom = 5;
			footer.style.paddingLeft = 5;
			footer.style.paddingRight = 5;
			footer.style.marginTop = 5;
			
			_footerLabel = new Label();
			_footerLabel.style.flexGrow = 1;
			footer.Add(_footerLabel);
			
			var closeAllButton = new Button(() => CloseAllPresenters()) { text = "Close All" };
			closeAllButton.style.width = 80;
			footer.Add(closeAllButton);
			
			return footer;
		}

		private void UpdateFooter()
		{
			if (_footerLabel == null)
				return;
			
			if (_activePresenters != null && _activePresenters.Length > 0)
			{
				var openCount = _activePresenters.Count(p => p != null && p.IsOpen);
				_footerLabel.text = $"Open: {openCount} | Closed: {_activePresenters.Length - openCount}";
			}
			else
			{
				_footerLabel.text = "";
			}
		}

		private void RefreshPresenters()
		{
			_activePresenters = FindObjectsByType<UiPresenter>(FindObjectsSortMode.None);
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

