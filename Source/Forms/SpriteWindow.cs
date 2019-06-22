using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using System.Diagnostics;

namespace SpriteWave
{
	public class SpriteWindow : TileWindow
	{
		private FileFormat _fmt;
		public FileFormat FormatToLoad { set { _fmt = value; } }

		private Edge[] _edges;
		private readonly Color outlineColour = Utils.FromRGB(0x303030);
		private const float outlineFactor = 4f;

		private const float scrollFactor = 5f;
		private const float zoomFactor = 5f;

		// 'zoom' = number of screen pixels that fit into the width of a scaled collage pixel
		private float _zoom;

		// 'xOff' and 'yOff' are measures in units of collage pixels
		private float _xOff, _yOff;
		
		public override SizeF TileDimensions
		{
			get {
				return new SizeF(
					_zoom * (float)_cl.TileW,
					_zoom * (float)_cl.TileH
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

		private HScrollBar _scrollX;
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

		private ContextMenuStrip _initialMenu;
		public ContextMenuStrip InitialMenu
		{
			set {
				_initialMenu = value;
				_initialMenu.Items[0].Enabled = true;
			}
		}

		public override ContextMenuStrip Menu
		{
			set {
				_menu = value;
				_menu.Items.Add("Erase Tile", null, (s, e) => Delete());
				_menu.Items.Add(new ToolStripSeparator());

				_menu.Items.Add(
					new ToolStripMenuItem(
						"Rotate Tile", null, new ToolStripMenuItem[] {
							new ToolStripMenuItem("Left", null, (s, e) => FlipTile(Translation.Left)),
							new ToolStripMenuItem("Right", null, (s, e) => FlipTile(Translation.Right))
						}
					)
				);
				_menu.Items.Add(
					new ToolStripMenuItem(
						"Mirror Tile", null, new ToolStripMenuItem[] {
							new ToolStripMenuItem("Horizontally", null, (s, e) => FlipTile(Translation.Horizontal)),
							new ToolStripMenuItem("Vertically", null, (s, e) => FlipTile(Translation.Vertical))
						}
					)
				);
				_menu.Items.Add(new ToolStripSeparator());

				_menu.Items.Add(
					new ToolStripMenuItem(
						"Insert", null, new ToolStripMenuItem[] {
							new ToolStripMenuItem("Column Left", null, (s, e) => InsertColumn(_selPos.col)),
							new ToolStripMenuItem("Column Right", null, (s, e) => InsertColumn(_selPos.col+1)),
							new ToolStripMenuItem("Row Above", null, (s, e) => InsertRow(_selPos.row)),
							new ToolStripMenuItem("Row Below", null, (s, e) => InsertRow(_selPos.row+1))
						}
					)
				);

				_menu.Items.Add(
					new ToolStripMenuItem(
						"Delete", null, new ToolStripMenuItem[] {
							new ToolStripMenuItem("Column", null, (s, e) => DeleteColumn(_selPos.col)),
							new ToolStripMenuItem("Row", null, (s, e) => DeleteRow(_selPos.row))
						}
					)
				);

				_menu.Items.Add(new ToolStripSeparator());
				_menu.Items.Add("Edit Palette", null, (s, e) => MessageBox.Show("nope"));
			}
		}

		public override PictureBox Canvas
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
				
				_palette1.TextChanged += (s, e) => SetPaletteIndex(0, _palette1.Text);
				_palette2.TextChanged += (s, e) => SetPaletteIndex(1, _palette2.Text);
				_palette3.TextChanged += (s, e) => SetPaletteIndex(2, _palette3.Text);
				_palette4.TextChanged += (s, e) => SetPaletteIndex(3, _palette4.Text);

				_rotateLeft = Utils.FindControl(_infoPanel, "rotateLeft") as Button;
				_rotateRight = Utils.FindControl(_infoPanel, "rotateRight") as Button;
				_mirrorHori = Utils.FindControl(_infoPanel, "mirrorHori") as Button;
				_mirrorVert = Utils.FindControl(_infoPanel, "mirrorVert") as Button;

				_rotateLeft.Click += (s, e) => FlipTile(Translation.Left);
				_rotateRight.Click += (s, e) => FlipTile(Translation.Right);
				_mirrorHori.Click += (s, e) => FlipTile(Translation.Horizontal);
				_mirrorVert.Click += (s, e) => FlipTile(Translation.Vertical);

				_nameBox = Utils.FindControl(_infoPanel, "spriteName") as TextBox;
				_nameBox.KeyDown += new KeyEventHandler(this.checkNameSubmit);

				_saveButton = Utils.FindControl(_infoPanel, "spriteSave") as Button;
				_saveButton.Click += new EventHandler(this.saveButtonHandler);
			}
		}

		public SpriteWindow() : base()
		{
			// Initialise all edges, including the invalid (centre) one
			_edges = new Edge[9];
			for (int i = 0; i < 9; i++)
				_edges[i] = new Edge((EdgeKind)i);
		}

		public override void Activate()
		{
			_yOff = -_cl.TileH * 2f;
			_zoom = _window.Size.Height / (float)(5f * _cl.TileH);
			_xOff = (-(_window.Size.Width / _zoom) + _cl.TileW) / 2f;

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

		public override EdgeKind EdgeAt(Position loc)
		{
			int x = 0, y = 0;
			if (loc.row == -1)
				y = -1;
			if (loc.row == _cl.Rows)
				y = 1;
			if (loc.col == -1)
				x = -1;
			if (loc.col == _cl.Columns)
				x = 1;

			return Edge.Direction(x, y);
		}

		public override IPiece PieceAt(Position loc)
		{
			if (_cl == null ||
			    loc.col < -1 || loc.row < -1 ||
			    loc.col > _cl.Columns || loc.row > _cl.Rows)
			{
				return null;
			}

			EdgeKind kind = EdgeAt(loc);
			if (kind == EdgeKind.None)
				return _cl.TileAt(loc);

			return _edges[(int)kind];
		}

		private void ResizeCollage(Edge msg)
		{
			if (msg.EdgeKind == EdgeKind.None)
				return;

			int x, y;
			Edge.GetCoords(msg.EdgeKind, out x, out y);
			var dist = msg.Distance;

			Func<int, int, int, Action<int>, Action<int>, int> resizeAxis = (side, delta, max, insert, delete) =>
			{
				int length = Math.Abs(delta);
				for (int i = 0; i < length; i++)
				{
					if (side < 0)
					{
						if (delta < 0)
							insert(0);
						else
							delete(0);
					}
					if (side > 0)
					{
						if (delta < 0)
							delete(max - 1);
						else
							insert(max);
					}
				}

				int shift = length;
				if ((x < 0 && delta >= 0) ||
				    (x > 0 && delta < 0))
				{
					shift *= -1;
				}

				return shift;
			};

			int shiftX = resizeAxis(x, dist.col, _cl.Columns, _cl.InsertColumn, _cl.DeleteColumn);
			int shiftY = resizeAxis(y, dist.row, _cl.Rows, _cl.InsertRow, _cl.DeleteRow);

			ShiftCamera(shiftX, shiftY);
			msg.Distance = new Position(0, 0);
		}

		// Implements ISelection.Receive()
		public override void Receive(IPiece obj)
		{
			if (obj is Edge)
			{
				ResizeCollage(obj as Edge);
				Render();
				return;
			}

			if (_cl == null)
			{
				_cl = new Collage(_fmt, 1, false);
				_cl.AddBlankTiles(1);
				Activate();
			}

			Position loc = _currentSel.Location;
			int shiftX = 0;
			if (loc.col == _cl.Columns || loc.col == -1)
			{
				if (loc.col == -1)
					loc.col = 0;

				InsertColumn(loc.col);
				shiftX = 1;
			}

			// definitely some
			// fishy
			// business going on here
			int shiftY = 0;
			if (loc.row == _cl.Rows || loc.row == -1)
			{
				if (loc.row == -1)
					loc.row = 0;

				InsertRow(loc.row);
				shiftY = 1;
			}

			_cl.SetTile(loc, obj as Tile);
			Render();
			ShiftCamera(shiftX, shiftY);
		}

		// Implements ISelection.Delete()
		public override void Delete()
		{
			if (_cl == null || _currentSel == null)
				return;

			_cl.SetTile(_currentSel.Location, _fmt.NewTile());
			Render();
			Draw();
		}

		public override void ShowMenu(int x, int y)
		{
			if (_cl == null && Transfer.HasPiece)
				_initialMenu.Show(_window, new Point(x, y));
			else
				base.ShowMenu(x, y);
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

		private void ShiftCamera(int cols, int rows)
		{
			float x = _xOff + (float)(cols * _cl.TileW) / 2f;
			float y = _yOff + (float)(rows * _cl.TileH) / 2f;
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

		public override Position GetPosition(int x, int y, bool allowOob = false)
		{
			if (_cl == null)
				return new Position(0, 0);

			float xPos = _xOff + ((float)x / _zoom);
			float yPos = _yOff + ((float)y / _zoom);

			int col = (int)(xPos / (float)_cl.TileW);
			int row = (int)(yPos / (float)_cl.TileH);

			col -= xPos < 0 ? 1 : 0;
			row -= yPos < 0 ? 1 : 0;

			if (!allowOob &&
			    (col < -1 || col > _cl.Columns ||
			     row < -1 || row > _cl.Rows)
			) {
				throw new ArgumentOutOfRangeException();
			}

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
				tileSc.Width,
				tileSc.Height
			);
		}

		public override void AdjustWindow(int width = 0, int height = 0) {}

		public override PointF[] ShapeEdge(Edge edge)
		{
			if (edge == null || edge.EdgeKind == EdgeKind.None)
				return null;

			edge.Render(_cl);

			var tri = new PointF[3];
			for (int i = 0; i < 3; i++)
			{
				tri[i].X = (edge.Shape[i].X - _xOff) * _zoom;
				tri[i].Y = (edge.Shape[i].Y - _yOff) * _zoom;
			}

			return tri;
		}

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

		public override void DrawEdges(Graphics g)
		{
			g.SmoothingMode = SmoothingMode.AntiAlias;
			Pen outline = new Pen(outlineColour, _zoom / outlineFactor);

			foreach (Edge e in _edges)
			{
				PointF[] tri = this.ShapeEdge(e);
				if (tri != null)
					g.DrawPolygon(outline, tri);
			}

			g.SmoothingMode = SmoothingMode.None;
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
		
		private void windowScrollAction(object sender, MouseEventArgs e)
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

		private void xScrollAction(object sender, ScrollEventArgs e)
		{
			ScrollTo((float)e.NewValue / _zoom, _yOff);
			Draw();
		}
		private void yScrollAction(object sender, ScrollEventArgs e)
		{
			ScrollTo(_xOff, (float)e.NewValue / _zoom);
			Draw();
		}

		private void InsertColumn(int pos)
		{
			_cl.InsertColumn(pos);
			Render();
			ShiftCamera(1, 0);
			Draw();
		}
		private void InsertRow(int pos)
		{
			_cl.InsertRow(pos);
			Render();
			ShiftCamera(0, 1);
			Draw();
		}
		private void DeleteColumn(int pos)
		{
			_cl.DeleteColumn(pos);
			Render();
			ShiftCamera(-1, 0);
			Draw();
		}
		private void DeleteRow(int pos)
		{
			_cl.DeleteRow(pos);
			Render();
			ShiftCamera(0, -1);
			Draw();
		}

		private void FlipTile(Translation tr)
		{
			if (_cl == null || _currentSel != this)
				return;

			Tile t = _cl.TileAt(_selPos);
			if (t != null)
			{
				t.Translate(tr);
				//_cl.SetTile(_selPos, t);
				Render();
				Draw();
			}
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
			Render();

			_currentSel = null;
			ResetGridPen();
			Draw();
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
