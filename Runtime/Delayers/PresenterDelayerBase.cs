using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace GameLovers.UiService
{
	/// <summary>
	/// Contract for presenter delayers, which handle delays for opening and closing presenters.
	/// </summary>
	public interface IPresenterDelayer
	{
		/// <summary>
		/// Gets the delay in seconds before opening the presenter.
		/// </summary>
		float OpenDelayInSeconds { get; }

		/// <summary>
		/// Gets the delay in seconds before closing the presenter.
		/// </summary>
		float CloseDelayInSeconds { get; }

	/// <summary>
	/// Gets the UniTask of the current's delay process
	/// </summary>
	UniTask CurrentDelayTask { get; }
	}

	/// <inheritdoc />
	public abstract class PresenterDelayerBase : MonoBehaviour, IPresenterDelayer
	{
	/// <inheritdoc />
	public abstract float OpenDelayInSeconds { get; }
	/// <inheritdoc />
	public abstract float CloseDelayInSeconds { get; }
	/// <inheritdoc />
	public UniTask CurrentDelayTask { get; protected set; }

		/// <summary>
		/// Called when the presenter's opening delay starts.
		/// </summary>
		protected virtual void OnOpenStarted() { }

		/// <summary>
		/// Called when the presenter's closing delay starts.
		/// </summary>
		protected virtual void OnCloseStarted() { }

internal async UniTask OpenWithDelay(Action onOpenedCompleted)
{
	OnOpenStarted();

	CurrentDelayTask = UniTask.Delay(TimeSpan.FromSeconds(OpenDelayInSeconds));

	await CurrentDelayTask;

	if (gameObject != null)
	{
		onOpenedCompleted();
	}
}

internal async UniTask CloseWithDelay(Action onCloseCompleted)
{
	OnCloseStarted();

	CurrentDelayTask = UniTask.Delay(TimeSpan.FromSeconds(CloseDelayInSeconds));

	await CurrentDelayTask;

	if (gameObject != null)
	{
		gameObject.SetActive(false);
		onCloseCompleted();
	}
}
	}
}
