using System.Linq;
using UnityEditor;
using UnityEngine;
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
		private Vector2 _scrollPosition;
		private bool _autoRefresh = true;
		private double _lastRefreshTime;
		private const double RefreshInterval = 1.0; // seconds

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
			Repaint();
		}

		// ReSharper disable once UnusedMember.Local
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
				_lastRefreshTime = EditorApplication.timeSinceStartup;
				Repaint();
			}

			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
			
			DrawAnalyticsData();
			
			EditorGUILayout.EndScrollView();
			
			DrawFooter();
		}

		private void DrawHeader()
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			EditorGUILayout.LabelField("UI Service Analytics", EditorStyles.boldLabel);
			
			GUILayout.FlexibleSpace();
			
			_autoRefresh = GUILayout.Toggle(_autoRefresh, "Auto Refresh", EditorStyles.toolbarButton, GUILayout.Width(100));
			
			if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
			{
				Repaint();
			}
			
			if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50)))
			{
				GetCurrentAnalytics()?.Clear();
			}
			
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space(5);
		}

		private void DrawNotPlayingMessage()
		{
			EditorGUILayout.HelpBox(
				"UI Analytics is only available in Play Mode.\n\n" +
				"Enter Play Mode to see performance metrics and event tracking.",
				MessageType.Info);
		}

		private void DrawAnalyticsData()
		{
			var analytics = GetCurrentAnalytics();
			
			if (analytics == null)
			{
				EditorGUILayout.HelpBox("No UiService instance found. Create a UiService to enable analytics tracking.", MessageType.Warning);
				return;
			}
			
			var metrics = analytics.PerformanceMetrics;
			
			if (metrics.Count == 0)
			{
				EditorGUILayout.HelpBox("No analytics data collected yet. Use the UI Service to generate data.", MessageType.Info);
				return;
			}

			EditorGUILayout.LabelField($"Tracked UIs: {metrics.Count}", EditorStyles.boldLabel);
			EditorGUILayout.Space(5);

			// Sort by total lifetime
			var sortedMetrics = metrics.Values.OrderByDescending(m => m.TotalLifetime).ToList();

			foreach (var metric in sortedMetrics)
			{
				DrawMetricCard(metric);
			}
		}

		private void DrawMetricCard(UiPerformanceMetrics metric)
		{
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			
			// Header
			EditorGUILayout.LabelField(metric.UiName, EditorStyles.boldLabel);
			EditorGUILayout.Space(5);
			
			// Performance metrics
			EditorGUI.indentLevel++;
			
			DrawMetricRow("Load Duration:", $"{metric.LoadDuration:F3}s", GetLoadColor(metric.LoadDuration));
			DrawMetricRow("Open Duration:", $"{metric.OpenDuration:F3}s", GetOpenColor(metric.OpenDuration));
			DrawMetricRow("Close Duration:", $"{metric.CloseDuration:F3}s", GetCloseColor(metric.CloseDuration));
			
			EditorGUILayout.Space(5);
			
			DrawMetricRow("Open Count:", metric.OpenCount.ToString(), Color.white);
			DrawMetricRow("Close Count:", metric.CloseCount.ToString(), Color.white);
			DrawMetricRow("Total Lifetime:", $"{metric.TotalLifetime:F1}s", Color.cyan);
			
			if (metric.FirstOpened != System.DateTime.MinValue)
			{
				EditorGUILayout.Space(5);
				DrawMetricRow("First Opened:", metric.FirstOpened.ToString("HH:mm:ss"), Color.white);
			}
			
			if (metric.LastClosed != System.DateTime.MinValue)
			{
				DrawMetricRow("Last Closed:", metric.LastClosed.ToString("HH:mm:ss"), Color.white);
			}
			
			EditorGUI.indentLevel--;
			
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space(5);
		}

		private void DrawMetricRow(string label, string value, Color color)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(label, GUILayout.Width(150));
			
			var originalColor = GUI.contentColor;
			GUI.contentColor = color;
			EditorGUILayout.LabelField(value, EditorStyles.boldLabel);
			GUI.contentColor = originalColor;
			
			EditorGUILayout.EndHorizontal();
		}

		private void DrawFooter()
		{
			EditorGUILayout.Space(5);
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			
			var analytics = GetCurrentAnalytics();
			if (analytics != null)
			{
				var metrics = analytics.PerformanceMetrics;
				if (metrics.Count > 0)
				{
					var totalOpens = metrics.Values.Sum(m => m.OpenCount);
					var totalCloses = metrics.Values.Sum(m => m.CloseCount);
					EditorGUILayout.LabelField($"Total Opens: {totalOpens} | Total Closes: {totalCloses}");
				}
			}
			
			GUILayout.FlexibleSpace();
			
			if (GUILayout.Button("Log Summary", EditorStyles.toolbarButton))
			{
				GetCurrentAnalytics()?.LogPerformanceSummary();
			}
			
			EditorGUILayout.EndHorizontal();
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

