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
		/// <summary>
		/// Sets the UI <paramref name="data"/>
		/// </summary>
		public virtual void SetData<T>(T data) where T : struct {}
		
		internal void Refresh()
		{
			OnRefresh();
		}

		internal void Open()
		{
			gameObject.SetActive(true);
			OnOpened();
		}

		internal void Close()
		{
			gameObject.SetActive(false);
			OnClosed();
		}

		protected virtual void OnRefresh() {}
		
		protected virtual void OnOpened() {}

		protected virtual void OnClosed() {}
	}
}