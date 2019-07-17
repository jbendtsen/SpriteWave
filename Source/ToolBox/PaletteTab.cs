using System;
using System.Drawing;
using System.Windows.Forms;

using System.Diagnostics;

namespace SpriteWave
{
	public class PaletteTab : TabPage, ITab
	{
		private PaletteBox _primary;

		private TileWindow _wnd;
		public TileWindow Window
		{
			get { return _wnd; }
			set {
				_wnd = value;
				_primary.Fill = Utils.FromRGB((uint)_wnd.GetHashCode());
			}
		}

		public Size Minimum { get { return new Size(100, 80); } }

		public PaletteTab(TileWindow wnd)
		{
			_wnd = wnd;
			this.SetupTab("Palette");

			_primary = new PaletteBox();
			_primary.Name = "primaryBox";
			_primary.Location = new Point(20, 10);
			_primary.Fill = Utils.FromRGB((uint)Math.Abs(_wnd.GetHashCode()));

			this.Controls.Add(_primary);
		}

		public void AdjustContents()
		{
			Size s = this.Size;
			if (s.Height <= _primary.Location.Y * 2)
				return;

			s.Width -= _primary.Location.X * 2;
			s.Height -= _primary.Location.Y * 2;

			_primary.Size = s;
			_primary.Image = new Bitmap(s.Width, s.Height);
			_primary.Draw();
		}
	}
}
