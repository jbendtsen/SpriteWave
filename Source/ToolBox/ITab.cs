using System;
using System.Windows.Forms;

namespace SpriteWave
{
	public interface ITab
	{
		TileWindow Window { get; set; }
		int MinimumWidth { get; }
		int MinimumHeight { get; }
	}
}
