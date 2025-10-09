using UnityEngine;

// ReSharper disable CheckNamespace

namespace GameLovers.UiService
{
	/// <inheritdoc />
	/// <remarks>
	/// Allows this Presenter to have an intro and outro animation when opened and closed to provide feedback and joy for players.
	/// </remarks>
	[RequireComponent(typeof(Animation))]
	public class AnimationDelayer : PresenterDelayerBase
	{
	[SerializeField] protected Animation _animation;
	[SerializeField] protected AnimationClip _introAnimationClip;
	[SerializeField] protected AnimationClip _outroAnimationClip;

	/// <inheritdoc />
	public override float OpenDelayInSeconds => _introAnimationClip?.length ?? 0f;

	/// <inheritdoc />
	public override float CloseDelayInSeconds => _outroAnimationClip?.length ?? 0f;

		private void OnValidate()
		{ 
			_animation = _animation != null ? _animation : GetComponent<Animation>();
		}

		protected override void OnOpenStarted()
		{
			_animation.clip = _introAnimationClip;
			_animation.Play();
		}

		protected override void OnCloseStarted()
		{
			_animation.clip = _outroAnimationClip;
			_animation.Play();
		}
	}
}
	
