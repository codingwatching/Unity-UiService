using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameLovers.LoaderExtension;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
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
		/// Adds the given UI <paramref name="config"/> to the service
		/// </summary>
		void AddUiConfig(UiConfig config);
		
		/// <summary>
		/// Adds the given <paramref name="ui"/> to the service and to be included inside the given <paramref name="layer"/>
		/// </summary>
		/// <exception cref="NullReferenceException">
		/// Thrown if the given <paramref name="ui"/> is null
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Thrown if the service already contains the given <paramref name="ui"/>
		/// </exception>
		void AddUi<T>(int layer, T ui) where T : UiPresenter;
		
		///<inheritdoc cref="AddUi{T}(int,T)"/>
		/// <exception cref="InvalidCastException">
		/// Thrown if the given <paramref name="type"/> is not a sub class of <seealso cref="UiPresenter"/>
		/// </exception>
		void AddUi(int layer, UiPresenter ui, Type type);
		
		/// <summary>
		/// Loads an UI asynchronously with the given <typeparamref name="T"/>
		/// This method can be controlled in an async method and returns the UI loaded
		/// </summary>
		Task<T> LoadUiAsync<T>() where T : UiPresenter;
		
		/// <summary>
		/// Loads an UI asynchronously with the given <paramref name="type"/>
		/// This method can be controlled in an async method and returns the UI loaded
		/// </summary>
		Task<UiPresenter> LoadUiAsync(Type type);
		
		/// <summary>
		/// Unloads the UI of the given type <typeparamref name="T"/>
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiPresenter"/> of the given type <typeparamref name="T"/>
		/// </exception>
		T UnloadUi<T>() where T : UiPresenter;
		
		/// <summary>
		/// Unloads the UI of the given <paramref name="type"/>
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiPresenter"/> of the given <paramref name="type"/>
		/// </exception>
		UiPresenter UnloadUi(Type type);
		
		/// <summary>
		/// Checks if the <seealso cref="UiPresenter"/> of the given <typeparamref name="T"/> is loaded or not
		/// </summary>
		bool IsUiLoaded<T>() where T : UiPresenter;
		
		/// <summary>
		/// Checks if the <seealso cref="UiPresenter"/> of the given <paramref name="type"/> is loaded or not 
		/// </summary>
		bool IsUiLoaded(Type type);
		
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
		/// <returns></returns>
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
		T OpenUi<T, TData>(TData initialData) where T : UiPresenter where TData : struct;

		///<inheritdoc cref="OpenUi(Type)"/>
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
		/// Loads asynchronously all the <see cref="UiPresenter"/> from given <paramref name="uiSetId "/> and have not yet been loaded.
		/// This method can be controlled in an async method and returns every UI when completes loaded.
		/// This method can be controlled in a foreach loop and it will return the UIs in a first-load-first-return scheme 
		/// </summary>
		Task<Task<UiPresenter>>[] LoadUiSetAsync(int uiSetId);

		/// <summary>
		/// Checks if the service contains or not the <seealso cref="UiSetConfig"/> of the given <paramref name="setId"/>
		/// </summary>
		bool HasUiSet(int setId);

		/// <summary>
		/// Checks if all the <seealso cref="UiPresenter"/> belonging in the given <paramref name="setId"/> is loaded or not
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiSetConfig"/> of the given <paramref name="setId"/>
		/// </exception>
		bool IsAllUiLoadedInSet(int setId);
		
		/// <summary>
		/// Requests the <seealso cref="UiSetConfig"/> of given type <paramref name="setId"/>
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiSetConfig"/> of the given <paramref name="setId"/>
		/// </exception>
		UiSetConfig GetUiSet(int setId);
		
		/// <summary>
		/// Opens all the <seealso cref="UiPresenter"/> that are part of the given <paramref name="setId"/>
		/// If the given <paramref name="closeVisibleUi"/> is set to true, will close the currently open <seealso cref="UiPresenter"/>
		/// that are not part of the given <paramref name="setId"/>
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiSetConfig"/> of with the given <paramref name="setId"/>
		/// </exception>
		void OpenUiSet(int setId, bool closeVisibleUi);
		
		/// <summary>
		/// Closes all the <seealso cref="UiPresenter"/> that are part of the given <paramref name="setId"/>
		/// </summary>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the service does NOT contain an <see cref="UiSetConfig"/> of with the given <paramref name="setId"/>
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
		public void AddUiConfig(UiConfig config)
		{
			if (_uiConfigs.ContainsKey(config.UiType))
			{
				throw new ArgumentException($"The UiConfig {config.AddressableAddress} was already added");
			}

			_uiConfigs.Add(config.UiType, config);
		}

		/// <inheritdoc />
		public void AddUi<T>(int layer, T ui) where T : UiPresenter
		{
			AddUi(layer, ui, typeof(T));
		}

		/// <inheritdoc />
		public void AddUi(int layer, UiPresenter ui, Type type)
		{
			if (ui == null)
			{
				throw new NullReferenceException($"The Ui {type} cannot be null");
			}
			
			if (IsUiLoaded(type))
			{
				throw new ArgumentException($"The Ui {type} was already added");
			}

			if (!typeof(UiPresenter).IsAssignableFrom(type))
			{
				throw new InvalidCastException($"The Ui {type} is not of a {typeof(UiPresenter)} type");
			}
			
			var reference = new UiReference
			{
				UiType = type,
				Layer = layer,
				Presenter = ui
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
			ui.transform.SetParent(_layers[layer].transform);
		}

		/// <inheritdoc />
		public async Task<T> LoadUiAsync<T>() where T : UiPresenter
		{
			UiPresenter uiPresenter = await LoadUiAsync(typeof(T));
			
			return uiPresenter as T;
		}

		/// <inheritdoc />
		public async Task<UiPresenter> LoadUiAsync(Type type)
		{
			if (!_uiConfigs.TryGetValue(type, out UiConfig config))
			{
				throw new KeyNotFoundException($"The UiConfig of type {type} was not added to the service. Call {nameof(AddUiConfig)} first");
			}
			
			var operation = Addressables.LoadAssetAsync<GameObject>(config.AddressableAddress);

			await operation.Task;

			if (operation.Status != AsyncOperationStatus.Succeeded)
			{
				throw operation.OperationException;
			}
			
			// ReSharper disable once AccessToStaticMemberViaDerivedType
			var uiPresenter = GameObject.Instantiate(operation.Result).GetComponent<UiPresenter>();

			AddUi(config.Layer, uiPresenter, config.UiType);
			Addressables.Release(operation);
			
			uiPresenter.SetActive(false);

			return uiPresenter;
		}

		/// <inheritdoc />
		public T UnloadUi<T>() where T : UiPresenter
		{
			return UnloadUi(typeof(T)) as T;
		}

		/// <inheritdoc />
		public UiPresenter UnloadUi(Type type)
		{
			if (!_uiViews.TryGetValue(type, out UiReference reference))
			{
				throw new KeyNotFoundException($"The Ui {type} is not present to be removed");
			}
			
			UiPresenter presenter = reference.Presenter;

			_uiViews.Remove(type);

			return presenter;
		}

		/// <inheritdoc />
		public bool IsUiLoaded<T>() where T : UiPresenter
		{
			return IsUiLoaded(typeof(T));
		}

		/// <inheritdoc />
		public bool IsUiLoaded(Type type)
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
			UiPresenter ui = GetUi(type);
			
			if (!_visibleUiList.Contains(type))
			{
				ui.Open();
				_visibleUiList.Add(type);
			}
			else
			{
				ui.Refresh();
			}
			
			return ui;
		}

		/// <inheritdoc />
		public T OpenUi<T, TData>(TData initialData) where T : UiPresenter where TData : struct
		{
			return OpenUi(typeof(T), initialData) as T;
		}

		/// <inheritdoc />
		public UiPresenter OpenUi<TData>(Type type, TData initialData) where TData : struct
		{
			GetUi(type).SetData(initialData);

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
			UiPresenter ui = GetUi(type);
			
			if (_visibleUiList.Contains(type))
			{
				_visibleUiList.Remove(type);
				ui.Close();
			}

			return ui;
		}

		/// <inheritdoc />
		public void CloseAllUi()
		{
			for (int i = 0; i < _visibleUiList.Count; i++)
			{
				GetUi(_visibleUiList[i]).Close();
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
					reference.Presenter.Close();
					_visibleUiList.Remove(reference.UiType);
				}
			}
		}

		/// <inheritdoc />
		public void AddUiSet(UiSetConfig set)
		{
			if (_uiSets.ContainsKey(set.SetId))
			{
				throw new ArgumentException($"The Ui Configuration with the id {set.SetId.ToString()} was already added");
			}
			
			_uiSets.Add(set.SetId, set);
		}

		/// <inheritdoc />
		public Task<Task<UiPresenter>>[] LoadUiSetAsync(int uiSetId)
		{
			var set = GetUiSet(uiSetId);
			var uiTasks = new List<Task<UiPresenter>>();

			for (int i = 0; i < set.UiConfigsType.Count; i++)
			{
				if (IsUiLoaded(set.UiConfigsType[i]))
				{
					continue;
				}
				
				uiTasks.Add(LoadUiAsync(set.UiConfigsType[i]));
			}

			return LoaderUtil.Interleaved(uiTasks);

		}

		/// <inheritdoc />
		public bool HasUiSet(int setId)
		{
			return _uiSets.ContainsKey(setId);
		}

		/// <inheritdoc />
		public bool IsAllUiLoadedInSet(int setId)
		{
			var set = GetUiSet(setId);

			for (var i = 0; i < set.UiConfigsType.Count; i++)
			{
				if (!IsUiLoaded(set.UiConfigsType[i]))
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