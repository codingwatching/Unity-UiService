using System;
using System.Collections.Generic;
using GameLovers.UiService;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace

namespace GameLoversEditor.UiService
{
	/// <summary>
	/// Helps selecting the <see cref="UiConfigs"/> asset file in the Editor
	/// </summary>
	public static class UiConfigsSelect
	{
		
		[MenuItem("Tools/Select UiConfigs.asset")]
		private static void SelectUiConfigs()
		{
			var assets = AssetDatabase.FindAssets($"t:{nameof(UiConfigs)}");
			var scriptableObject = assets.Length > 0 ? 
				AssetDatabase.LoadAssetAtPath<UiConfigs>(AssetDatabase.GUIDToAssetPath(assets[0])) :
				ScriptableObject.CreateInstance<UiConfigs>();

			if (assets.Length == 0)
			{
				AssetDatabase.CreateAsset(scriptableObject, $"Assets/{nameof(UiConfigs)}.asset");
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}

			Selection.activeObject = scriptableObject;
		}
	}
	
	/// <summary>
	/// Improves the inspector visualization for the <see cref="UiConfigs"/> scriptable object
	/// </summary>
	/// <remarks>
	/// This abstract class should be implemented in game project being used.
	/// 
	/// Ex:
	/// [CustomEditor(typeof(UiConfigs))]
	/// public class GameUiConfigsEditor : UiConfigsEditor<![CDATA[<UiSetId>]]>
	/// {
	/// }
	/// </remarks>
	/// <typeparam name="TSet">The enum type of the <see cref="UiSetConfig"/> id </typeparam>
	public abstract class UiConfigsEditor<TSet> : Editor
		where TSet : Enum
	{
		private readonly List<string> _assetsPath = new List<string>();
		private readonly List<string> _uiConfigsType = new List<string>();
		private readonly List<ReorderableList> _setsConfigsList = new List<ReorderableList>();
		private readonly GUIContent _uiConfigGuiContent = new GUIContent("Ui Config",
			"All the Addressable addresses for every UiPresenter in the game.\n" +
			"The second field is the layer where the UiPresenter should be shown. " +
			"The higher the value, the closer is the UiPresenter to the camera.\n" +
			"If the UiPresenter contains a Canvas/UIDocument in the root, the layer value is the same of the UI sorting order");
		private readonly GUIContent _uiSetConfigGuiContent = new GUIContent("Ui Set",
			"All the Ui Sets in the game.\n" +
			"A UiSet groups a list of UiConfigs and shows them all at the same time via the UiService.\n" +
			"The UiConfigs are all loaded in the order they are configured. Top = first; Bottom = Last");

		private string[] _uiConfigsAddress;
		private SerializedProperty _configsProperty;
		private SerializedProperty _setsProperty;
		private ReorderableList _configList;
		private ReorderableList _setList;
		private bool _resetValues;
		private UiConfigs _scriptableObject;

		private void OnEnable()
		{
			_resetValues = false;

			InitConfigValues();
			InitReorderableLists();

			_setList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, $"Ui {nameof(UiConfigs.Sets)}");
			_setList.elementHeightCallback = index => _setsConfigsList[index].GetHeight();
			_setList.drawElementCallback = (rect, index, active, focused) => _setsConfigsList[index].DoList(rect);
			_configList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, $"Ui {nameof(UiConfigs.Configs)}");
			_configList.drawElementCallback = DrawUiConfigElement;
		}

		/// <inheritdoc />
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			LoadingSpinnerLayout();

			EditorGUILayout.Space();
			EditorGUILayout.HelpBox(_uiConfigGuiContent.tooltip, MessageType.Info);
			_configList.DoLayoutList();
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			EditorGUILayout.Space();
			EditorGUILayout.HelpBox(_uiSetConfigGuiContent.tooltip, MessageType.Info);
			_setList.DoLayoutList();

			serializedObject.ApplyModifiedProperties();

			if (_resetValues)
			{
				OnEnable();
			}
		}

		private void LoadingSpinnerLayout()
		{
			var uiPresentersNames = new List<string> { "<None>" };
			var uiPresentersAssemblyNames = new List<string> { "<None>" };

			foreach (var uiConfig in _scriptableObject.Configs)
			{
				uiPresentersNames.Add(uiConfig.UiType.Name);
				uiPresentersAssemblyNames.Add(uiConfig.UiType.AssemblyQualifiedName);
			}

			var selectedIndex = 0;
			if (_scriptableObject.LoadingSpinnerType != null)
			{
				selectedIndex = uiPresentersAssemblyNames.FindIndex(uiPresenterName =>
																		_scriptableObject.LoadingSpinnerTypeString ==
																		uiPresenterName);
				selectedIndex = Math.Max(selectedIndex, 0);
			}

			selectedIndex = EditorGUILayout.Popup("Loading Spinner Presenter", selectedIndex, uiPresentersNames.ToArray());
			_scriptableObject.LoadingSpinnerTypeString =
				selectedIndex == 0 ? null : uiPresentersAssemblyNames[selectedIndex];
		}

		private void InitConfigValues()
		{
			var assetList = GetAssetList();
			var gameObjectType = typeof(GameObject);
			var uiConfigsAddress = new List<string>();
			var configs = new List<UiConfig>();
			_scriptableObject = target as UiConfigs;

			_uiConfigsType.Clear();
			_assetsPath.Clear();

			if (_scriptableObject == null)
			{
				throw new NullReferenceException($"The Object is not of type {nameof(UiConfigs)}");
			}

			var configsCache = _scriptableObject.Configs;

			for (int i = 0; i < assetList.Count; i++)
			{
				var assetAddress = assetList[i].address;

				if (AssetDatabase.GetMainAssetTypeAtPath(assetList[i].AssetPath) != gameObjectType)
				{
					continue;
				}

				var uiPresenter = AssetDatabase.LoadAssetAtPath<UiPresenter>(assetList[i].AssetPath);

				if (uiPresenter == null)
				{
					continue;
				}

				_assetsPath.Add(assetList[i].AssetPath);

				var sortingOrder = -1;
				if (uiPresenter.TryGetComponent<Canvas>(out var canvas))
				{
					sortingOrder = canvas.sortingOrder;
				}
				else if (uiPresenter.TryGetComponent<UIDocument>(out var document))
				{
					sortingOrder = (int) document.sortingOrder;
				}
				
				var indexMatch = configsCache.FindIndex(configCheck => configCheck.AddressableAddress == assetAddress);
				var type = uiPresenter.GetType();
				var config = new UiConfig
				{
					AddressableAddress = assetList[i].address,
					Layer = sortingOrder < 0 ? 0 : sortingOrder,
					UiType = type,
					LoadSynchronously = Attribute.IsDefined(type, typeof(LoadSynchronouslyAttribute))

				};

				if (indexMatch > -1)
				{
					uiConfigsAddress.Add(config.AddressableAddress);
					_uiConfigsType.Add(config.UiType.AssemblyQualifiedName);

					config.Layer = sortingOrder < 0 ? configsCache[indexMatch].Layer : config.Layer;
				}

				configs.Add(config);
			}

			_scriptableObject.Configs = configs;
			_uiConfigsAddress = uiConfigsAddress.ToArray();

			EditorUtility.SetDirty(_scriptableObject);
			AssetDatabase.SaveAssets();
			Resources.UnloadUnusedAssets();
		}

		private void InitReorderableLists()
		{
			var enumNames = Enum.GetNames(typeof(TSet));
			var scriptableObject = target as UiConfigs;

			if (scriptableObject == null)
			{
				throw new NullReferenceException($"The Object is not of type {nameof(UiConfigs)}");
			}

			scriptableObject.SetSetsSize(Enum.GetNames(typeof(TSet)).Length);

			_configsProperty = serializedObject.FindProperty("_configs");
			_setsProperty = serializedObject.FindProperty("_sets");
			_configList = new ReorderableList(serializedObject, _configsProperty, false, true, false, false);
			_setList = new ReorderableList(serializedObject, _setsProperty, false, true, false, false);

			_setsConfigsList.Clear();
			_setsConfigsList.Capacity = enumNames.Length;
			_configList.onChangedCallback = reorderableList => _resetValues = true;

			for (int i = 0; i < enumNames.Length; i++)
			{
				var property = _setsProperty.GetArrayElementAtIndex(i).FindPropertyRelative($"{nameof(UiConfigs.UiSetConfigSerializable.UiConfigsType)}");
				var enumName = enumNames[i];
				var list = new ReorderableList(serializedObject, property, true, true, true, true);

				list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, $"{enumName} Set");
				list.onCanAddCallback = reorderableList => _configList.count > 0;
				list.drawElementCallback = (rect, index, active, focused) =>
				{
					var serializedProperty = property.GetArrayElementAtIndex(index);
					var typeIndex = string.IsNullOrEmpty(serializedProperty.stringValue) ?
						0 : _uiConfigsType.FindIndex(type => type == serializedProperty.stringValue);

					serializedProperty.stringValue = _uiConfigsType[EditorGUI.Popup(rect, typeIndex, _uiConfigsAddress)];
				};

				_setsConfigsList.Add(list);
			}
		}

		private void DrawUiConfigElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			const int layerRectWidth = 100;

			var addressRect = new Rect(rect.x, rect.y, rect.width - layerRectWidth - 5, rect.height);
			var layerRect = new Rect(addressRect.xMax + 5, rect.y, layerRectWidth, rect.height);
			var arrayElement = _configsProperty.GetArrayElementAtIndex(index);
			var address = arrayElement.FindPropertyRelative($"{nameof(UiConfig.AddressableAddress)}");
			var layer = arrayElement.FindPropertyRelative($"{nameof(UiConfig.Layer)}");
			var previousLayer = layer.intValue;

			_uiConfigGuiContent.text = address.stringValue;

			EditorGUI.LabelField(addressRect, _uiConfigGuiContent);
			var newLayer = EditorGUI.IntField(layerRect, previousLayer);

			if (newLayer == previousLayer)
			{
				return;
			}

			layer.intValue = newLayer;

			var ui = AssetDatabase.LoadAssetAtPath<GameObject>(_assetsPath[index]);
			if (ui.TryGetComponent<Canvas>(out var canvas))
			{
				canvas.sortingOrder = newLayer;

				EditorUtility.SetDirty(canvas);
				AssetDatabase.SaveAssets();
			}
			else if (ui.TryGetComponent<UIDocument>(out var document))
			{
				document.sortingOrder = newLayer;

				EditorUtility.SetDirty(document);
				AssetDatabase.SaveAssets();
			}

			Resources.UnloadUnusedAssets();
		}
		
		private static List<AddressableAssetEntry> GetAssetList()
		{
			var assetList = new List<AddressableAssetEntry>();
			var assetsSettings = AddressableAssetSettingsDefaultObject.Settings;

			foreach (var settingsGroup in assetsSettings.groups)
			{
				if (settingsGroup.ReadOnly)
				{
					continue;
				}

				settingsGroup.GatherAllAssets(assetList, true, true, true);
			}

			return assetList;
		}
	}
}