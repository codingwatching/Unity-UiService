using UnityEngine;
using GameLovers.UiService;

namespace GameLovers.UiService.Examples
{
	/// <summary>
	/// Example demonstrating UI Toolkit integration with UiService
	/// </summary>
	public class UiToolkitExample : MonoBehaviour
	{
		[SerializeField] private UiConfigs _uiConfigs;
		
		private IUiService _uiService;

		private void Start()
		{
			// Initialize UI Service
			_uiService = new UiService();
			_uiService.Init(_uiConfigs);
			
			Debug.Log("=== UI Toolkit Example Started ===");
			Debug.Log("Press 1: Open UI Toolkit UI");
			Debug.Log("Press 2: Close UI Toolkit UI");
			Debug.Log("");
			Debug.Log("Note: Make sure to create a UXML document and assign it to the UIDocument component");
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				Debug.Log("Opening UI Toolkit UI...");
				_uiService.OpenUi<UiToolkitExamplePresenter>();
			}
			else if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				Debug.Log("Closing UI Toolkit UI...");
				_uiService.CloseUi<UiToolkitExamplePresenter>(destroy: false);
			}
		}
	}
}

