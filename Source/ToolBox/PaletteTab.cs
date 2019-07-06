using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public class PaletteTab : TabPage, ITab
	{
		private TileWindow _wnd;
		public TileWindow Window { get { return _wnd; } set { _wnd = value; } }

		public int MinimumWidth { get { return 100; } }
		public int MinimumHeight { get { return 50; } }

		public PaletteTab()
		{
		}
	}
}
