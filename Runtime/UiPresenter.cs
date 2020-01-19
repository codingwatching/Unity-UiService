using UnityEngine;

// ReSharper disable CheckNamespace

namespace GameLovers.UiService
{
	/// <summary>
	/// The root base of the UI Presenter of the <seealso cref="IUiService"/>
	/// Implement this abstract class in order to execute the proper UI life cycle
	/// </summary>
	public abstract class UiPresenter : MonoBehaviour
	{
		private IUiService _uiService;
		
		/// <summary>
		/// Sets the UI <paramref name="data"/>
		/// </summary>
		public virtual void SetData<T>(T data) where T : struct {}

		/// <summary>
		/// Refreshes this opened UI
		/// </summary>
		public virtual void Refresh() {}
		
		/// <summary>
		/// Allows the ui presenter implementation to have extra behaviour when it is opened
		/// </summary>
		protected virtual void OnOpened() {}

		/// <summary>
		/// Allows the ui presenter implementation to have extra behaviour when it is closed
		/// </summary>
		protected virtual void OnClosed() {}

		/// <summary>
		/// Allows the ui presenter implementation to directly close the ui presenter without needing to call the service directly
		/// </summary>
		protected void Close()
		{
			_uiService.CloseUi(this);
		}

		internal void Init(IUiService uiService)
		{
			_uiService = uiService;
		}

		internal void InternalOpen()
		{
			gameObject.SetActive(true);
			OnOpened();
		}

		internal void InternalClose()
		{
			gameObject.SetActive(false);
			OnClosed();
		}
	}
}