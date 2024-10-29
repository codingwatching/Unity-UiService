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
		[SerializeField] protected float _openDelayInSeconds;
		[SerializeField] protected float _closeDelayInSeconds;

		/// <inheritdoc />
		public override float OpenDelayInSeconds => _openDelayInSeconds;

		/// <inheritdoc />
		public override float CloseDelayInSeconds => _openDelayInSeconds;
	}
}
