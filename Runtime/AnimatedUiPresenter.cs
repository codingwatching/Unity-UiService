using System.Threading.Tasks;
using UnityEngine;

// ReSharper disable CheckNamespace

namespace GameLovers.UiService
{
	/// <inheritdoc />
	/// <remarks>
	/// Allows this Presenter to have an intro and outro animation when opened and closed to provide feedback and joy for players.
	/// </remarks>
	[RequireComponent(typeof(CanvasGroup))]
	public abstract class AnimatedUiPresenter : UiCloseActivePresenter
	{
		[SerializeField] protected CanvasGroup _canvasGroup;
		[SerializeField] protected Animation _animation;
		[SerializeField] protected AnimationClip _introAnimationClip;
		[SerializeField] protected AnimationClip _outroAnimationClip;

		private void OnValidate()
		{
			_canvasGroup = _canvasGroup ? _canvasGroup : GetComponent<CanvasGroup>();
			
			Debug.Assert(_animation != null, $"Presenter {gameObject.name} does not have a referenced Animation");
			OnEditorValidate();
		}

		protected override async void OnClosed()
		{
			_animation.clip = _outroAnimationClip;
			_animation.Play();

			await Task.Delay(Mathf.RoundToInt(_animation.clip.length * 1000));

			gameObject.SetActive(false);
			OnClosedCompleted();
		}

		protected override async void OnOpened()
		{
			_canvasGroup.alpha = 0;
			_animation.clip = _introAnimationClip;
			_animation.Play();
			await Task.Yield();
			_canvasGroup.alpha = 1;
			await Task.Delay(Mathf.RoundToInt(_animation.clip.length * 1000));

			OnOpenedCompleted();
		}

		/// <summary>
		/// Called in the end of this object MonoBehaviour's OnValidate() -> <see cref="OnValidate"/>.
		/// Override this method to have your custom extra validation.
		/// </summary>
		/// <remarks>
		/// This is Editor only call.
		/// </remarks>
		protected virtual void OnEditorValidate() { }

		/// <summary>
		/// Called in the end of this object's <see cref="OnOpened"/>.
		/// Override this method to have your custom extra execution when the presenter is opened.
		/// </summary>
		protected virtual void OnOpenedCompleted() { }

		/// <summary>
		/// Called in the end of this object's <see cref="OnClosed"/>.
		/// Override this method to have your custom extra execution when the presenter is closed.
		/// </summary>
		protected virtual void OnClosedCompleted() { }
	}

	/// <inheritdoc />
	/// <remarks>
	/// Allows this Presenter to have an intro and outro animation when opened and closed to provide feedback and joy for players.
	/// </remarks>
	[RequireComponent(typeof(Animation), typeof(CanvasGroup))]
	public abstract class AnimatedUiPresenterData<T> : UiCloseActivePresenterData<T> where T : struct
	{
		[SerializeField] protected Animation _animation;
		[SerializeField] protected AnimationClip _introAnimationClip;
		[SerializeField] protected AnimationClip _outroAnimationClip;

		private void OnValidate()
		{
			Debug.Assert(_animation != null, $"Presenter {gameObject.name} does not have a referenced Animation");
			OnEditorValidate();
		}

		protected override async void OnOpened()
		{
			_animation.clip = _introAnimationClip;
			_animation.Play();

			await Task.Delay(Mathf.RoundToInt(_animation.clip.length * 1000));

			OnOpenedCompleted();
		}

		protected override async void OnClosed()
		{
			_animation.clip = _outroAnimationClip;
			_animation.Play();

			await Task.Delay(Mathf.RoundToInt(_animation.clip.length * 1000));

			gameObject.SetActive(false);
			OnClosedCompleted();
		}

		
		/// <inheritdoc cref="AnimatedUiPresenter.OnEditorValidate"/>
		protected virtual void OnEditorValidate() { }

		/// <inheritdoc cref="AnimatedUiPresenter.OnOpenedCompleted"/>
		protected virtual void OnOpenedCompleted() { }

		/// <inheritdoc cref="AnimatedUiPresenter.OnClosedCompleted"/>
		protected virtual void OnClosedCompleted() { }
	}
}
	
