using System;

namespace GameLovers.UiService
{
	/// <summary>
	/// Presenters marked with this will be loaded synchronously by <see cref="UiService"/>
	/// </summary>
	public class LoadSynchronouslyAttribute : Attribute
	{
	}
}
