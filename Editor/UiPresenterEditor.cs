using UnityEditor;
using UnityEngine;
using GameLovers.UiService;

// ReSharper disable once CheckNamespace

namespace GameLoversEditor.UiService
{
	/// <summary>
	/// Custom editor for UiPresenter to show quick actions in the inspector
	/// </summary>
	[CustomEditor(typeof(UiPresenter), true)]
	public class UiPresenterEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			
			EditorGUILayout.Space(10);
			EditorGUILayout.LabelField("UI Presenter Controls", EditorStyles.boldLabel);
			
			var presenter = (UiPresenter)target;
			
			// Status display
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Status:", GUILayout.Width(100));
			
			var statusColor = presenter.IsOpen ? Color.green : Color.red;
			var originalColor = GUI.backgroundColor;
			GUI.backgroundColor = statusColor;
			
			EditorGUILayout.LabelField(presenter.IsOpen ? "OPEN" : "CLOSED", EditorStyles.boldLabel);
			GUI.backgroundColor = originalColor;
			
			EditorGUILayout.EndHorizontal();
			
			// Play mode controls
			if (Application.isPlaying)
			{
				DrawPlayModeControls(presenter);
			}
			else
			{
				EditorGUILayout.HelpBox("UI controls are only available in Play Mode", MessageType.Info);
			}
		}

		private void DrawPlayModeControls(UiPresenter presenter)
		{
			EditorGUILayout.Space(5);
			
			EditorGUILayout.BeginHorizontal();
			
			if (GUILayout.Button("Open UI", GUILayout.Height(30)))
			{
				// In play mode, we need to activate directly since service might not be initialized
				presenter.gameObject.SetActive(true);
			}
			
			if (GUILayout.Button("Close UI", GUILayout.Height(30)))
			{
				presenter.gameObject.SetActive(false);
			}
			
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			
			if (GUILayout.Button("Close (No Destroy)", GUILayout.Height(25)))
			{
				presenter.gameObject.SetActive(false);
			}
			
			if (GUILayout.Button("Close (Destroy)", GUILayout.Height(25)))
			{
				if (EditorUtility.DisplayDialog("Destroy UI", 
					"Are you sure you want to destroy this UI?", "Yes", "Cancel"))
				{
					DestroyImmediate(presenter.gameObject);
				}
			}
			
			EditorGUILayout.EndHorizontal();
		}
	}
}

