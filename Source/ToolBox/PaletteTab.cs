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

			Point org = new Point(20, 10);
			Size s = new Size(this.ClientSize.Width - org.X * 2, this.ClientSize.Height - org.Y * 2);

			_primary = new PaletteBox(org, s);
			_primary.Name = "primaryBox";
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
			_primary.AdjustContents();
			_primary.Draw();
		}
	}
}
