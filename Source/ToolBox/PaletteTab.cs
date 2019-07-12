using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public class PaletteTab : TabPage, ITab
	{
		private TileWindow _wnd;
		public TileWindow Window { get { return _wnd; } set { _wnd = value; } }

		public Size Minimum { get { return new Size(100, 50); } }

		public PaletteTab(TileWindow wnd)
		{
			_wnd = wnd;
			this.SetupTab("Palette");
		}

		public void AdjustContents()
		{
			
		}
	}
}
