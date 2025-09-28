using UnityEngine;
using UnityEngine.UIElements;

namespace GameLovers.UiService
{
	/// /// <summary>
	/// Tags the <see cref="UiToolkitPresenter"/> as a <see cref="UiToolkitPresenterData{T}"/> to allow defining a
	/// specific state when opening the UI via the <see cref="UiService"/>
	public interface IUiToolkitPresenterData : IUiPresenterData
	{
	}

	/// <summary>
	/// Base presenter for UI Toolkit-based views. Ensures a <see cref="UIDocument"/> is present
	/// and provides derived presenters access to it.
	/// </summary>
	[RequireComponent(typeof(UIDocument))]
	public abstract class UiToolkitPresenter : UiPresenter
	{
		[SerializeField] private UIDocument _document;

		/// <summary>
		/// Provides access to the attached <see cref="UIDocument"/> for derived presenters.
		/// </summary>
		protected UIDocument Document => _document;
		
		/// <summary>
		/// The root element of the <see cref="UIDocument"/> defining this presenter
		/// </summary>
		protected VisualElement Root => _document.rootVisualElement;

		/// <summary>
		/// Assigns the serialized <see cref="UIDocument"/> reference from the component on this GameObject.
		/// </summary>
		protected virtual void OnValidate()
		{
			_document = _document == null ? GetComponent<UIDocument>() : _document;
		}
	}
	
	/// <inheritdoc cref="UiToolkitPresenter"/>
	/// <remarks>
	/// Extends the <see cref="UiToolkitPresenter"/> behaviour to hold data of type <typeparamref name="T"/>
	/// </remarks>
	public abstract class UiToolkitPresenter<T> : UiToolkitPresenter, IUiToolkitPresenterData where T : struct
	{
		/// <summary>
		/// The Ui data defined when opened via the <see cref="UiService"/>
		/// </summary>
		public T Data { get; protected set; }

		/// <summary>
		/// Allows the ui presenter implementation to have extra behaviour when the data defined for the presenter is set
		/// </summary>
		protected virtual void OnSetData() {}

		internal void InternalSetData(T data)
		{
			Data = data;

			OnSetData();
		}
	}
}

