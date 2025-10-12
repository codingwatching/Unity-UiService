using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using GameLovers.UiService;

// ReSharper disable once CheckNamespace

namespace GameLoversEditor.UiService
{
	/// <summary>
	/// Editor window for viewing UI analytics and performance metrics.
	/// This window uses UiService.CurrentAnalytics (internal) to access the analytics instance
	/// from the currently active UiService in play mode.
	/// Note: CurrentAnalytics is internal and only accessible to editor code within this package.
	/// </summary>
	public class UiAnalyticsWindow : EditorWindow
	{
		private bool _autoRefresh = true;
		private double _lastRefreshTime;
		private const double RefreshInterval = 1.0; // seconds
		
		private ScrollView _scrollView;
		private VisualElement _contentContainer;
		private Label _footerStats;

		[MenuItem("Tools/UI Service/Analytics")]
		public static void ShowWindow()
		{
			var window = GetWindow<UiAnalyticsWindow>("UI Analytics");
			window.minSize = new Vector2(500, 300);
			window.Show();
		}

		// ReSharper disable once UnusedMember.Local
		private void OnEnable()
		{
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		// ReSharper disable once UnusedMember.Local
		private void OnDisable()
		{
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		}

		private void OnPlayModeStateChanged(PlayModeStateChange state)
		{
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
			
			// Content container
			_contentContainer = new VisualElement();
			root.Add(_contentContainer);
			
			// Scroll view
			_scrollView = new ScrollView();
			_scrollView.style.flexGrow = 1;
			_contentContainer.Add(_scrollView);
			
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
			
			var titleLabel = new Label("UI Service Analytics");
			titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
			titleLabel.style.flexGrow = 1;
			header.Add(titleLabel);
			
			// Auto refresh toggle
			var autoRefreshToggle = new Toggle("Auto Refresh") { value = _autoRefresh };
			autoRefreshToggle.RegisterValueChangedCallback(evt => _autoRefresh = evt.newValue);
			autoRefreshToggle.style.width = 110;
			header.Add(autoRefreshToggle);
			
			// Refresh button
			var refreshButton = new Button(() => UpdateContent()) { text = "Refresh" };
			refreshButton.style.width = 60;
			refreshButton.style.marginLeft = 5;
			header.Add(refreshButton);
			
			// Clear button
			var clearButton = new Button(() => GetCurrentAnalytics()?.Clear()) { text = "Clear" };
			clearButton.style.width = 50;
			clearButton.style.marginLeft = 5;
			header.Add(clearButton);
			
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
					"UI Analytics is only available in Play Mode.\n\n" +
					"Enter Play Mode to see performance metrics and event tracking.",
					HelpBoxMessageType.Info);
				_scrollView.Add(helpBox);
				UpdateFooter();
				return;
			}

			var analytics = GetCurrentAnalytics();
			
			if (analytics == null)
			{
				var warningBox = new HelpBox("No UiService instance found. Create a UiService to enable analytics tracking.", HelpBoxMessageType.Warning);
				_scrollView.Add(warningBox);
				UpdateFooter();
				return;
			}
			
			var metrics = analytics.PerformanceMetrics;
			
			if (metrics.Count == 0)
			{
				var infoBox = new HelpBox("No analytics data collected yet. Use the UI Service to generate data.", HelpBoxMessageType.Info);
				_scrollView.Add(infoBox);
				UpdateFooter();
				return;
			}

			var trackedLabel = new Label($"Tracked UIs: {metrics.Count}");
			trackedLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
			trackedLabel.style.marginBottom = 5;
			trackedLabel.style.marginLeft = 5;
			_scrollView.Add(trackedLabel);

			// Sort by total lifetime
			var sortedMetrics = metrics.Values.OrderByDescending(m => m.TotalLifetime).ToList();

			foreach (var metric in sortedMetrics)
			{
				var card = CreateMetricCard(metric);
				_scrollView.Add(card);
			}
			
			UpdateFooter();
		}

		private VisualElement CreateMetricCard(UiPerformanceMetrics metric)
		{
			var card = new VisualElement();
			card.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
			card.style.borderTopLeftRadius = 4;
			card.style.borderTopRightRadius = 4;
			card.style.borderBottomLeftRadius = 4;
			card.style.borderBottomRightRadius = 4;
			card.style.marginLeft = 5;
			card.style.marginRight = 5;
			card.style.marginBottom = 5;
			card.style.paddingTop = 8;
			card.style.paddingBottom = 8;
			card.style.paddingLeft = 10;
			card.style.paddingRight = 10;
			
			// Header
			var header = new Label(metric.UiName);
			header.style.unityFontStyleAndWeight = FontStyle.Bold;
			header.style.fontSize = 13;
			header.style.marginBottom = 8;
			card.Add(header);
			
			// Performance metrics
			card.Add(CreateMetricRow("Load Duration:", $"{metric.LoadDuration:F3}s", GetLoadColor(metric.LoadDuration)));
			card.Add(CreateMetricRow("Open Duration:", $"{metric.OpenDuration:F3}s", GetOpenColor(metric.OpenDuration)));
			card.Add(CreateMetricRow("Close Duration:", $"{metric.CloseDuration:F3}s", GetCloseColor(metric.CloseDuration)));
			
			card.Add(CreateSpacer(5));
			
			card.Add(CreateMetricRow("Open Count:", metric.OpenCount.ToString(), Color.white));
			card.Add(CreateMetricRow("Close Count:", metric.CloseCount.ToString(), Color.white));
			card.Add(CreateMetricRow("Total Lifetime:", $"{metric.TotalLifetime:F1}s", Color.cyan));
			
			if (metric.FirstOpened != System.DateTime.MinValue)
			{
				card.Add(CreateSpacer(5));
				card.Add(CreateMetricRow("First Opened:", metric.FirstOpened.ToString("HH:mm:ss"), Color.white));
			}
			
			if (metric.LastClosed != System.DateTime.MinValue)
			{
				card.Add(CreateMetricRow("Last Closed:", metric.LastClosed.ToString("HH:mm:ss"), Color.white));
			}
			
			return card;
		}

		private VisualElement CreateMetricRow(string label, string value, Color color)
		{
			var row = new VisualElement();
			row.style.flexDirection = FlexDirection.Row;
			row.style.marginBottom = 2;
			
			var labelElement = new Label(label);
			labelElement.style.width = 150;
			labelElement.style.marginLeft = 10;
			row.Add(labelElement);
			
			var valueElement = new Label(value);
			valueElement.style.unityFontStyleAndWeight = FontStyle.Bold;
			valueElement.style.color = color;
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
			
			_footerStats = new Label();
			_footerStats.style.flexGrow = 1;
			footer.Add(_footerStats);
			
			var logButton = new Button(() => GetCurrentAnalytics()?.LogPerformanceSummary()) { text = "Log Summary" };
			logButton.style.width = 100;
			footer.Add(logButton);
			
			return footer;
		}

		private void UpdateFooter()
		{
			if (_footerStats == null)
				return;
			
			var analytics = GetCurrentAnalytics();
			if (analytics != null)
			{
				var metrics = analytics.PerformanceMetrics;
				if (metrics.Count > 0)
				{
					var totalOpens = metrics.Values.Sum(m => m.OpenCount);
					var totalCloses = metrics.Values.Sum(m => m.CloseCount);
					_footerStats.text = $"Total Opens: {totalOpens} | Total Closes: {totalCloses}";
				}
				else
				{
					_footerStats.text = "";
				}
			}
			else
			{
				_footerStats.text = "";
			}
		}

		private VisualElement CreateSpacer(int height)
		{
			var spacer = new VisualElement();
			spacer.style.height = height;
			return spacer;
		}

		/// <summary>
		/// Helper method to get the current analytics instance from the active UiService.
		/// Note: This accesses an internal property only available to editor code within this package.
		/// </summary>
		private IUiAnalytics GetCurrentAnalytics()
		{
			return GameLovers.UiService.UiService.CurrentAnalytics;
		}

		private Color GetLoadColor(float duration)
		{
			if (duration < 0.1f) return Color.green;
			if (duration < 0.5f) return Color.yellow;
			return Color.red;
		}

		private Color GetOpenColor(float duration)
		{
			if (duration < 0.05f) return Color.green;
			if (duration < 0.2f) return Color.yellow;
			return Color.red;
		}

		private Color GetCloseColor(float duration)
		{
			if (duration < 0.05f) return Color.green;
			if (duration < 0.2f) return Color.yellow;
			return Color.red;
		}
	}
}

