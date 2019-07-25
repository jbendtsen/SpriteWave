using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public class PaletteTab : ITab
	{
		private string _name;
		private string _id;
		private Button _tabButton;
		private Panel _panel;
		private TileWindow _wnd;

		private PaletteBox _primary;

		public string Name { get { return _name; } }
		public string ID { get { return _id; } }
		public Button TabButton { get { return _tabButton; } }
		public Panel Panel { get { return _panel; } }

		public TileWindow Window
		{
			get { return _wnd; }
			set {
				_wnd = value;
				_primary.Fill = Utils.FromRGB((uint)_wnd.GetHashCode());
			}
		}

		public Size Minimum { get { return new Size(100, 80); } }

		public int X { set { _panel.Location = new Point(value, _panel.Location.Y); } }

		public PaletteTab(TileWindow wnd)
		{
			_wnd = wnd;
			_id = "paletteTab";
			_name = "Palette";

			_tabButton = new ToolBoxButton(_name);
			_tabButton.Tag = this;

			_panel = new Panel();
			_panel.Name = "palettePanel";

			Point org = new Point(20, 10);
			//Size s = new Size(this.ClientSize.Width - org.X * 2, this.ClientSize.Height - org.Y * 2);

			_primary = new PaletteBox(org, new Size(80, 20));
			_primary.Name = "primaryBox";
			_primary.Fill = Utils.FromRGB((uint)Math.Abs(_wnd.GetHashCode()));

			_panel.Controls.Add(_primary);
		}

		public void AdjustContents(Size size)
		{
			_panel.Size = size;

			if (size.Height <= _primary.Location.Y * 2)
				return;

			Size s = size;
			s.Width -= _primary.Location.X * 2;
			s.Height -= _primary.Location.Y * 2;
			_primary.Size = s;

			_primary.AdjustContents();
			_primary.Draw();
		}
	}
}
