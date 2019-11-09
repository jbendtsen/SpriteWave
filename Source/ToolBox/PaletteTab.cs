using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public class PaletteTab : ITab, IPalettePicker
	{
		private const int DividerH = 9;
		private const int PrimaryH = 80;

		private string _name;
		private string _id;
		private Button _tabButton;
		private Panel _panel;
		private TileWindow _wnd;
		private MainForm _main;

		private PalettePanel _primary;
		private PalettePanel _second;
		private int _pmIdx;

		public string Name { get { return _name; } }
		public string ID { get { return _id; } }
		public Button TabButton { get { return _tabButton; } }
		public Panel Panel { get { return _panel; } }

		public Size Minimum
		{
			get {
				return new Size(150, _second != null ? 250 : PrimaryH);
			}
		}

		public TileWindow Window
		{
			get { return _wnd; }
			set {
				if (value is SpriteWindow)
				{
					_wnd = value;
					_primary.Palette = _wnd.Collage;
				}
			}
		}

		public int X { set { _panel.Location = new Point(value, _panel.Location.Y); } }

		public PaletteTab(MainForm main, SpriteWindow wnd)
		{
			_main = main;
			_wnd = wnd;
			_id = "paletteTab";
			_name = "Palette";

			_tabButton = new ToolBoxButton(_name);
			_tabButton.Tag = this;

			_panel = new Panel();
			_panel.Name = "palettePanel";

			_primary = new PalettePanel(this, _wnd.Collage);
			_primary.Name = "primaryBox";

			_panel.Controls.Add(_primary);
		}

		public void SelectFromTable(PalettePanel panel, int cellIdx)
		{
			if (panel == _primary)
			{
				panel.CurrentCell = cellIdx;
				panel.Draw();
			}

			var table = _wnd.Collage.Format.ColorTable;
			if (!(table is ColorList))
			{
				_main.OpenColorPicker(panel.Palette, cellIdx);
				return;
			}

			if (panel == _second && _second != null)
			{
				var cl = _wnd.Collage;
				cl.NativeColors[_pmIdx] = (uint)cellIdx;
				cl.UpdateGridPen();
				cl.Render();
				_main.PerformLayout();
				return;
			}

			_pmIdx = cellIdx;
			if (_second == null)
			{
				_second = new PalettePanel(this, table as ColorList, maxVisRows: 4);
				_second.Name = "secondaryBox";
				_panel.Controls.Add(_second);
				_main.PerformLayout();
			}
		}

		public bool HandleEscapeKey(MainForm main)
		{
			_primary.CurrentCell = -1;

			bool shrink = _second != null;
			if (shrink)
			{
				_panel.Controls.Remove(_second);
				_second = null;
				main.PerformLayout();
			}
			else
				_primary.Draw();

			return shrink;
		}

		public void AdjustContents(Size size, ToolBoxOrientation layout)
		{
			if (size.Height < Minimum.Height)
				return;

			_panel.Size = size;

			_primary.Location = new Point(0, size.Height - PrimaryH);
			_primary.Size = new Size(size.Width, PrimaryH);

			if (_second != null)
			{
				_second.Size = new Size(size.Width, size.Height - (PrimaryH + DividerH));
				_second.AdjustContents(layout);
			}

			_primary.AdjustContents(layout);

			_primary.Draw();
			if (_second != null)
				_second.Draw();
		}

		public void Destruct() {}
	}
}
