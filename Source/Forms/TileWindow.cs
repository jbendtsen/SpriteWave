using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public abstract class TileWindow : ISelection
	{
		protected Collage _cl;

		bool active = false;
		// Implements ISelection.IsActive
		public bool IsActive { get { return _cl != null && active; } }

		//public abstract Position VisibleSelection { get; }

		public abstract SizeF TileDimensions { get; }
		public abstract Rectangle VisibleCollageBounds { get; }

		protected Brush _selHl;
		protected Position _selPos;

		public abstract HScrollBar ScrollX { set; }

		protected VScrollBar _scrollY;
		public abstract VScrollBar ScrollY { set; }

		protected PictureBox _window;
		public abstract PictureBox Canvas { set; }

		protected ContextMenuStrip _menu;
		public abstract ContextMenuStrip Menu { set; }

		protected Panel _infoPanel;
		public abstract Panel Panel { set; }

		public virtual string Prompt { set { return; } }

		protected Rectangle _bounds;
		protected Pen _gridPen;

		protected ISelection _currentSel;

		// Implements ISelection.Location
		public Position Location
		{
			get {
				return _selPos;
			}
			set {
				_selPos = value;
				_currentSel = this;
				ResetSample();
			}
		}

		// Implements ISelection.Piece
		public IPiece Piece
		{
			get {
				if (_currentSel == null)
					return null;

				if (_currentSel != this)
					return _currentSel.Piece;

				return PieceAt(_selPos);
			}
		}

		public ISelection Selection
		{
			get {
				return _currentSel;
			}
			set {
				// If the new value is not this window or a mouseselection, don't accept it
				ISelection isel = value;
				if (isel != null && isel != this && !(isel is DragPoint))
					isel = null;
		
				bool enable = isel != null && isel.Piece is Tile;
				_currentSel = isel;
		
				if (enable)
					EnableSelection();
				else
					DisableSelection();
			}
		}

		// Implements ISelection.Receive()
		public abstract void Receive(IPiece isel);

		// Implements ISelection.Delete()
		public abstract void Delete();

		public abstract void Scroll(float dx, float dy);
		public abstract void ScrollTo(float x, float y);

		public abstract void ResetScroll();
		public abstract void UpdateBars();

		public abstract void EnableSelection();
		public abstract void DisableSelection();
		
		public abstract Position GetPosition(int x, int y, bool allowOob = false);
		public abstract RectangleF PieceHitbox(Position p);

		public abstract void AdjustWindow(int width = 0, int height = 0);

		public abstract void DrawGrid(Graphics g);

		protected TileWindow()
		{
			_selPos = new Position(0, 0);
			_selHl = new SolidBrush(Color.FromArgb(96, 0, 64, 255));
		}

		public virtual void Activate()
		{
			active = true;
			_scrollY.Visible = true;
		}

		public virtual void Close()
		{
			active = false;
			_scrollY.Visible = false;
			_cl = null;
			DeleteFrame();
		}

		public virtual void Render()
		{
			_cl.Render();
		}

		public virtual IPiece PieceAt(Position loc)
		{
			if (_cl == null)
				return null;

			return _cl.TileAt(loc);
		}
		
		public void DeleteFrame()
		{
			if (_window.Image != null)
			{
				_window.Image.Dispose();
				_window.Image = null;
			}
		}

		public virtual void ShowMenu(int x, int y)
		{
			if ( _window != null && _cl != null)
				_menu.Show(_window, new Point(x, y));
		}

		public void MoveSelection(int dCol, int dRow)
		{
			if (_cl == null)
				return;
			
			int nCols = _cl.Columns;
			int idx = (_selPos.row + dRow) * nCols + _selPos.col + dCol;

			if (idx < 0)
				idx = 0;
			else if (idx >= _cl.nTiles)
				idx = _cl.nTiles - 1;

			this.Location = new Position(idx % nCols, idx / nCols);
		}

		public Bitmap TileBitmap(Tile t)
		{
			if (t == null || _cl == null)
				return null;

			return _cl.RenderTile(t);
		}
		
		public virtual void ResetSample() {}

		public virtual EdgeKind EdgeOf(Position p) { return EdgeKind.None; }
		public virtual PointF[] ShapeEdge(Edge edge) { return null; }

		public void ResetGridPen()
		{
			uint marginClr = Utils.InvertRGB(_cl.MeanColour);
			_gridPen = new Pen(Utils.FromRGB(marginClr), 1.0f);
		}

		public virtual void DrawCanvas(Graphics g)
		{
			Rectangle clBounds = VisibleCollageBounds;
			if (clBounds.Width <= 0 || clBounds.Height <= 0)
				return;

			// Select the subset of the window's collage to render
			using (Bitmap canvas = _cl.Bitmap.Clone(clBounds, _cl.Bitmap.PixelFormat))
			{
				// Draw the visible section of the collage. This method automatically resizes our provided bitmap before drawing it.
				g.DrawImage(canvas, 0, 0, _window.Width, _window.Height);
			}
		}

		// Implements ISelection.DrawSelection
		public void DrawSelection(Graphics g)
		{
			Position loc = this.Location;
			IPiece obj = PieceAt(loc);
			Utils.DrawSelection(g, this, _selHl, obj, loc);
		}

		public virtual void DrawEdges(Graphics g) {}

		public virtual void Draw()
		{
			if (_cl == null || _window == null)
				return;

			int wndW = _window.Width;
			int wndH = _window.Height;
			if (wndW <= 0 || wndH <= 0)
				return;

			// Make sure the window's collage has something for us to draw
			if (_cl.Bitmap == null)
				Render();

			DeleteFrame();
			_window.Image = new Bitmap(wndW, wndH);

			using (var g = Graphics.FromImage(_window.Image))
			{
				/*
					Here, we first disable pixel interpolation (blurring). This is because the section of the collage we just selected
					is almost certainly going to be smaller or larger than the space we want to render it to.
					The aim is to retain the blocky, "8-bit" look, while maximising the available space.
				*/
				g.ToggleSmoothing(false);

				// Draw the tiles
				DrawCanvas(g);

				// Highlight the current tile, if one is currently selected
				if (_currentSel != null)
					_currentSel.DrawSelection(g);

				// Draw some borders to indicate that the window's collage can be resized
				// Only implemented in SpriteWindow
				DrawEdges(g);

				// In order to more easily discern between tiles on the screen, we draw margins around each tile.
				if (_gridPen == null)
					ResetGridPen();

				DrawGrid(g);
			}
		}

		protected void adjustWindowSize(object sender, EventArgs e)
		{
			AdjustWindow(_window.Width, _window.Height);
			//Draw();
		}
	}
}
