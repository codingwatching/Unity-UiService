using System;
using System.Threading.Tasks;
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
		/// Gets the Task of the current's delay process
		/// </summary>
		Task CurrentDelayTask { get; }
	}

	/// <inheritdoc />
	public abstract class PresenterDelayerBase : MonoBehaviour, IPresenterDelayer
	{
		/// <inheritdoc />
		public abstract float OpenDelayInSeconds { get; }
		/// <inheritdoc />
		public abstract float CloseDelayInSeconds { get; }
		/// <inheritdoc />
		public Task CurrentDelayTask { get; protected set; }

		/// <summary>
		/// Called when the presenter's opening delay starts.
		/// </summary>
		protected virtual void OnOpenStarted() { }

		/// <summary>
		/// Called when the presenter's closing delay starts.
		/// </summary>
		protected virtual void OnCloseStarted() { }

	internal async Task OpenWithDelay(Action onOpenedCompleted)
	{
		OnOpenStarted();

		CurrentDelayTask = Task.Delay(Mathf.RoundToInt(OpenDelayInSeconds * 1000));

		await CurrentDelayTask;

		if (gameObject != null)
		{
			onOpenedCompleted();
		}
	}

	internal async Task CloseWithDelay(Action onCloseCompleted)
	{
		OnCloseStarted();

		CurrentDelayTask = Task.Delay(Mathf.RoundToInt(CloseDelayInSeconds * 1000));

		await CurrentDelayTask;

		if (gameObject != null)
		{
			gameObject.SetActive(false);
			onCloseCompleted();
		}
	}
	}
}
