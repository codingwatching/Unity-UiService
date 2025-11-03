using UnityEngine;
using UnityEngine.UIElements;

namespace GameLovers.UiService
{
	/// <summary>
	/// Feature that provides UI Toolkit integration for a <see cref="UiPresenter"/>.
	/// Ensures a <see cref="UIDocument"/> is present and provides access to it.
	/// </summary>
	[RequireComponent(typeof(UIDocument))]
	public class UiToolkitPresenterFeature : PresenterFeatureBase
	{
		[SerializeField] private UIDocument _document;

		/// <summary>
		/// Provides access to the attached <see cref="UIDocument"/>
		/// </summary>
		public UIDocument Document => _document;

		/// <summary>
		/// The root element of the <see cref="UIDocument"/>
		/// </summary>
		public VisualElement Root => _document?.rootVisualElement;

		private void OnValidate()
		{
			_document = _document ?? GetComponent<UIDocument>();
		}
	}
}

