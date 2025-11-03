using UnityEngine;
using UnityEngine.UI;
using GameLovers.UiService;

namespace GameLovers.UiService.Examples
{
	/// <summary>
	/// Example UI Presenter with animation-based delays using the self-contained AnimationDelayFeature.
	/// Demonstrates how animations automatically control timing and respond to animation completion.
	/// </summary>
	[RequireComponent(typeof(AnimationDelayFeature))]
	public class AnimatedUiExamplePresenter : UiPresenter
	{
		[SerializeField] private AnimationDelayFeature _animationFeature;
		[SerializeField] private Text _titleText;
		[SerializeField] private Text _statusText;
		[SerializeField] private Button _closeButton;

		protected override void OnInitialized()
		{
			base.OnInitialized();
			Debug.Log("[AnimatedUiExample] UI Initialized");
			
			if (_closeButton != null)
			{
				_closeButton.onClick.AddListener(() => Close(destroy: false));
			}
			
			// Subscribe to animation completion events
			if (_animationFeature != null)
			{
				_animationFeature.OnOpenCompletedEvent += OnOpenAnimationCompleted;
				_animationFeature.OnCloseCompletedEvent += OnCloseAnimationCompleted;
			}
		}

		protected override void OnOpened()
		{
			base.OnOpened();
			Debug.Log("[AnimatedUiExample] UI Opened, playing intro animation...");
			
			if (_titleText != null)
			{
				_titleText.text = "Animated UI Example";
			}
			
			if (_statusText != null && _animationFeature != null)
			{
				var duration = _animationFeature.OpenDelayInSeconds;
				_statusText.text = $"Playing intro animation ({duration:F2}s)...";
			}
		}

		protected override void OnClosed()
		{
			base.OnClosed();
			Debug.Log("[AnimatedUiExample] UI Closed, outro animation playing...");
		}

		private void OnOpenAnimationCompleted()
		{
			Debug.Log("[AnimatedUiExample] Intro animation completed!");
			
			if (_statusText != null)
			{
				_statusText.text = "Animation complete - Ready!";
			}
		}

		private void OnCloseAnimationCompleted()
		{
			Debug.Log("[AnimatedUiExample] Outro animation completed!");
		}

		private void OnDestroy()
		{
			// Clean up event subscriptions
			if (_animationFeature != null)
			{
				_animationFeature.OnOpenCompletedEvent -= OnOpenAnimationCompleted;
				_animationFeature.OnCloseCompletedEvent -= OnCloseAnimationCompleted;
			}
		}
	}
}
