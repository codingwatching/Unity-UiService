using UnityEngine;
using GameLovers.UiService;

namespace GameLovers.UiService.Examples
{
	/// <summary>
	/// Example demonstrating basic UI flow: loading, opening, closing, unloading
	/// </summary>
	public class BasicUiFlowExample : MonoBehaviour
	{
		[SerializeField] private UiConfigs _uiConfigs;
		
		private IUiService _uiService;

		private void Start()
		{
			// Initialize UI Service
			_uiService = new UiService();
			_uiService.Init(_uiConfigs);
			
			Debug.Log("=== Basic UI Flow Example Started ===");
			Debug.Log("Press 1: Load UI");
			Debug.Log("Press 2: Open UI");
			Debug.Log("Press 3: Close UI (keep in memory)");
			Debug.Log("Press 4: Unload UI (destroy)");
			Debug.Log("Press 5: Load & Open (combined)");
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				Debug.Log("Loading BasicUiExamplePresenter...");
				_uiService.LoadUi<BasicUiExamplePresenter>();
				Debug.Log("UI Loaded (but not visible yet)");
			}
			else if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				Debug.Log("Opening BasicUiExamplePresenter...");
				_uiService.OpenUi<BasicUiExamplePresenter>();
				Debug.Log("UI Opened and visible");
			}
			else if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				Debug.Log("Closing BasicUiExamplePresenter (destroy: false)...");
				_uiService.CloseUi<BasicUiExamplePresenter>(destroy: false);
				Debug.Log("UI Closed but still in memory");
			}
			else if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				Debug.Log("Unloading BasicUiExamplePresenter...");
				_uiService.UnloadUi<BasicUiExamplePresenter>();
				Debug.Log("UI Destroyed and removed from memory");
			}
			else if (Input.GetKeyDown(KeyCode.Alpha5))
			{
				Debug.Log("Loading and Opening BasicUiExamplePresenter (combined)...");
				_uiService.LoadUi<BasicUiExamplePresenter>(openAfter: true);
				Debug.Log("UI Loaded and Opened in one call");
			}
		}
	}
}

