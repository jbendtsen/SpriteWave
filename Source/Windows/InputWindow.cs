using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public partial class InputWindow : TileWindow
	{
		private byte[] _contents;
		
		// camera offset, number of visible tiles
		int _row;
		protected Position _vis;

		public override SizeF TileDimensions
		{
			get {
				return new SizeF(
					(float)_window.Width / (float)_vis.col,
					(float)_window.Height / (float)_vis.row
				);
			}
		}
		
		public override Rectangle VisibleCollageBounds
		{
			get
			{
				return new Rectangle(
					0,
					_row * _cl.TileH,
					_vis.col * _cl.TileW,
					_vis.row * _cl.TileH
				);
			}
		}

		public Tile TileSample { set { ((InputControlsTab)_controlsTab).Sample = TileBitmap(value); } }

		public InputWindow(MainForm main)
			: base(main)
		{
			_row = 0;
			_vis = new Position(0, 0);
			DeleteFrame();
		}

		public override void Activate()
		{
			_vis.col = _cl.Columns;
			AdjustWindow();
/*
			//_infoPanel.Visible = true;

			_offsetLabel.Visible = true;

			_offsetBox.Visible = true;
			_offsetBox.Enabled = true;
			_offsetBox.Text = "0";

			_sizeLabel.Visible = true;
*/
			base.Activate();
		}

		public void Load(FileFormat fmt, byte[] file, int offset = 0)
		{
			_selPos = new Position(0, 0);
			Selected = false;
			TileSample = null;

			_contents = file;
			_cl = new Collage(fmt);
			_cl.LoadTiles(_contents, offset);
			Render();

			((InputControlsTab)_controlsTab).SizeText = file.Length;

			Activate();
			ResetScroll();
		}

		public void Load(int offset)
		{
			if (offset < 0 || offset >= _contents.Length)
				return;

			_selPos = new Position(0, 0);
			Selected = false;
			TileSample = null;

			if (_cl != null && _contents != null)
			{
				_cl.LoadTiles(_contents, offset);
				Render();
			}

			ResetScroll();
			Draw();
		}

		public override void Scroll(float dx, float dy)
		{
			if (_cl == null)
				return;

			int lastRow = _cl.Rows - _vis.row;
			_row += (int)dy;
			_row = _row.Between(0, lastRow);

			Draw();
		}
		public override void ScrollTo(float x, float y)
		{
			Scroll(0, (int)y - _row);
		}

		public override void ResetScroll()
		{
			AdjustWindow();
			_scrollY.Reset();
			ScrollTo(0, 0);
		}

		public override void UpdateBars()
		{
			if (_cl != null)
				_scrollY.Inform(_row, _vis.row, 0, _cl.Rows);
		}

		public override Position GetPosition(int x, int y, bool allowOob = false)
		{
			SizeF tileSc = TileDimensions;

			var pos = new Position(
				(int)((float)x / tileSc.Width),
				(int)((float)y / tileSc.Height)
			);

			pos.row += _row;
			return pos;
		}

		public override void MoveSelection(int dCol, int dRow)
		{
			base.MoveSelection(dCol, dRow);

			if (_selPos.row >= _row + _vis.row)
			{
				_row = _selPos.row - _vis.row + 1;
				UpdateBars();
			}
			else if (_selPos.row < _row)
			{
				_row = _selPos.row;
				UpdateBars();
			}
		}

		/*
		private int _count = 0;
		public override void ResetSample(Tile t)
		{
			System.Diagnostics.Debug.WriteLine("{0} ResetSample()", _count++);

			if (t == null && _isSel)
				t = PieceAt(_selPos) as Tile;

			((InputControlsTab)_controlsTab).ToggleSample(t != null);
			_tileSampleBmp = TileBitmap(t);
		}
		*/

		public override RectangleF PieceHitbox(Position p)
		{
			int col = p.col;
			int row = p.row - _row;
			SizeF tileSc = TileDimensions;

			return new RectangleF(
				(float)col * tileSc.Width,
				(float)row * tileSc.Height,
				(int)tileSc.Width + 1,
				(int)tileSc.Height + 1
			);
		}

		public override void AdjustWindow(int width = 0, int height = 0)
		{
			if (_cl == null || _window == null)
				return;

			// Start with our "constants"
			//int wndW = width > 0 ? width : _infoPanel.Size.Width;
			//int wndH = height > 0 ? height : _infoPanel.Location.Y;

			int wndW = width > 0 ? width : _window.Size.Width;
			int wndH = height > 0 ? height : _window.Size.Height;
			float thF = (float)_cl.TileH;

			float scaleX = (float)wndW / (float)_cl.Width;

			// The number of visible rows is the foundation from which all InputWindow sizing is based
			float visRowsF = (float)wndH / (scaleX * thF);
			visRowsF = (float)Math.Round(visRowsF);
			_vis.row = (int)visRowsF;

			int rows = _cl.Rows;
			bool scroll = true;
			if (_vis.row > rows)
			{
				scroll = false;
				_vis.row = rows;
				wndH = (int)((float)_vis.row * thF * scaleX);
			}

			_window.Width = wndW;
			if (wndH <= _window.Height)
				_window.Height = wndH;

			_scrollY.Visible = scroll;
		}

		public override void DrawGrid(Graphics g)
		{
			int wndW = _window.Width;
			int wndH = _window.Height;

			// Draw vertical margins
			float tlW = (float)wndW / (float)_vis.col;
			for (int i = 0; i < _vis.col - 1; i++)
			{
				float x = (float)(i + 1) * tlW;
				g.DrawLine(_gridPen, x, 0, x, wndH);
			}

			// Draw horizontal margins
			float tlH = (float)wndH / (float)_vis.row;
			for (int i = 0; i < _vis.row - 1; i++)
			{
				float y = (float)(i + 1) * tlH;
				g.DrawLine(_gridPen, 0, y, wndW, y);
			}
		}
		
		protected override void windowScrollAction(object sender, MouseEventArgs e)
		{
			Scroll(0, -e.Delta / 120);
			UpdateBars();
		}

		protected override void yScrollAction(object sender, ScrollEventArgs e)
		{
			ScrollTo(0, e.NewValue);
		}
	}
}
