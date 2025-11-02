using GameLovers.UiService;
using UnityEngine;

namespace GameLovers.UiServiceExamples
{
	/// <summary>
	/// Example demonstrating the <see cref="DelayUiToolkitPresenter"/> functionality.
	/// This example shows how to combine UI Toolkit rendering with delay-based transitions.
	/// 
	/// Press number keys to test different scenarios:
	/// - 1: Open time-delayed UI Toolkit presenter
	/// - 2: Open animation-delayed UI Toolkit presenter with data
	/// - 3: Close all UIs
	/// </summary>
	public class DelayedUiToolkitExample : MonoBehaviour
	{
		[SerializeField] private UiConfigs _uiConfigs;

		private IUiService _uiService;

		private void Start()
		{
			// Initialize the UI service
			_uiService = new UiService(_uiConfigs, new UiAssetLoader());

			Debug.Log("=== Delayed UI Toolkit Example ===");
			Debug.Log("Press 1: Open Time-Delayed UI Toolkit");
			Debug.Log("Press 2: Open Animation-Delayed UI Toolkit with Data");
			Debug.Log("Press 3: Close All UIs");
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				OpenTimeDelayedUiToolkit();
			}
			else if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				OpenAnimationDelayedUiToolkit();
			}
			else if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				CloseAllUis();
			}
		}

		/// <summary>
		/// Opens a UI Toolkit presenter with time-based delays
		/// </summary>
		private async void OpenTimeDelayedUiToolkit()
		{
			Debug.Log("Opening Time-Delayed UI Toolkit Presenter...");
			await _uiService.OpenUi<TimeDelayedUiToolkitPresenter>();
			Debug.Log("Time-Delayed UI Toolkit Presenter opened!");
		}

		/// <summary>
		/// Opens a UI Toolkit presenter with animation-based delays and data
		/// </summary>
		private async void OpenAnimationDelayedUiToolkit()
		{
			var data = new UiToolkitExampleData
			{
				Title = "Animated UI Toolkit",
				Message = "This UI uses animation delays!",
				Score = Random.Range(100, 999)
			};

			Debug.Log($"Opening Animation-Delayed UI Toolkit with data: {data.Title}");
			await _uiService.OpenUi<AnimationDelayedUiToolkitPresenter, UiToolkitExampleData>(data);
			Debug.Log("Animation-Delayed UI Toolkit Presenter opened!");
		}

		/// <summary>
		/// Closes all open UIs
		/// </summary>
		private void CloseAllUis()
		{
			Debug.Log("Closing all UIs...");
			_uiService.CloseUi<TimeDelayedUiToolkitPresenter>();
			_uiService.CloseUi<AnimationDelayedUiToolkitPresenter>();
		}
	}
}

