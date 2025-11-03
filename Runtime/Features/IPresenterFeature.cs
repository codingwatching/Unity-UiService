namespace GameLovers.UiService
{
	/// <summary>
	/// Interface for composable presenter features that extend the lifecycle of a <see cref="UiPresenter"/>.
	/// Features can be attached to presenters to add functionality like delays, animations, or UI Toolkit integration.
	/// </summary>
	public interface IPresenterFeature
	{
		/// <summary>
		/// Called when the presenter is initialized. This is invoked once when the presenter is created.
		/// </summary>
		/// <param name="presenter">The presenter that owns this feature</param>
		void OnPresenterInitialized(UiPresenter presenter);

		/// <summary>
		/// Called when the presenter is being opened. This is invoked before the presenter becomes visible.
		/// </summary>
		void OnPresenterOpening();

		/// <summary>
		/// Called after the presenter has been opened and is now visible.
		/// </summary>
		void OnPresenterOpened();

		/// <summary>
		/// Called when the presenter is being closed. This is invoked before the presenter becomes hidden.
		/// </summary>
		void OnPresenterClosing();

		/// <summary>
		/// Called after the presenter has been closed and is no longer visible.
		/// </summary>
		void OnPresenterClosed();
	}
}

