using UnityEngine;
using UnityEngine.UIElements;

namespace GameLovers.UiService
{
	/// <summary>
	/// Interface for UI Toolkit-based presenters.
	/// </summary>
	public interface IUiPresenter
	{
	}

	/// <summary>
	/// Base presenter for UI Toolkit-based views. Ensures a <see cref="UIDocument"/> is present
	/// and provides derived presenters access to it.
	/// </summary>
	[RequireComponent(typeof(UIDocument))]
	public abstract class UiToolkitPresenter : UiPresenter, IUiPresenter
	{
		[SerializeField] private UIDocument _document;

		/// <summary>
		/// Provides access to the attached <see cref="UIDocument"/> for derived presenters.
		/// </summary>
		protected UIDocument Document => _document;

		/// <summary>
		/// Assigns the serialized <see cref="UIDocument"/> reference from the component on this GameObject.
		/// </summary>
		protected virtual void OnValidate()
		{
			_document = GetComponent<UIDocument>();
		}
	}
}

