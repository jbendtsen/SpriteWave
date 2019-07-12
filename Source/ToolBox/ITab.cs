using System;
using System.Drawing;

namespace SpriteWave
{
	public interface ITab
	{
		TileWindow Window { get; set; }
		Size Minimum { get; }

		void AdjustContents();
	}
}
