using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public class SpriteControlsTab : TabPage, ITab
	{
		private readonly SpriteWindow _wnd;
		public TileWindow Window { get { return _wnd; } set {} }

		public int MinimumWidth { get { return 100; } }
		public int MinimumHeight { get { return 50; } }

		public SpriteControlsTab(SpriteWindow wnd)
			: base()
		{
			_wnd = wnd;
			this.SetupTab("Controls");
		}
	}
}
