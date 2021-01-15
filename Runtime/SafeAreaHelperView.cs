using System;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable CheckNamespace

namespace FirstLight.UiService
{
	/// <summary>
	/// This view helper translate anchored views based on device safe area (screens witch a notch)
	/// </summary>
	[RequireComponent(typeof(RectTransform))]
	public class SafeAreaHelperView : MonoBehaviour
	{
		private const float _floatTolerance = 0.01f;
		
		[SerializeField] private RectTransform _rectTransform;
		[SerializeField] private bool _ignoreWidth = true;
		[SerializeField] private bool _onUpdate = false;
		[SerializeField] private Vector2 _refResolution;

		private Vector2 _initAnchoredPosition;
		private Rect _resolution;
		private Rect _safeArea;

		internal void OnValidate()
		{
			_rectTransform = _rectTransform ? _rectTransform : GetComponent<RectTransform>();
			_refResolution = transform.root.GetComponent<CanvasScaler>().referenceResolution;
			_initAnchoredPosition = _rectTransform.anchoredPosition;
		}

		private void Awake()
		{
			_initAnchoredPosition = _rectTransform.anchoredPosition;
			_resolution = new Rect(0,0, Screen.currentResolution.width, Screen.currentResolution.height);
			_safeArea = Screen.safeArea;
		}

		private void OnEnable()
		{
			UpdatePositions();
		}

		private void Update()
		{
			if (_onUpdate)
			{
				UpdatePositions();
			}
		}

		internal void UpdatePositions()
		{
			var anchorMax = _rectTransform.anchorMax;
			var anchorMin = _rectTransform.anchorMin;
			var anchoredPosition = _initAnchoredPosition;
			
#if UNITY_EDITOR
			// Because Unity Device Simulator and Game View have different screen resolution configs and sometimes use Desktop resolution
			_safeArea = Screen.safeArea;
			_resolution = new Rect(0, 0, Screen.width, Screen.height);
			_resolution = _resolution == _safeArea ? _resolution : new Rect(0,0, Screen.currentResolution.width, Screen.currentResolution.height); 
#endif

			if (_safeArea == _resolution)
			{
				return;
			}
			
			// Check if anchored to top or bottom
			if (Math.Abs(anchorMax.y - anchorMin.y) < _floatTolerance)
			{
				// bottom
				if (anchorMax.y < _floatTolerance)
				{
					anchoredPosition.y += (_safeArea.yMin - _resolution.yMin) * _refResolution.y / _resolution.height; 
				}
				else // top
				{
					anchoredPosition.y += (_safeArea.yMax - _resolution.yMax) * _refResolution.y / _resolution.height;
				}
			}
			
			// Check if anchored to left or right
			if (!_ignoreWidth && Math.Abs(anchorMax.x - anchorMin.x) < _floatTolerance)
			{
				// left
				if (anchorMax.x < _floatTolerance)
				{
					anchoredPosition.x += (_safeArea.xMin - _resolution.xMin) * _refResolution.x / _resolution.width;
				}
				else // right
				{
					anchoredPosition.x += (_safeArea.xMax - _resolution.xMax) * _refResolution.x / _resolution.width;
				}
			}
			
			_rectTransform.anchoredPosition = anchoredPosition;
		}
	}
	
	#if UNITY_EDITOR
	[UnityEditor.CustomEditor(typeof(SafeAreaHelperView))]
	public class SafeAreaHelperViewEditor : UnityEditor.Editor 
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Update Anchored Data"))
			{
				var view = (SafeAreaHelperView) target;
				
				view.OnValidate();
			}

			if (GUILayout.Button("Update Anchored View"))
			{
				var view = (SafeAreaHelperView) target;
				
				view.UpdatePositions();
			}
		}
	}
	#endif
}