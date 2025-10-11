using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

// ReSharper disable CheckNamespace

namespace GameLovers.UiService
{
	/// <inheritdoc />
	public class UiService : IUiServiceInit
	{
		public static readonly UnityEvent<DeviceOrientation, DeviceOrientation> OnOrientationChanged = new ();
		public static readonly UnityEvent<Vector2, Vector2> OnResolutionChanged = new ();

		/// <summary>
		/// Internal static reference to the current analytics instance for editor tools only.
		/// This is set when a UiService instance is created and cleared when disposed.
		/// Only accessible to editor code within this package.
		/// </summary>
		internal static IUiAnalytics CurrentAnalytics { get; private set; }
		
		private readonly IUiAssetLoader _assetLoader;
		private readonly IUiAnalytics _analytics;
		private readonly IDictionary<Type, UiConfig> _uiConfigs = new Dictionary<Type, UiConfig>();
		private readonly IList<Type> _visibleUiList = new List<Type>();
		private readonly IDictionary<int, UiSetConfig> _uiSets = new Dictionary<int, UiSetConfig>();
		private readonly IDictionary<int, GameObject> _layers = new Dictionary<int, GameObject>();
		private readonly IDictionary<Type, UiPresenter> _uiPresenters = new Dictionary<Type, UiPresenter>();

		private readonly IReadOnlyDictionary<Type, UiPresenter> _loadedPresentersReadOnly;
		private readonly IReadOnlyDictionary<int, GameObject> _layersReadOnly;
		private readonly IReadOnlyDictionary<int, UiSetConfig> _uiSetsReadOnly;
		private readonly IReadOnlyList<Type> _visiblePresentersReadOnly;

		private Transform _uiParent;
		private bool _disposed;

		/// <inheritdoc />
		public IReadOnlyDictionary<Type, UiPresenter> LoadedPresenters => _loadedPresentersReadOnly;

		/// <inheritdoc />
		public IReadOnlyDictionary<int, GameObject> Layers => _layersReadOnly;

		/// <inheritdoc />
		public IReadOnlyDictionary<int, UiSetConfig> UiSets => _uiSetsReadOnly;

		/// <inheritdoc />
		public IReadOnlyList<Type> VisiblePresenters => _visiblePresentersReadOnly;

		/// <summary>
		/// Gets the analytics instance being used by this service
		/// </summary>
		public IUiAnalytics Analytics => _analytics;

		public UiService() : this(new UiAssetLoader(), null) { }

		public UiService(IUiAssetLoader assetLoader) : this(assetLoader, null) { }

		public UiService(IUiAssetLoader assetLoader, IUiAnalytics analytics)
		{
			_assetLoader = assetLoader;
			_analytics = analytics ?? new NullAnalytics();
			
			// Set static reference for editor/debugging access
			CurrentAnalytics = _analytics;
			
			// Initialize readonly wrappers to avoid allocations on property access
			_loadedPresentersReadOnly = new ReadOnlyDictionary<Type, UiPresenter>(_uiPresenters);
			_layersReadOnly = new ReadOnlyDictionary<int, GameObject>(_layers);
			_uiSetsReadOnly = new ReadOnlyDictionary<int, UiSetConfig>(_uiSets);
			_visiblePresentersReadOnly = new ReadOnlyCollection<Type>(_visibleUiList);
		}

		/// <inheritdoc />
		public void Init(UiConfigs configs)
		{
			if (configs == null)
			{
				throw new ArgumentNullException(nameof(configs), "UiConfigs cannot be null");
			}

			var uiConfigs = configs.Configs;
			var sets = configs.Sets;

			foreach (var uiConfig in uiConfigs)
			{
				if (string.IsNullOrEmpty(uiConfig.AddressableAddress))
				{
					throw new ArgumentException($"UiConfig for type '{uiConfig.UiType.Name}' has empty addressable address. This UI will fail to load.");
				}
				if (uiConfig.UiType == null)
				{
					throw new ArgumentException($"UiConfig with addressable '{uiConfig.AddressableAddress}' has null UiType, skipping");
				}

				if (uiConfig.Layer < 0)
				{
					Debug.LogWarning($"UiConfig for type '{uiConfig.UiType.Name}' has negative layer number ({uiConfig.Layer}). This may cause unexpected behavior.");
				}
				if (uiConfig.Layer > 1000)
				{
					Debug.LogWarning($"UiConfig for type '{uiConfig.UiType.Name}' has very high layer number ({uiConfig.Layer}). Consider using lower values for better organization.");
				}

				AddUiConfig(uiConfig);
			}

			foreach (var set in sets)
			{
				AddUiSet(set);
			}

			_uiParent = new GameObject("Ui").transform;

			_uiParent.gameObject.AddComponent<UiServiceMonoComponent>();
			Object.DontDestroyOnLoad(_uiParent.gameObject);
		}

		/// <inheritdoc />
		public T GetUi<T>() where T : UiPresenter
		{
			return _uiPresenters[typeof(T)] as T;
		}

		/// <inheritdoc />
		public bool IsVisible<T>() where T : UiPresenter
		{
			return _visibleUiList.Contains(typeof(T));
		}

		/// <inheritdoc />
		public void AddUiConfig(UiConfig config)
		{
			if (!_uiConfigs.TryAdd(config.UiType, config))
			{
				Debug.LogWarning($"The UiConfig {config.AddressableAddress} was already added");
			}
		}

		/// <inheritdoc />
		public void AddUiSet(UiSetConfig uiSet)
		{
			if (!_uiSets.TryAdd(uiSet.SetId, uiSet))
			{
				Debug.LogWarning($"The Ui Configuration with the id {uiSet.SetId.ToString()} was already added");
			}
		}

		/// <inheritdoc />
		public void AddUi<T>(T ui, int layer, bool openAfter = false) where T : UiPresenter
		{
			var type = ui.GetType().UnderlyingSystemType;

			if (!_uiPresenters.TryAdd(type, ui))
			{
				Debug.LogWarning($"The Ui {type} was already added");
				return;
			}

			ui.Init(this);

			if (openAfter)
			{
				OpenUi(type);
			}
		}

		/// <inheritdoc />
		public bool RemoveUi(Type type)
		{
			_visibleUiList.Remove(type);
			
			return _uiPresenters.Remove(type);
		}

		/// <inheritdoc />
		public List<UiPresenter> RemoveUiSet(int setId)
		{
			if (!_uiSets.TryGetValue(setId, out var set))
			{
				throw new KeyNotFoundException($"UI Set with id {setId} not found.");
			}
			
			var list = new List<UiPresenter>();

			foreach (var type in set.UiConfigsType)
			{
				if (!_uiPresenters.TryGetValue(type, out var ui))
				{
					continue;
				}

				RemoveUi(type);

				list.Add(ui);
			}

			return list;
		}

		/// <inheritdoc />
		public async UniTask<UiPresenter> LoadUiAsync(Type type, bool openAfter = false, CancellationToken cancellationToken = default)
		{
			if (!_uiConfigs.TryGetValue(type, out var config))
			{
				throw new KeyNotFoundException($"The UiConfig of type {type} was not added to the service. Call {nameof(AddUiConfig)} first");
			}

			if (_uiPresenters.TryGetValue(type, out var ui))
			{
				Debug.LogWarning($"The Ui {type} was already loaded");
				ui.gameObject.SetActive(openAfter);

				return ui;
			}

			_analytics.TrackLoadStart(type);

			var layer = AddLayer(config.Layer);
			var gameObject = await _assetLoader.InstantiatePrefab(config, layer.transform, cancellationToken);

			// Double check if the same UiPresenter was already loaded. This can happen if the coder spam calls LoadUiAsync
			if (_uiPresenters.TryGetValue(type, out var uiDouble))
			{
				_assetLoader.UnloadAsset(gameObject);
				uiDouble.gameObject.SetActive(openAfter);

				return uiDouble;
			}

			var uiPresenter = gameObject.GetComponent<UiPresenter>();

			gameObject.SetActive(false);
			AddUi(uiPresenter, config.Layer, openAfter);
			
			_analytics.TrackLoadComplete(type, config.Layer);

			return uiPresenter;
		}

		/// <inheritdoc />
		public IList<UniTask<UiPresenter>> LoadUiSetAsync(int setId)
		{
			var uiTasks = new List<UniTask<UiPresenter>>();

			if (_uiSets.TryGetValue(setId, out var set))
			{
				foreach (var type in set.UiConfigsType)
				{
					if (_uiPresenters.ContainsKey(type))
					{
						continue;
					}

					uiTasks.Add(LoadUiAsync(type));
				}
			}

			return uiTasks;
		}

		/// <inheritdoc />
		public void UnloadUi(Type type)
		{
			if (!_uiPresenters.TryGetValue(type, out var ui))
			{
				throw new KeyNotFoundException($"Cannot unload UI of type {type}. It is not loaded.");
			}
			
			var config = _uiConfigs[type];
			
			RemoveUi(type);

			_assetLoader.UnloadAsset(ui.gameObject);
			
			_analytics.TrackUnload(type, config.Layer);
		}

		/// <inheritdoc />
		public void UnloadUiSet(int setId)
		{
			var set = _uiSets[setId];

			foreach (var type in set.UiConfigsType)
			{
				if (_uiPresenters.ContainsKey(type))
				{
					UnloadUi(type);
				}
			}
		}

		/// <inheritdoc />
		public async UniTask<UiPresenter> OpenUiAsync(Type type, CancellationToken cancellationToken = default)
		{
			var ui = await GetOrLoadUiAsync(type, cancellationToken);

			OpenUi(type);

			return ui;
		}

		/// <inheritdoc />
		public async UniTask<UiPresenter> OpenUiAsync<TData>(Type type, TData initialData, CancellationToken cancellationToken = default) where TData : struct
		{
			var ui = await GetOrLoadUiAsync(type, cancellationToken);

			if (ui is UiToolkitPresenter<TData>)
			{
				var uiPresenter = ui as UiToolkitPresenter<TData>;
				
				uiPresenter.InternalSetData(initialData);
			}
			else if (ui is UiPresenter<TData>)
			{
				var uiPresenter = ui as UiPresenter<TData>;
				
				uiPresenter.InternalSetData(initialData);
			}
			else
			{
				Debug.LogError($"The UiPresenter {type} is not a {nameof(UiPresenter<TData>)} nor {nameof(UiToolkitPresenter<TData>)} type. " +
							$"Implement it to allow it to open with initial defined data");
				return ui;
			}
			
			OpenUi(type);

			return ui;
		}

		/// <inheritdoc />
		public void CloseUi(Type type, bool destroy = false)
		{
			if (!_visibleUiList.Contains(type))
			{
				Debug.LogWarning($"Is trying to close the {type.Name} ui but is not open");
				return;
			}

			_analytics.TrackCloseStart(type);
			
			_visibleUiList.Remove(type);
			_uiPresenters[type].InternalClose(destroy);
			
			var config = _uiConfigs[type];
			_analytics.TrackCloseComplete(type, config.Layer, destroy);
		}

		/// <inheritdoc />
		public void CloseAllUi()
		{
			foreach (var type in _visibleUiList)
			{
				_uiPresenters[type].InternalClose(false);
			}

			_visibleUiList.Clear();
		}

		/// <inheritdoc />
		public void CloseAllUi(int layer)
		{
			for (int i = _visibleUiList.Count - 1; i >= 0; i--)
			{
				var type = _visibleUiList[i];

				if (_uiConfigs[type].Layer == layer)
				{
					_uiPresenters[type].InternalClose(false);
					_visibleUiList.Remove(type);
				}
			}
		}

		/// <inheritdoc />
		public void CloseAllUiSet(int setId)
		{
			var set = _uiSets[setId];

			foreach (var type in set.UiConfigsType)
			{
				CloseUi(type);
			}
		}

		private GameObject AddLayer(int layer)
		{
			if (_layers.ContainsKey(layer)) return _layers[layer];

			var newObj = new GameObject($"Layer {layer.ToString()}");

			newObj.transform.position = Vector3.zero;

			newObj.transform.SetParent(_uiParent);
			_layers.Add(layer, newObj);

			return _layers[layer];
		}

		private void OpenUi(Type type)
		{
			if (_visibleUiList.Contains(type))
			{
				Debug.LogWarning($"Is trying to open the {type.Name} ui but is already open");
				return;
			}

			_analytics.TrackOpenStart(type);
			
			_uiPresenters[type].InternalOpen();
			_visibleUiList.Add(type);
			
			var config = _uiConfigs[type];
			_analytics.TrackOpenComplete(type, config.Layer);
		}

		private async UniTask<UiPresenter> GetOrLoadUiAsync(Type type, CancellationToken cancellationToken = default)
		{
			if (!_uiPresenters.TryGetValue(type, out var ui))
			{
				ui = await LoadUiAsync(type, false, cancellationToken);
			}

			return ui;
		}

		/// <summary>
		/// Disposes of the UI service, cleaning up all resources and unsubscribing from events.
		/// </summary>
		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			// Clear static reference
			if (CurrentAnalytics == _analytics)
			{
				CurrentAnalytics = null;
			}

			// Close all visible UI
			CloseAllUi();

			// Unload all UI presenters
			var presenterTypes = new List<Type>(_uiPresenters.Keys);
			foreach (var type in presenterTypes)
			{
				try
				{
					UnloadUi(type);
				}
				catch (Exception ex)
				{
					Debug.LogWarning($"Failed to unload UI of type {type.Name} during disposal: {ex.Message}");
				}
			}

			// Clear all collections
			_uiPresenters.Clear();
			_visibleUiList.Clear();
			_uiConfigs.Clear();
			_uiSets.Clear();
			_layers.Clear();

			// Clean up static events
			// Note: We don't call RemoveAllListeners on static UnityEvents as it would affect other instances
			// Users should unsubscribe from OnOrientationChanged and OnResolutionChanged in their own code

			// Destroy UI parent GameObject
			if (_uiParent != null)
			{
				Object.Destroy(_uiParent.gameObject);
				_uiParent = null;
			}
		}
	}
}