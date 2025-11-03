using UnityEngine;
using GameLovers.UiService;

namespace GameLovers.UiService.Examples
{
	/// <summary>
	/// Example demonstrating delayed UI presenters with animations and time delays
	/// </summary>
	public class DelayedPresenterExample : MonoBehaviour
	{
		[SerializeField] private UiConfigs _uiConfigs;
		
		private IUiService _uiService;

		private void Start()
		{
			// Initialize UI Service
			_uiService = new UiService();
			_uiService.Init(_uiConfigs);
			
			Debug.Log("=== Delayed Presenter Example Started ===");
			Debug.Log("Press 1: Show Time-Delayed UI");
			Debug.Log("Press 2: Show Animation-Delayed UI");
			Debug.Log("Press 3: Close Active UI");
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				Debug.Log("Opening Time-Delayed UI (watch for delayed appearance)...");
				_uiService.OpenUi<DelayedUiExamplePresenter>();
			}
			else if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				Debug.Log("Opening Animation-Delayed UI (watch for animation)...");
				_uiService.OpenUi<AnimatedUiExamplePresenter>();
			}
			else if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				Debug.Log("Closing UI...");
				
				if (_uiService.IsUiOpen<DelayedUiExamplePresenter>())
				{
					_uiService.CloseUi<DelayedUiExamplePresenter>(destroy: false);
				}
				else if (_uiService.IsUiOpen<AnimatedUiExamplePresenter>())
				{
					_uiService.CloseUi<AnimatedUiExamplePresenter>(destroy: false);
				}
			}
		}
	}
}

