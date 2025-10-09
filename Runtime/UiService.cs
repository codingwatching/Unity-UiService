using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
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
		
		private readonly IUiAssetLoader _assetLoader;
		private readonly IDictionary<Type, UiConfig> _uiConfigs = new Dictionary<Type, UiConfig>();
		private readonly IList<Type> _visibleUiList = new List<Type>();
		private readonly IDictionary<int, UiSetConfig> _uiSets = new Dictionary<int, UiSetConfig>();
		private readonly IDictionary<int, GameObject> _layers = new Dictionary<int, GameObject>();
		private readonly IDictionary<Type, UiPresenter> _uiPresenters = new Dictionary<Type, UiPresenter>();

		private Type _loadingSpinnerType;
		private Transform _uiParent;

		/// <inheritdoc />
		public IReadOnlyDictionary<Type, UiPresenter> LoadedPresenters => new Dictionary<Type, UiPresenter>(_uiPresenters);

		/// <inheritdoc />
		public IReadOnlyDictionary<int, GameObject> Layers => new Dictionary<int, GameObject>(_layers);

		/// <inheritdoc />
		public IReadOnlyDictionary<int, UiSetConfig> UiSets => new Dictionary<int, UiSetConfig>(_uiSets);

		/// <inheritdoc />
		public IReadOnlyList<Type> VisiblePresenters => new List<Type>(_visibleUiList);

		public UiService() : this(new UiAssetLoader()) { }

		public UiService(IUiAssetLoader assetLoader)
		{
			_assetLoader = assetLoader;
		}

		/// <inheritdoc />
		public void Init(UiConfigs configs)
		{
			var uiConfigs = configs.Configs;
			var sets = configs.Sets;

			foreach (var uiConfig in uiConfigs)
			{
				AddUiConfig(uiConfig);
			}

			foreach (var set in sets)
			{
				AddUiSet(set);
			}

			_uiParent = new GameObject("Ui").transform;
			_loadingSpinnerType = configs.LoadingSpinnerType;

			_uiParent.gameObject.AddComponent<UiServiceMonoComponent>();
			Object.DontDestroyOnLoad(_uiParent.gameObject);

			if (_loadingSpinnerType != null)
			{
				LoadUiAsync(_loadingSpinnerType).Forget();
			}
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
			if (_uiConfigs.ContainsKey(config.UiType))
			{
				Debug.LogWarning($"The UiConfig {config.AddressableAddress} was already added");
				return;
			}

			_uiConfigs.Add(config.UiType, config);
		}

		/// <inheritdoc />
		public void AddUiSet(UiSetConfig uiSet)
		{
			if (_uiSets.ContainsKey(uiSet.SetId))
			{
				Debug.LogWarning($"The Ui Configuration with the id {uiSet.SetId.ToString()} was already added");
				return;
			}

			_uiSets.Add(uiSet.SetId, uiSet);
		}

		/// <inheritdoc />
		public void AddUi<T>(T ui, int layer, bool openAfter = false) where T : UiPresenter
		{
			var type = ui.GetType().UnderlyingSystemType;

			if (_uiPresenters.ContainsKey(type))
			{
				Debug.LogWarning($"The Ui {type} was already added");
				return;
			}

			_uiPresenters.Add(type, ui);
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
			var set = _uiSets[setId];
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
		public async UniTask<UiPresenter> LoadUiAsync(Type type, bool openAfter = false)
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

			var layer = AddLayer(config.Layer);
			var gameObject = await _assetLoader.InstantiatePrefab(config, layer.transform);

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
			var ui = _uiPresenters[type];

			RemoveUi(type);

			_assetLoader.UnloadAsset(ui.gameObject);
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
		public async UniTask<UiPresenter> OpenUiAsync(Type type)
		{
			var ui = await GetOrLoadUiAsync(type);

			OpenUi(type);

			return ui;
		}

		/// <inheritdoc />
		public async UniTask<UiPresenter> OpenUiAsync<TData>(Type type, TData initialData) where TData : struct
		{
			var ui = await GetOrLoadUiAsync(type);

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

			_visibleUiList.Remove(type);
			_uiPresenters[type].InternalClose(destroy);
		}

		/// <inheritdoc />
		public void CloseAllUi()
		{
			for (int i = 0; i < _visibleUiList.Count; i++)
			{
				_uiPresenters[_visibleUiList[i]].InternalClose(false);
				_visibleUiList.Remove(_visibleUiList[i]);
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

			_uiPresenters[type].InternalOpen();
			_visibleUiList.Add(type);
		}

	private async UniTask<UiPresenter> GetOrLoadUiAsync(Type type)
	{
		if (!_uiPresenters.TryGetValue(type, out var ui))
		{
			OpenLoadingSpinner();
			ui = await LoadUiAsync(type);
			CloseLoadingSpinner();
		}

		return ui;
	}

		private void OpenLoadingSpinner()
		{
			if (_loadingSpinnerType == null)
			{
				return;
			}

			OpenUi(_loadingSpinnerType);
		}

		private void CloseLoadingSpinner()
		{
			if (_loadingSpinnerType == null)
			{
				return;
			}

			CloseUi(_loadingSpinnerType);
		}
	}
}