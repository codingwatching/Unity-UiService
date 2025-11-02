using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameLovers.UiService
{
	/// <summary>
	/// Abstract base class for UI Toolkit-based presenters that delay their opening and closing.
	/// Combines the functionality of <see cref="UiToolkitPresenter"/> with delay capabilities from <see cref="DelayUiPresenter"/>.
	/// </summary>
	[RequireComponent(typeof(PresenterDelayerBase))]
	public abstract class DelayUiToolkitPresenter : UiToolkitPresenter
	{
		[SerializeField] private PresenterDelayerBase _delayer;

		/// <summary>
		/// Provides access to the attached <see cref="PresenterDelayerBase"/> for derived presenters.
		/// </summary>
		protected PresenterDelayerBase Delayer => _delayer;

		/// <inheritdoc />
		protected override void OnValidate()
		{
			base.OnValidate();

			_delayer = _delayer != null ? _delayer : GetComponent<PresenterDelayerBase>();

			OnEditorValidate();
		}

		/// <inheritdoc />
		protected override void OnOpened()
		{
			_delayer.OpenWithDelay(OnOpenedCompleted).Forget();
		}

		/// <inheritdoc />
		protected override void OnClosed()
		{
			_delayer.CloseWithDelay(OnClosedCompleted).Forget();
		}

		/// <summary>
		/// Called only in the Editor. Called at the end of this object MonoBehaviour's OnValidate() -> <see cref="OnValidate"/>
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
			// Not pretty inheritance with its dependency but it works
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

	/// <inheritdoc cref="DelayUiToolkitPresenter"/>
	/// <remarks>
	/// Extends the presenter behaviour to hold data of type <typeparamref name="T"/>
	/// </remarks>
	/// <typeparam name="T">The type of data held by the presenter.</typeparam>
	[RequireComponent(typeof(PresenterDelayerBase))]
	public abstract class DelayUiToolkitPresenter<T> : UiToolkitPresenter<T> where T : struct
	{
		[SerializeField] private PresenterDelayerBase _delayer;

		/// <summary>
		/// Provides access to the attached <see cref="PresenterDelayerBase"/> for derived presenters.
		/// </summary>
		protected PresenterDelayerBase Delayer => _delayer;

		/// <inheritdoc />
		protected override void OnValidate()
		{
			base.OnValidate();

			_delayer = _delayer ?? GetComponent<PresenterDelayerBase>();

			OnEditorValidate();
		}

		/// <inheritdoc />
		protected override void OnOpened()
		{
			_delayer.OpenWithDelay(OnOpenedCompleted).Forget();
		}

		/// <inheritdoc />
		protected override void OnClosed()
		{
			_delayer.CloseWithDelay(OnClosedCompleted).Forget();
		}

		/// <summary>
		/// Called only in the Editor. Called at the end of this object MonoBehaviour's OnValidate() -> <see cref="OnValidate"/>
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
			// Not pretty inheritance with its dependency but it works
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

