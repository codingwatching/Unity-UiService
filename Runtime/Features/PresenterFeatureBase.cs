using UnityEngine;

namespace GameLovers.UiService
{
	/// <summary>
	/// Base class for presenter features that can be attached to a <see cref="UiPresenter"/> to extend its functionality.
	/// Inherit from this class to create custom features that hook into the presenter lifecycle.
	/// </summary>
	public abstract class PresenterFeatureBase : MonoBehaviour, IPresenterFeature
	{
		/// <summary>
		/// The presenter that owns this feature
		/// </summary>
		protected UiPresenter Presenter { get; private set; }

		/// <inheritdoc />
		public virtual void OnPresenterInitialized(UiPresenter presenter)
		{
			Presenter = presenter;
		}

		/// <inheritdoc />
		public virtual void OnPresenterOpening() { }

		/// <inheritdoc />
		public virtual void OnPresenterOpened() { }

		/// <inheritdoc />
		public virtual void OnPresenterClosing() { }

		/// <inheritdoc />
		public virtual void OnPresenterClosed() { }
	}
}

