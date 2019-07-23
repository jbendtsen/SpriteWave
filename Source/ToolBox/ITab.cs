using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public interface ITab
	{
		string Name { get; }
		Panel Panel { get; }
		TileWindow Window { get; set; }
		Size Minimum { get; }
		int X { set; }

		void AdjustContents(Size size);
	}
}
