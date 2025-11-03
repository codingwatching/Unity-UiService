using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// ReSharper disable CheckNamespace

namespace GameLovers.UiService
{
	/// <summary>
	/// Event data for UI lifecycle events
	/// </summary>
	public struct UiEventData
	{
		public Type UiType;
		public string UiName;
		public int Layer;
		public float Timestamp;
		public bool WasDestroyed;

		public UiEventData(Type uiType, int layer, bool wasDestroyed = false)
		{
			UiType = uiType;
			UiName = uiType.Name;
			Layer = layer;
			Timestamp = Time.time;
			WasDestroyed = wasDestroyed;
		}
	}

	/// <summary>
	/// Performance metrics for UI operations
	/// </summary>
	public struct UiPerformanceMetrics
	{
		public Type UiType;
		public string UiName;
		public float LoadDuration;
		public float OpenDuration;
		public float CloseDuration;
		public float TotalLifetime;
		public int OpenCount;
		public int CloseCount;
		public DateTime FirstOpened;
		public DateTime LastClosed;

		public UiPerformanceMetrics(Type uiType)
		{
			UiType = uiType;
			UiName = uiType.Name;
			LoadDuration = 0f;
			OpenDuration = 0f;
			CloseDuration = 0f;
			TotalLifetime = 0f;
			OpenCount = 0;
			CloseCount = 0;
			FirstOpened = DateTime.MinValue;
			LastClosed = DateTime.MinValue;
		}
	}

	/// <summary>
	/// Callback interface for custom analytics integration
	/// </summary>
	public interface IUiAnalyticsCallback
	{
		/// <summary>
		/// Called when a UI has been loaded into memory
		/// </summary>
		/// <param name="data">Event data containing UI type, name, layer, and timestamp</param>
		void OnUiLoaded(UiEventData data);
		
		/// <summary>
		/// Called when a UI has been opened and made visible
		/// </summary>
		/// <param name="data">Event data containing UI type, name, layer, and timestamp</param>
		void OnUiOpened(UiEventData data);
		
		/// <summary>
		/// Called when a UI has been closed and hidden
		/// </summary>
		/// <param name="data">Event data containing UI type, name, layer, timestamp, and destruction status</param>
		void OnUiClosed(UiEventData data);
		
		/// <summary>
		/// Called when a UI has been unloaded from memory
		/// </summary>
		/// <param name="data">Event data containing UI type, name, layer, and timestamp</param>
		void OnUiUnloaded(UiEventData data);
		
		/// <summary>
		/// Called when performance metrics for a UI have been updated
		/// </summary>
		/// <param name="metrics">Performance metrics including load/open/close durations, counts, and lifetime</param>
		void OnPerformanceMetricsUpdated(UiPerformanceMetrics metrics);
	}

	/// <summary>
	/// Interface for UI analytics tracking
	/// </summary>
	public interface IUiAnalytics
	{
		/// <summary>
		/// Gets all performance metrics for all tracked UIs
		/// </summary>
		IReadOnlyDictionary<Type, UiPerformanceMetrics> PerformanceMetrics { get; }

		/// <summary>
		/// Unity Events for easy integration
		/// </summary>
		UnityEvent<UiEventData> OnUiLoaded { get; }
		UnityEvent<UiEventData> OnUiOpened { get; }
		UnityEvent<UiEventData> OnUiClosed { get; }
		UnityEvent<UiEventData> OnUiUnloaded { get; }
		UnityEvent<UiPerformanceMetrics> OnPerformanceMetricsUpdated { get; }

		/// <summary>
		/// Sets a custom callback for analytics events
		/// </summary>
		void SetCallback(IUiAnalyticsCallback callback);

		/// <summary>
		/// Clears all analytics data
		/// </summary>
		void Clear();

		/// <summary>
		/// Gets performance metrics for a specific UI type
		/// </summary>
		UiPerformanceMetrics GetMetrics(Type uiType);

		/// <summary>
		/// Logs a summary of all performance metrics to the console
		/// </summary>
		void LogPerformanceSummary();

		/// <summary>
		/// Tracks the start of a UI load operation
		/// </summary>
		void TrackLoadStart(Type uiType);

		/// <summary>
		/// Tracks the completion of a UI load operation
		/// </summary>
		void TrackLoadComplete(Type uiType, int layer);

		/// <summary>
		/// Tracks the start of a UI open operation
		/// </summary>
		void TrackOpenStart(Type uiType);

		/// <summary>
		/// Tracks the completion of a UI open operation
		/// </summary>
		void TrackOpenComplete(Type uiType, int layer);

		/// <summary>
		/// Tracks the start of a UI close operation
		/// </summary>
		void TrackCloseStart(Type uiType);

		/// <summary>
		/// Tracks the completion of a UI close operation
		/// </summary>
		void TrackCloseComplete(Type uiType, int layer, bool destroyed);

		/// <summary>
		/// Tracks a UI unload operation
		/// </summary>
		void TrackUnload(Type uiType, int layer);
	}

	/// <summary>
	/// Analytics system for tracking UI events and performance
	/// </summary>
	public class UiAnalytics : IUiAnalytics
	{
		// Performance tracking
		private readonly Dictionary<Type, UiPerformanceMetrics> _performanceMetrics = new Dictionary<Type, UiPerformanceMetrics>();
		private readonly Dictionary<Type, float> _loadStartTimes = new Dictionary<Type, float>();
		private readonly Dictionary<Type, float> _openStartTimes = new Dictionary<Type, float>();
		private readonly Dictionary<Type, float> _closeStartTimes = new Dictionary<Type, float>();
		private readonly Dictionary<Type, float> _firstOpenTimes = new Dictionary<Type, float>();

		// Optional callback for custom analytics
		private IUiAnalyticsCallback _callback;
		
		// Unity Events for easy integration
		public UnityEvent<UiEventData> OnUiLoaded { get; } = new UnityEvent<UiEventData>();
		public UnityEvent<UiEventData> OnUiOpened { get; } = new UnityEvent<UiEventData>();
		public UnityEvent<UiEventData> OnUiClosed { get; } = new UnityEvent<UiEventData>();
		public UnityEvent<UiEventData> OnUiUnloaded { get; } = new UnityEvent<UiEventData>();
		public UnityEvent<UiPerformanceMetrics> OnPerformanceMetricsUpdated { get; } = new UnityEvent<UiPerformanceMetrics>();

		/// <summary>
		/// Gets all performance metrics for all tracked UIs
		/// </summary>
		public IReadOnlyDictionary<Type, UiPerformanceMetrics> PerformanceMetrics => _performanceMetrics;

		/// <summary>
		/// Sets a custom callback for analytics events
		/// </summary>
		public void SetCallback(IUiAnalyticsCallback callback)
		{
			_callback = callback;
		}

		/// <summary>
		/// Clears all analytics data
		/// </summary>
		public void Clear()
		{
			_performanceMetrics.Clear();
			_loadStartTimes.Clear();
			_openStartTimes.Clear();
			_closeStartTimes.Clear();
			_firstOpenTimes.Clear();
		}

		/// <summary>
		/// Gets performance metrics for a specific UI type
		/// </summary>
		public UiPerformanceMetrics GetMetrics(Type uiType)
		{
			return _performanceMetrics.TryGetValue(uiType, out var metrics) 
				? metrics 
				: new UiPerformanceMetrics(uiType);
		}

		public void TrackLoadStart(Type uiType)
		{
			_loadStartTimes[uiType] = Time.realtimeSinceStartup;
		}

		public void TrackLoadComplete(Type uiType, int layer)
		{
			var duration = 0f;
			if (_loadStartTimes.TryGetValue(uiType, out var startTime))
			{
				duration = Time.realtimeSinceStartup - startTime;
				_loadStartTimes.Remove(uiType);
			}

			UpdateMetrics(uiType, metrics =>
			{
				metrics.LoadDuration = duration;
				return metrics;
			});

			var eventData = new UiEventData(uiType, layer);
			OnUiLoaded.Invoke(eventData);
			_callback?.OnUiLoaded(eventData);
		}

		public void TrackOpenStart(Type uiType)
		{
			_openStartTimes[uiType] = Time.realtimeSinceStartup;
			
			if (!_firstOpenTimes.ContainsKey(uiType))
			{
				_firstOpenTimes[uiType] = Time.realtimeSinceStartup;
			}
		}

		public void TrackOpenComplete(Type uiType, int layer)
		{
			var duration = 0f;
			if (_openStartTimes.TryGetValue(uiType, out var startTime))
			{
				duration = Time.realtimeSinceStartup - startTime;
				_openStartTimes.Remove(uiType);
			}

			UpdateMetrics(uiType, metrics =>
			{
				metrics.OpenDuration = duration;
				metrics.OpenCount++;
				if (metrics.FirstOpened == DateTime.MinValue)
				{
					metrics.FirstOpened = DateTime.Now;
				}
				return metrics;
			});

			var eventData = new UiEventData(uiType, layer);
			OnUiOpened.Invoke(eventData);
			_callback?.OnUiOpened(eventData);
		}

		public void TrackCloseStart(Type uiType)
		{
			_closeStartTimes[uiType] = Time.realtimeSinceStartup;
		}

		public void TrackCloseComplete(Type uiType, int layer, bool destroyed)
		{
			var duration = 0f;
			if (_closeStartTimes.TryGetValue(uiType, out var startTime))
			{
				duration = Time.realtimeSinceStartup - startTime;
				_closeStartTimes.Remove(uiType);
			}

			// Calculate lifetime
			var lifetime = 0f;
			if (_firstOpenTimes.TryGetValue(uiType, out var firstOpenTime))
			{
				lifetime = Time.realtimeSinceStartup - firstOpenTime;
			}

			UpdateMetrics(uiType, metrics =>
			{
				metrics.CloseDuration = duration;
				metrics.CloseCount++;
				metrics.TotalLifetime = lifetime;
				metrics.LastClosed = DateTime.Now;
				return metrics;
			});

			var eventData = new UiEventData(uiType, layer, destroyed);
			OnUiClosed.Invoke(eventData);
			_callback?.OnUiClosed(eventData);
		}

		public void TrackUnload(Type uiType, int layer)
		{
			var eventData = new UiEventData(uiType, layer, true);
			OnUiUnloaded.Invoke(eventData);
			_callback?.OnUiUnloaded(eventData);

			// Clean up tracking data
			_firstOpenTimes.Remove(uiType);
		}

		private void UpdateMetrics(Type uiType, Func<UiPerformanceMetrics, UiPerformanceMetrics> updateFunc)
		{
			if (!_performanceMetrics.ContainsKey(uiType))
			{
				_performanceMetrics[uiType] = new UiPerformanceMetrics(uiType);
			}

			_performanceMetrics[uiType] = updateFunc(_performanceMetrics[uiType]);
			OnPerformanceMetricsUpdated.Invoke(_performanceMetrics[uiType]);
			_callback?.OnPerformanceMetricsUpdated(_performanceMetrics[uiType]);
		}

		/// <summary>
		/// Logs a summary of all performance metrics to the console
		/// </summary>
		public void LogPerformanceSummary()
		{
			Debug.Log("=== UI Service Performance Summary ===");
			
			foreach (var kvp in _performanceMetrics)
			{
				var metrics = kvp.Value;
				Debug.Log($"\n{metrics.UiName}:");
				Debug.Log($"  Load Time: {metrics.LoadDuration:F3}s");
				Debug.Log($"  Avg Open Time: {metrics.OpenDuration:F3}s");
				Debug.Log($"  Avg Close Time: {metrics.CloseDuration:F3}s");
				Debug.Log($"  Open Count: {metrics.OpenCount}");
				Debug.Log($"  Close Count: {metrics.CloseCount}");
				Debug.Log($"  Total Lifetime: {metrics.TotalLifetime:F1}s");
			}
			
			Debug.Log("\n======================================");
		}
	}

	/// <summary>
	/// Null implementation of IUiAnalytics that does nothing. Used when analytics is disabled.
	/// </summary>
	public class NullAnalytics : IUiAnalytics
	{
		private static readonly IReadOnlyDictionary<Type, UiPerformanceMetrics> EmptyMetrics = new Dictionary<Type, UiPerformanceMetrics>();
		
		public IReadOnlyDictionary<Type, UiPerformanceMetrics> PerformanceMetrics => EmptyMetrics;
		
		public UnityEvent<UiEventData> OnUiLoaded { get; } = new UnityEvent<UiEventData>();
		public UnityEvent<UiEventData> OnUiOpened { get; } = new UnityEvent<UiEventData>();
		public UnityEvent<UiEventData> OnUiClosed { get; } = new UnityEvent<UiEventData>();
		public UnityEvent<UiEventData> OnUiUnloaded { get; } = new UnityEvent<UiEventData>();
		public UnityEvent<UiPerformanceMetrics> OnPerformanceMetricsUpdated { get; } = new UnityEvent<UiPerformanceMetrics>();
		
		public void SetCallback(IUiAnalyticsCallback callback) { }
		public void Clear() { }
		public UiPerformanceMetrics GetMetrics(Type uiType) => new UiPerformanceMetrics(uiType);
		public void LogPerformanceSummary() { }
		public void TrackLoadStart(Type uiType) { }
		public void TrackLoadComplete(Type uiType, int layer) { }
		public void TrackOpenStart(Type uiType) { }
		public void TrackOpenComplete(Type uiType, int layer) { }
		public void TrackCloseStart(Type uiType) { }
		public void TrackCloseComplete(Type uiType, int layer, bool destroyed) { }
		public void TrackUnload(Type uiType, int layer) { }
	}
}

