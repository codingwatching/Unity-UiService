using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameLovers.AssetLoader;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable CheckNamespace

namespace GameLovers.UiService
{
	/// <summary>
	/// This service provides an abstraction layer to interact with the game's <seealso cref="UiPresenter"/>
	/// The Ui Service is organized by layers. The higher the layer the more close is to the camera viewport
	/// </summary>
	public interface IUiService
	{
		/// <summary>
		/// Requests the <see cref="Canvas"/> of the given <paramref name="layer"/>
		/// </summary>
		Canvas GetLayer(int layer);
		
		/// <summary>
		/// Adds the given UI <paramref name="config"/> to the service
		/// </summary>
		/// <exception cref="ArgumentException">
		/// Thrown if the service already contains the given <paramref name="config"/>
		/// </exception>
		void AddUiConfig(UiConfig config);
		
		/// <summary>
		/// Adds the given <paramref name="uiPresenter"/> to the service and to be included inside the given <paramref name="layer"/>.
		/// If the given <paramref name="openAfter"/> is true, will open the <see cref="UiPresenter"/> after adding it to the service
		/// </summary>
		/// <exception cref="ArgumentException">
		/// Thrown if the service already contains the given <paramref name="uiPresenter"/>
		/// </exception>
		void AddUi<T>(T uiPresenter, int layer, bool openAfter = false) where T : UiPresenter;
		
		/// <summary>
		/// Removes and returns the UI of the given type <typeparamref name="T"/> without unloading it from the service
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiPresenter"/> of the given type <typeparamref name="T"/>
		/// </exception>
		T RemoveUi<T>() where T : UiPresenter;

		/// <summary>
		/// Removes and returns the UI of the given <paramref name="type"/> without unloading it from the service
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiPresenter"/> of the given <paramref name="type"/>
		/// </exception>
		UiPresenter RemoveUi(Type type);

		/// <summary>
		/// Removes and returns the given <paramref name="uiPresenter"/> without unloading it from the service
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiPresenter"/>
		/// </exception>
		T RemoveUi<T>(T uiPresenter) where T : UiPresenter;
		
		/// <summary>
		/// Loads an UI asynchronously with the given <typeparamref name="T"/>.
		/// This method can be controlled in an async method and returns the UI loaded.
		/// If the given <paramref name="openAfter"/> is true, will open the <see cref="UiPresenter"/> after loading
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain a <see cref="UiConfig"/> of the given type <typeparamref name="T"/>.
		/// You need to call <seealso cref="AddUiConfig"/> or <seealso cref="AddUi{T}"/> or initialize the service first
		/// </exception>
		Task<T> LoadUiAsync<T>(bool openAfter = false) where T : UiPresenter;
		
		/// <summary>
		/// Loads an UI asynchronously with the given <paramref name="type"/>.
		/// This method can be controlled in an async method and returns the UI loaded.
		/// If the given <paramref name="openAfter"/> is true, will open the <see cref="UiPresenter"/> after loading
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain a <see cref="UiConfig"/> of the given <paramref name="type"/>
		/// You need to call <seealso cref="AddUiConfig"/> or <seealso cref="AddUi{T}"/> or initialize the service first
		/// </exception>
		Task<UiPresenter> LoadUiAsync(Type type, bool openAfter = false);
		
		/// <summary>
		/// Unloads the UI of the given type <typeparamref name="T"/>
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiPresenter"/> of the given type <typeparamref name="T"/>
		/// </exception>
		void UnloadUi<T>() where T : UiPresenter;
		
		/// <summary>
		/// Unloads the UI of the given <paramref name="type"/>
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiPresenter"/> of the given <paramref name="type"/>
		/// </exception>
		void UnloadUi(Type type);

		/// <summary>
		/// Unloads the UI of the given <paramref name="uiPresenter"/>
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiPresenter"/> of the given <paramref name="type"/>
		/// </exception>
		void UnloadUi<T>(T uiPresenter) where T : UiPresenter;
		
		/// <summary>
		/// Checks if the service contains <seealso cref="UiPresenter"/> of the given <typeparamref name="T"/>
		/// </summary>
		bool HasUiPresenter<T>() where T : UiPresenter;
		
		/// <summary>
		/// Checks if the service contains <seealso cref="UiPresenter"/> of the given <paramref name="type"/> is loaded or not 
		/// </summary>
		bool HasUiPresenter(Type type);
		
		/// <summary>
		/// Requests the UI of given type <typeparamref name="T"/>
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiPresenter"/> of the given <typeparamref name="T"/>
		/// </exception>
		T GetUi<T>() where T : UiPresenter;
		
		/// <summary>
		/// Requests the UI of given <paramref name="type"/>
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiPresenter"/> of the given <paramref name="type"/>
		/// </exception>
		UiPresenter GetUi(Type type);

		/// <summary>
		/// Requests the list all the visible UIs' <seealso cref="Type"/> on the screen
		/// </summary>
		List<Type> GetAllVisibleUi();

		/// <summary>
		/// Opens and returns the UI of given type <typeparamref name="T"/>
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiPresenter"/> of the given <typeparamref name="T"/>
		/// </exception>
		T OpenUi<T>() where T : UiPresenter;

		/// <summary>
		/// Opens and returns the UI of given <paramref name="type"/>
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiPresenter"/> of the given <paramref name="type"/>
		/// </exception>
		UiPresenter OpenUi(Type type);

		///<inheritdoc cref="OpenUi{T}()"/>
		/// <remarks>
		/// It sets the given <paramref name="initialData"/> data BEFORE opening the UI
		/// </remarks>
		T OpenUi<T, TData>(TData initialData) 
			where T : class, IUiPresenterData 
			where TData : struct;

		///<inheritdoc cref="OpenUi(Type)"/>
		/// <exception cref="ArgumentException">
		/// Thrown if the the given <paramref name="type"/> is not of inhereting from <see cref="UiPresenterData{T}"/> class
		/// </exception>
		/// <remarks>
		/// It sets the given <paramref name="initialData"/> data BEFORE opening the UI
		/// </remarks>
		UiPresenter OpenUi<TData>(Type type, TData initialData) where TData : struct;

		/// <summary>
		/// Closes and returns the UI of given type <typeparamref name="T"/>
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiPresenter"/> of the given type <typeparamref name="T"/>
		/// </exception>
		T CloseUi<T>() where T : UiPresenter;

		/// <summary>
		/// Closes and returns the UI of given <paramref name="type"/>
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiPresenter"/> of the given <paramref name="type"/>
		/// </exception>
		UiPresenter CloseUi(Type type);

		/// <summary>
		/// Closes and returns the same given <paramref name="uiPresenter"/>
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain the given <paramref name="uiPresenter"/>
		/// </exception>
		T CloseUi<T>(T uiPresenter) where T : UiPresenter;

		/// <summary>
		/// Closes all the visible <seealso cref="UiPresenter"/>
		/// </summary>
		void CloseAllUi();

		/// <summary>
		/// Closes all the visible <seealso cref="UiPresenter"/> in the given <paramref name="layer"/>
		/// </summary>
		void CloseAllUi(int layer);

		/// <summary>
		/// Closes all the visible <seealso cref="UiPresenter"/> in front or in the same layer of the given type <typeparamref name="T"/>
		/// It excludes any visible  <seealso cref="UiPresenter"/> present in layers of the given <paramref name="excludeLayers"/>
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiPresenter"/> of the given type <typeparamref name="T"/>
		/// </exception>
		void CloseUiAndAllInFront<T>(params int[] excludeLayers) where T : UiPresenter;

		/// <summary>
		/// Adds the given <paramref name="uiSet"/> to the service
		/// </summary>
		/// <exception cref="ArgumentException">
		/// Thrown if the service already contains the given <paramref name="uiSet"/>
		/// </exception>
		void AddUiSet(UiSetConfig uiSet);

		/// <summary>
		/// Removes and returns all the <see cref="UiPresenter"/> from given <paramref name="setId "/> that are still present in the service
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiSetConfig"/> with the given <paramref name="setId"/>.
		/// You need to add it first by calling <seealso cref="AddUiSet"/>
		/// </exception>
		List<UiPresenter> RemoveUiPresentersFromSet(int setId);
		
		/// <summary>
		/// Loads asynchronously all the <see cref="UiPresenter"/> from given <paramref name="setId "/> and have not yet been loaded.
		/// This method can be controlled in an async method and returns every UI when completes loaded.
		/// This method can be controlled in a foreach loop and it will return the UIs in a first-load-first-return scheme 
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiSetConfig"/> with the given <paramref name="setId"/>.
		/// You need to add it first by calling <seealso cref="AddUiSet"/>
		/// </exception>
		Task<Task<UiPresenter>>[] LoadUiSetAsync(int setId);

		/// <summary>
		/// Unloads all the <see cref="UiPresenter"/> from given <paramref name="setId "/> that are still present in the service
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiSetConfig"/> with the given <paramref name="setId"/>.
		/// You need to add it first by calling <seealso cref="AddUiSet"/>
		/// </exception>
		void UnloadUiSet(int setId);

		/// <summary>
		/// Checks if the service contains or not the <seealso cref="UiSetConfig"/> of the given <paramref name="setId"/>
		/// </summary>
		bool HasUiSet(int setId);

		/// <summary>
		/// Checks if the service containers all the <seealso cref="UiPresenter"/> belonging in the given <paramref name="setId"/>
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiSetConfig"/> with the given <paramref name="setId"/>.
		/// You need to add it first by calling <seealso cref="AddUiSet"/>
		/// </exception>
		bool HasAllUiPresentersInSet(int setId);
		
		/// <summary>
		/// Requests the <seealso cref="UiSetConfig"/> of given type <paramref name="setId"/>
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiSetConfig"/> with the given <paramref name="setId"/>.
		/// You need to add it first by calling <seealso cref="AddUiSet"/>
		/// </exception>
		UiSetConfig GetUiSet(int setId);
		
		/// <summary>
		/// Opens all the <seealso cref="UiPresenter"/> that are part of the given <paramref name="setId"/>
		/// If the given <paramref name="closeVisibleUi"/> is set to true, will close the currently open <seealso cref="UiPresenter"/>
		/// that are not part of the given <paramref name="setId"/>
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiSetConfig"/> with the given <paramref name="setId"/>.
		/// You need to add it first by calling <seealso cref="AddUiSet"/>
		/// </exception>
		void OpenUiSet(int setId, bool closeVisibleUi);
		
		/// <summary>
		/// Closes all the <seealso cref="UiPresenter"/> that are part of the given <paramref name="setId"/>
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiSetConfig"/> with the given <paramref name="setId"/>.
		/// You need to add it first by calling <seealso cref="AddUiSet"/>
		/// </exception>
		void CloseUiSet(int setId);
	}

	/// <inheritdoc />
	public class UiService : IUiService
	{
		private readonly IDictionary<Type, UiReference> _uiViews = new Dictionary<Type, UiReference>();
		private readonly IDictionary<Type, UiConfig> _uiConfigs = new Dictionary<Type, UiConfig>();
		private readonly IDictionary<int, UiSetConfig> _uiSets = new Dictionary<int, UiSetConfig>();
		private readonly IList<Type> _visibleUiList = new List<Type>();
		private readonly IList<Canvas> _layers = new List<Canvas>();

		/// <summary>
		/// Initialize the service with the proper <paramref name="configs"/>
		/// </summary>
		/// <exception cref="ArgumentException">
		/// Thrown if any of the <see cref="UiConfig"/> in the given <paramref name="configs"/> is duplicated
		/// </exception>
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
		}

		/// <inheritdoc />
		public Canvas GetLayer(int layer)
		{
			return _layers[layer];
		}

		/// <inheritdoc />
		public void AddUiConfig(UiConfig config)
		{
			if (_uiConfigs.ContainsKey(config.UiType))
			{
				throw new ArgumentException($"The UiConfig {config.AddressableAddress} was already added");
			}

			_uiConfigs.Add(config.UiType, config);
		}

		/// <inheritdoc />
		public void AddUi<T>(T uiPresenter, int layer, bool openAfter = false) where T : UiPresenter
		{
			var type = uiPresenter.GetType().UnderlyingSystemType;
			
			if (HasUiPresenter(type))
			{
				throw new ArgumentException($"The Ui {type} was already added");
			}
			
			var reference = new UiReference
			{
				UiType = type,
				Layer = layer,
				Presenter = uiPresenter
			};
			
			for(int i = _layers.Count; i <= layer; i++)
			{
				var newObj = new GameObject($"Layer {i.ToString()}");
				var canvas = newObj.AddComponent<Canvas>();
				
				newObj.transform.position = Vector3.zero;
				canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				canvas.sortingOrder = i;

				_layers.Add(canvas);
			}
			
			_uiViews.Add(reference.UiType, reference);
			uiPresenter.transform.SetParent(_layers[layer].transform);
			uiPresenter.Init(this);

			if (openAfter)
			{
				OpenUi<T>();
			}
		}

		/// <inheritdoc />
		public T RemoveUi<T>() where T : UiPresenter
		{
			return RemoveUi(typeof(T)) as T;
		}

		/// <inheritdoc />
		public UiPresenter RemoveUi(Type type)
		{
			if (!_uiViews.TryGetValue(type, out UiReference reference))
			{
				throw new KeyNotFoundException($"The Ui {type} is not present to be removed");
			}
			
			_uiViews.Remove(type);
			_visibleUiList.Remove(type);

			return reference.Presenter;
		}

		/// <inheritdoc />
		public T RemoveUi<T>(T uiPresenter) where T : UiPresenter
		{
			RemoveUi(uiPresenter.GetType().UnderlyingSystemType);
			
			return uiPresenter;
		}

		/// <inheritdoc />
		public async Task<T> LoadUiAsync<T>(bool openAfter = false) where T : UiPresenter
		{
			var uiPresenter = await LoadUiAsync(typeof(T), openAfter);
			
			return uiPresenter as T;
		}

		/// <inheritdoc />
		public async Task<UiPresenter> LoadUiAsync(Type type, bool openAfter = false)
		{
			if (!_uiConfigs.TryGetValue(type, out var config))
			{
				throw new KeyNotFoundException($"The UiConfig of type {type} was not added to the service. Call {nameof(AddUiConfig)} first");
			}
			
			var gameObject = await AssetLoaderService.InstantiatePrefabAsync(config.AddressableAddress);
			var uiPresenter = gameObject.GetComponent<UiPresenter>();
			
			gameObject.SetActive(false);

			AddUi(uiPresenter, config.Layer, openAfter);

			return uiPresenter;
		}

		/// <inheritdoc />
		public void UnloadUi<T>() where T : UiPresenter
		{
			UnloadUi(typeof(T));
		}

		/// <inheritdoc />
		public void UnloadUi(Type type)
		{
			var gameObject = RemoveUi(type).gameObject;
			
			AssetLoaderService.UnloadAsset(gameObject);
		}

		/// <inheritdoc />
		public void UnloadUi<T>(T uiPresenter) where T : UiPresenter
		{
			UnloadUi(uiPresenter.GetType().UnderlyingSystemType);
		}

		/// <inheritdoc />
		public bool HasUiPresenter<T>() where T : UiPresenter
		{
			return HasUiPresenter(typeof(T));
		}

		/// <inheritdoc />
		public bool HasUiPresenter(Type type)
		{
			return _uiViews.ContainsKey(type);
		}

		/// <inheritdoc />
		public T GetUi<T>() where T : UiPresenter
		{
			return GetUi(typeof(T)) as T;
		}

		/// <inheritdoc />
		public UiPresenter GetUi(Type type)
		{
			return GetReference(type).Presenter;
		}

		/// <inheritdoc />
		public List<Type> GetAllVisibleUi()
		{
			return new List<Type>(_visibleUiList);
		}

		/// <inheritdoc />
		public T OpenUi<T>() where T : UiPresenter
		{
			return OpenUi(typeof(T)) as T;
		}

		/// <inheritdoc />
		public UiPresenter OpenUi(Type type)
		{
			var ui = GetUi(type);
			
			if (!_visibleUiList.Contains(type))
			{
				ui.InternalOpen();
				_visibleUiList.Add(type);
			}
			else
			{
				Debug.LogWarning($"Is trying to open the {type.Name} ui but is already open");
			}
			
			return ui;
		}

		/// <inheritdoc />
		public T OpenUi<T, TData>(TData initialData) 
			where T : class, IUiPresenterData 
			where TData : struct
		{
			return OpenUi(typeof(T), initialData) as T;
		}

		/// <inheritdoc />
		public UiPresenter OpenUi<TData>(Type type, TData initialData) where TData : struct
		{
			var uiPresenterData = GetUi(type) as UiPresenterData<TData>;

			if (uiPresenterData == null)
			{
				throw new ArgumentException($"The UiPresenter {type} is not of a {nameof(UiPresenterData<TData>)}");
			}
			
			uiPresenterData.InternalSetData(initialData);

			return OpenUi(type);
		}

		/// <inheritdoc />
		public T CloseUi<T>() where T : UiPresenter
		{
			return CloseUi(typeof(T)) as T;
		}

		/// <inheritdoc />
		public UiPresenter CloseUi(Type type)
		{
			var ui = GetUi(type);
			
			if (_visibleUiList.Contains(type))
			{
				_visibleUiList.Remove(type);
				ui.InternalClose();
			}
			else
			{
				Debug.LogWarning($"Is trying to close the {type.Name} ui but is not open");
			}

			return ui;
		}

		/// <inheritdoc />
		public T CloseUi<T>(T uiPresenter) where T : UiPresenter
		{
			CloseUi(uiPresenter.GetType().UnderlyingSystemType);

			return uiPresenter;
		}

		/// <inheritdoc />
		public void CloseAllUi()
		{
			for (int i = 0; i < _visibleUiList.Count; i++)
			{
				GetUi(_visibleUiList[i]).InternalClose();
				_visibleUiList.Remove(_visibleUiList[i]);
			}
			
			_visibleUiList.Clear();
		}

		/// <inheritdoc />
		public void CloseUiAndAllInFront<T>(params int[] excludeLayers) where T : UiPresenter
		{
			var layers = new List<int>(excludeLayers);
			
			for (int i = GetReference(typeof(T)).Layer; i <= _layers.Count; i++)
			{
				if (layers.Contains(i))
				{
					continue;
				}
				
				CloseAllUi(i);
			}
		}

		/// <inheritdoc />
		public void CloseAllUi(int layer)
		{
			for (int i = 0; i < _visibleUiList.Count; i++)
			{
				var reference = GetReference(_visibleUiList[i]);
				if (reference.Layer == layer)
				{
					reference.Presenter.InternalClose();
					_visibleUiList.Remove(reference.UiType);
				}
			}
		}

		/// <inheritdoc />
		public void AddUiSet(UiSetConfig uiSet)
		{
			if (_uiSets.ContainsKey(uiSet.SetId))
			{
				throw new ArgumentException($"The Ui Configuration with the id {uiSet.SetId.ToString()} was already added");
			}
			
			_uiSets.Add(uiSet.SetId, uiSet);
		}

		/// <inheritdoc />
		public List<UiPresenter> RemoveUiPresentersFromSet(int setId)
		{
			var set = GetUiSet(setId);
			var list = new List<UiPresenter>();

			for (int i = 0; i < set.UiConfigsType.Count; i++)
			{
				if (!HasUiPresenter(set.UiConfigsType[i]))
				{
					continue;
				}
				
				list.Add(RemoveUi(set.UiConfigsType[i]));
			}

			return list;
		}

		/// <inheritdoc />
		public Task<Task<UiPresenter>>[] LoadUiSetAsync(int setId)
		{
			var set = GetUiSet(setId);
			var uiTasks = new List<Task<UiPresenter>>();

			for (int i = 0; i < set.UiConfigsType.Count; i++)
			{
				if (HasUiPresenter(set.UiConfigsType[i]))
				{
					continue;
				}
				
				uiTasks.Add(LoadUiAsync(set.UiConfigsType[i]));
			}

			return AssetLoaderService.Interleaved(uiTasks);
		}

		/// <inheritdoc />
		public void UnloadUiSet(int setId)
		{
			var set = GetUiSet(setId);

			for (var i = 0; i < set.UiConfigsType.Count; i++)
			{
				if (HasUiPresenter(set.UiConfigsType[i]))
				{
					UnloadUi(set.UiConfigsType[i]);
				}
			}
		}

		/// <inheritdoc />
		public bool HasUiSet(int setId)
		{
			return _uiSets.ContainsKey(setId);
		}

		/// <inheritdoc />
		public bool HasAllUiPresentersInSet(int setId)
		{
			var set = GetUiSet(setId);

			for (var i = 0; i < set.UiConfigsType.Count; i++)
			{
				if (!HasUiPresenter(set.UiConfigsType[i]))
				{
					return false;
				}
			}

			return true;
		}

		/// <inheritdoc />
		public UiSetConfig GetUiSet(int setId)
		{
			if (!_uiSets.TryGetValue(setId, out UiSetConfig set))
			{
				throw new KeyNotFoundException($"The UiSet with the id {setId.ToString()} was not added to the service. Call {nameof(AddUiSet)} first");
			}

			return set;
		}

		/// <inheritdoc />
		public void OpenUiSet(int setId, bool closeVisibleUi)
		{
			var set = GetUiSet(setId);

			if (closeVisibleUi)
			{
				var list = new List<Type>(set.UiConfigsType);
				for (var i = 0; i < _visibleUiList.Count; i++)
				{
					if (list.Contains(_visibleUiList[i]))
					{
						continue;
					}

					CloseUi(_visibleUiList[i]);
				}
			}

			for (var i = 0; i < set.UiConfigsType.Count; i++)
			{
				if (_visibleUiList.Contains(set.UiConfigsType[i]))
				{
					continue;
				}
				
				OpenUi(set.UiConfigsType[i]);
			}
		}

		/// <inheritdoc />
		public void CloseUiSet(int setId)
		{
			var set = GetUiSet(setId);
			
			for (var i = 0; i < set.UiConfigsType.Count; i++)
			{
				CloseUi(set.UiConfigsType[i]);
			}
		}

		private UiReference GetReference(Type type)
		{
			if (!_uiViews.TryGetValue(type, out UiReference uiReference))
			{
				throw new KeyNotFoundException($"The Ui {type} was not added to the service. Call {nameof(AddUi)} or {nameof(LoadUiAsync)} first");
			}

			return uiReference;
		}
		
		private struct UiReference
		{
			public Type UiType;
			public int Layer;
			public UiPresenter Presenter;
		}
	}
}