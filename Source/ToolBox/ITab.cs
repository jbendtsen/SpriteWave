using System;

namespace SpriteWave
{
	public interface ITab
	{
		TileWindow Window { get; set; }
		int MinimumWidth { get; }
		int MinimumHeight { get; }

		void SetControlOrigins();
		void AdjustContents();
	}
}
