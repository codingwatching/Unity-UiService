using UnityEngine;

namespace GameLovers.UiService
{
	/// <summary>
	/// Abstract base class for UI presenters that delay their opening and closing.
	/// </summary>
	[RequireComponent(typeof(PresenterDelayerBase))]
	public abstract class DelayUiPresenter : UiPresenter
	{
		[SerializeField] private PresenterDelayerBase _delayer;

		private void OnValidate()
		{
			_delayer = _delayer ?? GetComponent<PresenterDelayerBase>();

			OnEditorValidate();
		}

		/// <inheritdoc />
		protected override void OnOpened()
		{
			_ = _delayer.OpenWithDelay(OnOpenedCompleted);
		}

		/// <inheritdoc />
		protected override void OnClosed()
		{
			_ = _delayer.CloseWithDelay(OnClosedCompleted);
		}

		/// <summary>
		/// Called only in the Editor. Called in the end of this object MonoBehaviour's OnValidate() -> <see cref="OnValidate"/>
		/// </summary>
		protected virtual void OnEditorValidate() { }

		/// <summary>
		/// Called when the presenter's opening delay is completed.
		/// </summary>
		protected virtual void OnOpenedCompleted() { }

		/// <summary>
		/// Called when the presenter's closing delay is completed.
		/// </summary>
		protected virtual void OnClosedCompleted() { }

		internal override void InternalClose(bool destroy)
		{
			// Override the behaviour to not allow the presenter to be disabled until delay is done.
			// Not pretty inherithance with it's dependency but it works
			if (destroy)
			{
				base.InternalClose(true);
			}
			else
			{
				OnClosed();
			}
		}
	}

	/// <inheritdoc cref="DelayUiPresenter"/>
	/// <remarks>
	/// Extends the presenter behaviour to hold data of type <typeparamref name="T"/>
	/// </remarks>
	/// <typeparam name="T">The type of data held by the presenter.</typeparam>
	[RequireComponent(typeof(PresenterDelayerBase))]
	public abstract class DelayUiPresenterData<T> : UiPresenter, IUiPresenterData where T : struct
	{
		[SerializeField] private PresenterDelayerBase _delayer;

		private void OnValidate()
		{
			_delayer = _delayer ?? GetComponent<PresenterDelayerBase>();

			OnEditorValidate();
		}

		/// <inheritdoc />
		protected override void OnOpened()
		{
			_ = _delayer.OpenWithDelay(OnOpenedCompleted);
		}

		/// <inheritdoc />
		protected override void OnClosed()
		{
			_ = _delayer.CloseWithDelay(OnClosedCompleted);
		}

		/// <summary>
		/// Called only in the Editor. Called in the end of this object MonoBehaviour's OnValidate() -> <see cref="OnValidate"/>
		/// </summary>
		protected virtual void OnEditorValidate() { }

		/// <summary>
		/// Called when the presenter's opening delay is completed.
		/// </summary>
		protected virtual void OnOpenedCompleted() { }

		/// <summary>
		/// Called when the presenter's closing delay is completed.
		/// </summary>
		protected virtual void OnClosedCompleted() { }

		internal override void InternalClose(bool destroy)
		{
			// Override the behaviour to not allow the presenter to be disabled until delay is done.
			// Not pretty inherithance with it's dependency but it works
			if (destroy)
			{
				base.InternalClose(true);
			}
			else
			{
				OnClosed();
			}
		}
	}
}
