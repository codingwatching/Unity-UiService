using UnityEngine;
using GameLovers.UiService;

namespace GameLovers.UiService.Examples
{
	/// <summary>
	/// Example demonstrating data-driven UI presenters
	/// </summary>
	public class DataPresenterExample : MonoBehaviour
	{
		[SerializeField] private UiConfigs _uiConfigs;
		
		private IUiService _uiService;

		private void Start()
		{
			// Initialize UI Service
			_uiService = new UiService();
			_uiService.Init(_uiConfigs);
			
			Debug.Log("=== Data Presenter Example Started ===");
			Debug.Log("Press 1: Show Warrior Data");
			Debug.Log("Press 2: Show Mage Data");
			Debug.Log("Press 3: Show Rogue Data");
			Debug.Log("Press 4: Update to Low Health");
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				ShowWarriorData();
			}
			else if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				ShowMageData();
			}
			else if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				ShowRogueData();
			}
			else if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				UpdateToLowHealth();
			}
		}

		private void ShowWarriorData()
		{
			var data = new PlayerData
			{
				PlayerName = "Thor the Warrior",
				Level = 45,
				Score = 12500,
				HealthPercentage = 0.85f
			};
			
			Debug.Log("Opening UI with Warrior data...");
			_uiService.OpenUi<DataUiExamplePresenter, PlayerData>(data);
		}

		private void ShowMageData()
		{
			var data = new PlayerData
			{
				PlayerName = "Gandalf the Mage",
				Level = 99,
				Score = 50000,
				HealthPercentage = 0.60f
			};
			
			Debug.Log("Opening UI with Mage data...");
			_uiService.OpenUi<DataUiExamplePresenter, PlayerData>(data);
		}

		private void ShowRogueData()
		{
			var data = new PlayerData
			{
				PlayerName = "Shadow the Rogue",
				Level = 33,
				Score = 8900,
				HealthPercentage = 1.0f
			};
			
			Debug.Log("Opening UI with Rogue data...");
			_uiService.OpenUi<DataUiExamplePresenter, PlayerData>(data);
		}

		private void UpdateToLowHealth()
		{
			var data = new PlayerData
			{
				PlayerName = "Wounded Hero",
				Level = 15,
				Score = 3500,
				HealthPercentage = 0.15f
			};
			
			Debug.Log("Updating to low health data...");
			_uiService.OpenUi<DataUiExamplePresenter, PlayerData>(data);
		}
	}
}

