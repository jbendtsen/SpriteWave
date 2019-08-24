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

		private PalettePanel _primary;

		public string Name { get { return _name; } }
		public string ID { get { return _id; } }
		public Button TabButton { get { return _tabButton; } }
		public Panel Panel { get { return _panel; } }

		public Size Minimum { get { return new Size(150, 80); } }

		public TileWindow Window
		{
			get { return _wnd; }
			set {
				if (value is SpriteWindow)
				{
					_wnd = value;
					_primary.Collage = _wnd.Collage;
				}
			}
		}

		public int X { set { _panel.Location = new Point(value, _panel.Location.Y); } }

		public PaletteTab(SpriteWindow wnd)
		{
			_wnd = wnd;
			_id = "paletteTab";
			_name = "Palette";

			_tabButton = new ToolBoxButton(_name);
			_tabButton.Tag = this;

			_panel = new Panel();
			_panel.Name = "palettePanel";

			//Point org = new Point(20, 10);
            Point org = new Point(0, 0);
			//Size s = new Size(this.ClientSize.Width - org.X * 2, this.ClientSize.Height - org.Y * 2);

			_primary = new PalettePanel(_wnd.Collage, org, new Size(80, 20));
			_primary.Name = "primaryBox";

			_panel.Controls.Add(_primary);
		}

		public void AdjustContents(Size size, ToolBoxOrientation layout)
		{
			_panel.Size = size;
			_primary.Size = size;

			_primary.AdjustContents(layout);
			_primary.Draw();
		}
	}
}
