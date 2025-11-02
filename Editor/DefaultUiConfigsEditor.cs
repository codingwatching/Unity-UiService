using GameLovers.UiService;
using UnityEditor;

namespace GameLoversEditor.UiService
{
	/// <summary>
	/// Default UI Set identifiers for out-of-the-box usage.
	/// Users can create their own enum and custom editor to override these defaults.
	/// </summary>
	public enum DefaultUiSetId
	{
		None = 0,
		MainMenu = 1,
		Gameplay = 2,
		Settings = 3,
		Overlay = 4,
		Popup = 5
	}

	/// <summary>
	/// Default implementation of the UiConfigs editor.
	/// This allows the library to work out-of-the-box without requiring user implementation.
	/// Users can override by creating their own CustomEditor implementation for UiConfigs.
	/// </summary>
	[CustomEditor(typeof(UiConfigs))]
	public class DefaultUiConfigsEditor : UiConfigsEditor<DefaultUiSetId>
	{
		// No additional implementation needed - uses base class functionality
	}
}