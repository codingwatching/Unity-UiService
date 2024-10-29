using UnityEngine;

// ReSharper disable CheckNamespace

namespace GameLovers.UiService
{
	/// <inheritdoc />
	/// <remarks>
	/// Allows this Presenter to have an intro and outro delay on a defined user time.
	/// </remarks>
	public class TimeDelayer : PresenterDelayerBase
	{
		[SerializeField, Range(0f, float.MaxValue)] protected float _openDelayInSeconds;
		[SerializeField, Range(0f, float.MaxValue)] protected float _closeDelayInSeconds;

		/// <inheritdoc />
		public override float OpenDelayInSeconds => _openDelayInSeconds;

		/// <inheritdoc />
		public override float CloseDelayInSeconds => _closeDelayInSeconds;
	}
}
