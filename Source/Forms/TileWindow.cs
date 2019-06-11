using System;
using System.Drawing;
using System.Windows.Forms;

using System.Diagnostics;

namespace SpriteWave
{
	public abstract class TileWindow : ISelection
	{
		protected Collage _cl;
		public bool IsOpen { get { return _cl != null; } }

		// camera offset, number of visible tiles
		protected Position _offset, _vis;
		public Position VisibleSelection { get { return _vis; } }

		protected Brush _selHl;
		protected Position _selPos;

		public abstract HScrollBar ScrollX { set; }

		protected VScrollBar _scrollY;
		public abstract VScrollBar ScrollY { set; }

		protected PictureBox _window;
		public abstract PictureBox Window { set; }

		protected Panel _infoPanel;
		public abstract Panel Panel { set; }

		protected Rectangle _bounds;
		protected Pen _gridPen;

		// Implements ISelection.Selection
		protected ISelection _sel;
		public ISelection Selection
		{
			get {
				return _sel;
			}
			set {
				// If the new value is not this window or a mouseselection, don't accept it
				ISelection isel = value;
				if (isel != null && isel != this && !(isel is DragPoint))
					isel = null;

				bool enable = isel != null;
				_sel = isel;

				if (enable)
					EnableSelection();
				else
					DisableSelection();
			}
		}
		
		// Implements ISelection.Location
		public Position Location
		{
			get {
				return new Position(
					_selPos.col - _offset.col,
					_selPos.row - _offset.row
				);
			}
			set {
				SetPosition(value);
			}
		}
		
		public Tile SelectTileAt(Position loc, bool relative = true)
		{
			if (_cl == null)
				return null;

			Position p;
			if (relative)
				p = new Position(
					loc.col + _offset.col,
					loc.row + _offset.row
				);
			else
				p = loc;

			return _cl.TileAt(p);
		}

		// Implements ISelection.Tile
		public Tile Tile
		{
			get {
				if (_sel == null)
					return null;

				if (_sel != this)
					return _sel.Tile;

				return SelectTileAt(_selPos, false);
			}
		}

		public SizeF TilePx
		{
			get {
				return new SizeF(
					(float)_window.Width / (float)_vis.col,
					(float)_window.Height / (float)_vis.row
				);
			}
		}

		public Rectangle VisibleCollageBounds
		{
			get
			{
				return new Rectangle(
					_offset.col * _cl.TileW,
					_offset.row * _cl.TileH,
					_vis.col * _cl.TileW,
					_vis.row * _cl.TileH
				);
			}
		}

		public abstract void UpdateBars();
		//public abstract void AdjustWindow(int width = 0, int height = 0);

		// Implements ISelection.Receive()
		public abstract void Receive(ISelection isel);

		protected TileWindow()
		{
			_offset = new Position(0, 0);
			_vis = new Position(0, 0);
			_selPos = new Position(0, 0);
			_selHl = new SolidBrush(Color.FromArgb(96, 0, 64, 255));
		}

		public virtual void Activate()
		{
			_scrollY.Visible = true;
		}

		public virtual void Close()
		{
			_cl = null;
			if (_window.Image != null)
			{
				_window.Image.Dispose();
				_window.Image = null;
			}

			_scrollY.Visible = false;
		}

		public virtual void Scroll(int dCol, int dRow)
		{
			if (_cl == null)
				return;
			
			int lastCol = _cl.Columns - _vis.col;
			int lastRow = _cl.Rows - _vis.row;
			
			_offset.col += dCol;
			_offset.row += dRow;
			
			_offset.col = _offset.col.Between(0, lastCol);
			_offset.row = _offset.row.Between(0, lastRow);

			Draw();
		}
		public void ScrollTo(int col, int row)
		{
			Scroll(col - _offset.col, row - _offset.row);
		}

		public virtual void ResetScroll()
		{
			_scrollY.Reset();

			ScrollTo(0, 0);
			AdjustWindow();
		}

		public abstract void EnableSelection();
		public abstract void DisableSelection();
		/*
		{
			_mouse = null;
			_selPos.col = 0;
			_selPos.row = 0;
			_sel = null;
		}
		*/

		public void MoveSelection(int dCol, int dRow)
		{
			if (_cl == null)
				return;
			
			int nCols = _cl.Columns;
			int idx = (_selPos.row + dRow) * nCols + _selPos.col + dCol;
			if (idx < 0 || idx >= _cl.nTiles)
				return;

			_selPos.col = idx % nCols;
			_selPos.row = idx / nCols;
			
			//Debug.WriteLine("col = {0}, row = {1}", _selPos.col, _selPos.row);
		}

		public Position GetPosition(int x, int y)
		{
			SizeF tileSc = TilePx;

			return new Position(
				(int)((float)x / tileSc.Width),
				(int)((float)y / tileSc.Height)
			);
		}

		public virtual void SetPosition(Position p)
		{
			_selPos.col = p.col + _offset.col;
			_selPos.row = p.row + _offset.row;
			_sel = this;
		}

		public Bitmap TileBitmap(Tile t)
		{
			if (t == null || _cl == null)
				return null;

			return _cl.RenderTile(t);
		}
		
		public void AdjustWindow(int width = 0, int height = 0)
		{
			if (_cl == null || _window == null)
				return;

			// Start with our "constants"
			int wndW = width > 0 ? width : _infoPanel.Size.Width;
			int wndH = height > 0 ? height : _infoPanel.Location.Y;
			float thF = (float)_cl.TileH;

			float scaleX = (float)wndW / (float)_cl.Width;

			// The number of visible rows is the foundation from which all window sizing is based
			float visRowsF = (float)wndH / (scaleX * thF);
			visRowsF = (float)Math.Round(visRowsF);
			_vis.row = (int)visRowsF;

			int rows = _cl.Rows;
			_scrollY.Visible = _vis.row < rows;

			if (!_scrollY.Visible)
			{
				_vis.row = rows;
				wndH = (int)((float)_vis.row * thF * scaleX);
			}

			_window.Width = wndW;
			_window.Height = wndH;
		}

		// Implements ISelection.DrawSelection
		public void DrawSelection(TileWindow tw, Graphics g)
		{
			Position p = this.Location;

			if (p.col < 0 || p.col >= _vis.col ||
				p.row < 0 || p.row >= _vis.row)
			{
				return;
			}

			Rectangle selRect = p.TileRect(TilePx);
			g.FillRectangle(_selHl, selRect);
		}

		public void Draw(Graphics gCtx = null)
		{
			if (_cl == null || _window == null || _vis.col <= 0 || _vis.row <= 0)
				return;

			int wndW = _window.Width;
			int wndH = _window.Height;
			if (wndW <= 0 || wndH <= 0)
				return;

			// Make sure the window's collage has something for us to draw
			if (_cl.Bitmap == null)
				_cl.Render();

			// Select the subset of the window's collage to render
			Bitmap canvas = _cl.Bitmap.Clone(VisibleCollageBounds, _cl.Bitmap.PixelFormat);

			// Ensure that we have a window to draw to and the ability to draw (if that's not already the case)
			if (gCtx == null && _window.Image != null)
			{
				_window.Image.Dispose();
				_window.Image = null;
			}
			if (_window.Image == null)
				_window.Image = new Bitmap(wndW, wndH);

			Graphics g = gCtx ?? Graphics.FromImage(_window.Image);

			/*
				Here, we first disable pixel interpolation (blurring). This is because the section of the collage we just selected
				is almost certainly going to be smaller or larger than the space we want to render it to.
				The aim is to retain the blocky, "8-bit" look, while maximising the available space.
			*/
			g.ToggleSmoothing(false);

			// Draw the visible section of the collage. This method automatically resizes our provided bitmap before drawing it.
			g.DrawImage(canvas, 0, 0, wndW, wndH);

			if (_sel != null)
			{
				_sel.DrawSelection(this, g);
			}

			// In order to more easily discern between tiles on the screen, we draw margins around each tile.
			if (_gridPen == null)
			{
				uint marginClr = Utils.InvertRGB(_cl.MeanColour);
				_gridPen = new Pen(Utils.FromRGBA(marginClr), 1.0f);
			}

			// Draw vertical margins
			float tw = (float)wndW / (float)_vis.col;
			for (int i = 0; i < _vis.col - 1; i++)
			{
				float x = (float)(i + 1) * tw;
				g.DrawLine(_gridPen, x, 0, x, wndH);
			}

			// Draw horizontal margins
			float th = (float)wndH / (float)_vis.row;
			for (int i = 0; i < _vis.row - 1; i++)
			{
				float y = (float)(i + 1) * th;
				g.DrawLine(_gridPen, 0, y, wndW, y);
			}

			// Don't Dispose of the Graphics object if it was passed by a Paint event
			if (gCtx == null)
				g.Dispose();
		}

		protected void paintBox(object sender, PaintEventArgs e)
		{
			//Scroll(0, 0); // adjusts camera if necessary
			Draw(e.Graphics);
		}

		protected void adjustWindowSize(object sender, EventArgs e)
		{
			AdjustWindow(_window.Width, _window.Height);
			//Draw();
		}

		protected void windowScrollAction(object sender, MouseEventArgs e)
		{
			Scroll(0, -e.Delta / 120);
			UpdateBars();
		}

		protected void barScrollAction(object sender, ScrollEventArgs e)
		{
			ScrollTo(0, e.NewValue);
		}
	}
}
