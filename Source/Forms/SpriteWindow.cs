using System;
using System.Drawing;
using System.Windows.Forms;

using System.Diagnostics;

namespace SpriteWave
{
	public class SpriteWindow : TileWindow
	{
		protected FileFormat _fmt;
		public FileFormat FormatToLoad { set { _fmt = value; } }

		protected const float scrollFactor = 10f;
		protected const float zoomFactor = 5f;

		// 'zoom' = number of screen pixels that fit into the width of a scaled collage pixel
		protected float _zoom;

		// 'xOff' and 'yOff' are measures in units of collage pixels
		protected float _xOff, _yOff;
		
		public override SizeF TileDimensions
		{
			get {
				return new SizeF(
					_zoom * (float)_cl.TileW,
					_zoom * (float)_cl.TileW
				);
			}
		}

		public override Rectangle VisibleCollageBounds
		{
			get {
				var canvas = new Rectangle(
					(int)_xOff,
					(int)_yOff,
					(int)((float)_window.Size.Width / _zoom) + 1,
					(int)((float)_window.Size.Height / _zoom) + 1
				);

				if (_cl == null || _cl.Bitmap == null)
					return canvas;

				var clRect = new Rectangle(
					0,
					0,
					_cl.Bitmap.Width,
					_cl.Bitmap.Height
				);

				return Rectangle.Intersect(canvas, clRect);
			}
		}

		protected HScrollBar _scrollX;
		public override HScrollBar ScrollX
		{
			set {
				_scrollX = value;
				_scrollX.Scroll += new ScrollEventHandler(this.xScrollAction);
			}
		}

		public override VScrollBar ScrollY
		{
			set {
				_scrollY = value;
				_scrollY.Scroll += new ScrollEventHandler(this.yScrollAction);
			}
		}
		
		public override PictureBox Window
		{
			set {
				_window = value;
				_window.Resize += new EventHandler(this.adjustWindowSize);
				_window.MouseWheel += new MouseEventHandler(this.windowScrollAction);
			}
		}
		
		Label _panelPrompt;

		TextBox _palette1;
		TextBox _palette2;
		TextBox _palette3;
		TextBox _palette4;

		Button _rotateLeft;
		Button _rotateRight;
		Button _mirrorHori;
		Button _mirrorVert;

		TextBox _nameBox;
		Button _saveButton;

		public override Panel Panel
		{
			set {
				_infoPanel = value;
				_panelPrompt = Utils.FindControl(_infoPanel, "spritePrompt") as Label;

				_palette1 = Utils.FindControl(_infoPanel, "palette1") as TextBox;
				_palette2 = Utils.FindControl(_infoPanel, "palette2") as TextBox;
				_palette3 = Utils.FindControl(_infoPanel, "palette3") as TextBox;
				_palette4 = Utils.FindControl(_infoPanel, "palette4") as TextBox;
				
				_palette1.TextChanged += new EventHandler(this.palette1Handler);
				_palette2.TextChanged += new EventHandler(this.palette2Handler);
				_palette3.TextChanged += new EventHandler(this.palette3Handler);
				_palette4.TextChanged += new EventHandler(this.palette4Handler);

				_rotateLeft = Utils.FindControl(_infoPanel, "rotateLeft") as Button;
				_rotateRight = Utils.FindControl(_infoPanel, "rotateRight") as Button;
				_mirrorHori = Utils.FindControl(_infoPanel, "mirrorHori") as Button;
				_mirrorVert = Utils.FindControl(_infoPanel, "mirrorVert") as Button;

				_rotateLeft.Click += new EventHandler(this.rotateLeftHandler);
				_rotateRight.Click += new EventHandler(this.rotateRightHandler);
				_mirrorHori.Click += new EventHandler(this.mirrorHoriHandler);
				_mirrorVert.Click += new EventHandler(this.mirrorVertHandler);

				_nameBox = Utils.FindControl(_infoPanel, "spriteName") as TextBox;
				_nameBox.KeyDown += new KeyEventHandler(this.checkNameSubmit);

				_saveButton = Utils.FindControl(_infoPanel, "spriteSave") as Button;
				_saveButton.Click += new EventHandler(this.saveButtonHandler);
			}
		}

		public SpriteWindow() : base()
		{
			_zoom = 10f;
			_xOff = 0f;
			_yOff = 0f;
		}

		public override void Activate()
		{
			//_vis.col = 5;
			//_vis.row = 4;

			base.Activate();
			_scrollX.Visible = true;
			_panelPrompt.Visible = false;

			_palette1.Visible = true;
			_palette2.Visible = true;
			_palette3.Visible = true;
			_palette4.Visible = true;
	
			_rotateLeft.Visible = true;
			_rotateRight.Visible = true;
			_mirrorHori.Visible = true;
			_mirrorVert.Visible = true;

			_nameBox.Visible = true;
			_saveButton.Visible = true;

			UpdateBars();
		}

		public override void Close()
		{
			base.Close();
			_scrollX.Visible = false;
			_panelPrompt.Visible = true;

			_palette1.Visible = false;
			_palette1.Text = "";
			_palette2.Visible = false;
			_palette2.Text = "";
			_palette3.Visible = false;
			_palette3.Text = "";
			_palette4.Visible = false;
			_palette4.Text = "";
	
			_rotateLeft.Visible = false;
			_rotateRight.Visible = false;
			_mirrorHori.Visible = false;
			_mirrorVert.Visible = false;

			_nameBox.Visible = false;
			_saveButton.Visible = false;
		}

		// Implements ISelection.Receive()
		public override void Receive(ISelection isel)
		{
			Tile selTile = isel.Piece as Tile;
			if (selTile == null)
				return;

			if (_cl == null)
			{
				_cl = new Collage(_fmt, 5, false);
				_cl.AddBlankTiles(20);
				Activate();
			}
			
			Tile t = _fmt.NewTile();
			t.Copy(selTile);

			_cl.SetTile(_sel.Location, t);
			_cl.Render();
		}

		private float AdjustScroll(ScrollBar scroll, float pos)
		{
			int val = (int)(pos * _zoom);
			if (val < scroll.Minimum)
				return (float)scroll.Minimum / _zoom;
			if (val > scroll.Maximum - scroll.LargeChange)
				return (float)(scroll.Maximum - scroll.LargeChange) / _zoom;

			return pos;
		}
		public override void ScrollTo(float x, float y)
		{
			_xOff = AdjustScroll(_scrollX, x);
			_yOff = AdjustScroll(_scrollY, y);
		}
		public override void Scroll(float dx, float dy)
		{
			float x = _xOff + (dx * scrollFactor);
			float y = _yOff + (dy * scrollFactor);
			ScrollTo(x, y);
		}

		public void Zoom(int delta, int x, int y)
		{
			float xPos = _xOff + ((float)x / _zoom);
			float yPos = _yOff + ((float)y / _zoom);

			float n = Math.Abs((float)delta / 120f);
			float amount = 1f + (n / zoomFactor);

			float z;
			if (delta < 0)
				z = _zoom / amount;
			else
				z = _zoom * amount;

			_xOff = xPos - ((float)x / z);
			_yOff = yPos - ((float)y / z);
			_zoom = z;
		}

		public override void ResetScroll()
		{
			_scrollX.Reset();
			base.ResetScroll();
		}

		public override void UpdateBars()
		{
			if (_cl == null || _window == null)
				return;

			int wndW = _window.Size.Width;
			int wndH = _window.Size.Height;

			// When the MainForm (window) gets minimised, the width and height of (at least) the _spriteBox window get set to 0
			if (wndW <= 0 || wndH <= 0)
				return;

			// Value (current position), LargeChange (slider size), Minimum, Maximum
			_scrollX.Inform(
				(int)(_xOff * _zoom),
				wndW,
				-wndW,
				wndW + (int)((float)_cl.Width * _zoom)
			);

			_scrollY.Inform(
				(int)(_yOff * _zoom),
				wndH,
				-wndH,
				wndH + (int)((float)_cl.Height * _zoom)
			);
		}

		public override void EnableSelection() {}
		public override void DisableSelection() {}

		public override Position GetPosition(int x, int y)
		{
			if (_cl == null)
				return new Position(0, 0);

			float xPos = _xOff + ((float)x / _zoom);
			float yPos = _yOff + ((float)y / _zoom);

			int col = (int)(xPos / (float)_cl.TileW);
			int row = (int)(yPos / (float)_cl.TileH);

			col -= xPos < 0 ? 1 : 0;
			row -= yPos < 0 ? 1 : 0;

			if (col < 0 || col >= _cl.Columns ||
			    row < 0 || row >= _cl.Rows)
				throw new ArgumentOutOfRangeException();

			return new Position(col, row);
		}

		public override RectangleF PieceHitbox(Position p)
		{
			SizeF tileSc = TileDimensions;

			float xCl = (p.col * _cl.TileW) - _xOff;
			float yCl = (p.row * _cl.TileH) - _yOff;

			return new RectangleF(
				xCl * _zoom,
				yCl * _zoom,
				tileSc.Width + 1f,
				tileSc.Height + 1f
			);
		}

		public override void AdjustWindow(int width = 0, int height = 0) {}

		public override void DrawCanvas(Graphics g)
		{
			Rectangle clBounds = VisibleCollageBounds;
			if (clBounds.Width <= 0 || clBounds.Height <= 0)
				return;

			using (Bitmap canvas = _cl.Bitmap.Clone(clBounds, _cl.Bitmap.PixelFormat))
			{
				var area = new RectangleF(
					(float)(clBounds.X - _xOff) * _zoom,
					(float)(clBounds.Y - _yOff) * _zoom,
					(float)clBounds.Width * _zoom,
					(float)clBounds.Height * _zoom
				);

				g.DrawImage(canvas, area);
			}
		}

		private static float PadOffset(float value, float unit)
		{
			return -(value % unit);
		}

		public override void DrawGrid(Graphics g)
		{
			float wndW = _window.Size.Width;
			float wndH = _window.Size.Height;
			SizeF tileSc = TileDimensions;

			float xLn = PadOffset(_xOff * _zoom, tileSc.Width);
			float yLn = PadOffset(_yOff * _zoom, tileSc.Height);

			while (xLn < wndW)
			{
				g.DrawLine(_gridPen, xLn, 0, xLn, wndH);
				xLn += tileSc.Width;
			}
			while (yLn < wndH)
			{
				g.DrawLine(_gridPen, 0, yLn, wndW, yLn);
				yLn += tileSc.Height;
			}
		}
		
		protected void windowScrollAction(object sender, MouseEventArgs e)
		{
			Keys mod = Control.ModifierKeys;
			bool ctrlKey = (mod & Keys.Control) != 0;
			bool shiftKey = (mod & Keys.Shift) != 0;

			if (ctrlKey)
			{
				Zoom(e.Delta, e.X, e.Y);
			}
			else
			{
				float n = -e.Delta / 120;
				if (shiftKey)
					Scroll(n, 0);
				else
					Scroll(0, n);
			}

			UpdateBars();
			Draw();
		}

		protected void xScrollAction(object sender, ScrollEventArgs e)
		{
			ScrollTo((float)e.NewValue / _zoom, _yOff);
			Draw();
		}
		protected void yScrollAction(object sender, ScrollEventArgs e)
		{
			ScrollTo(_xOff, (float)e.NewValue / _zoom);
			Draw();
		}

		private void FlipTile(Translation tr)
		{
			if (_cl == null || _sel != this)
				return;

			Tile t = _cl.TileAt(_selPos);
			if (t != null)
			{
				t.Translate(tr);
				//_cl.SetTile(_selPos, t);
				_cl.Render();
			}
		}

		public void rotateLeftHandler(object sender, EventArgs e)
		{
			FlipTile(Translation.Left);
		}
		public void rotateRightHandler(object sender, EventArgs e)
		{
			FlipTile(Translation.Right);
		}
		public void mirrorHoriHandler(object sender, EventArgs e)
		{
			FlipTile(Translation.Horizontal);
		}
		public void mirrorVertHandler(object sender, EventArgs e)
		{
			FlipTile(Translation.Vertical);
		}

		private void SetPaletteIndex(int idx, string text)
		{
			if (_fmt.Name != "NES")
				return;

			uint n;
			try {
				n = Convert.ToUInt32(text, 16);
			}
			catch {
				n = 0;
			}

			_cl.SetColour(idx, n);
			_cl.Render();

			_sel = null;
			ResetGridPen();
			Draw();
		}

		public void palette1Handler(object sender, EventArgs e)
		{
			SetPaletteIndex(0, _palette1.Text);
		}
		public void palette2Handler(object sender, EventArgs e)
		{
			SetPaletteIndex(1, _palette2.Text);
		}
		public void palette3Handler(object sender, EventArgs e)
		{
			SetPaletteIndex(2, _palette3.Text);
		}
		public void palette4Handler(object sender, EventArgs e)
		{
			SetPaletteIndex(3, _palette4.Text);
		}

		public void Save()
		{
			string name = _nameBox.Text;
			if (name == "")
				name = "sprite";

			string fileName = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + name + ".png";
			_cl.Bitmap.Scale(10).Save(fileName);
		}

		public void checkNameSubmit(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				Save();
		}
		public void saveButtonHandler(object sender, EventArgs e)
		{
			Save();
		}
	}
}
