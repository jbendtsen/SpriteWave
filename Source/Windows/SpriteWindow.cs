using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SpriteWave
{
	public partial class SpriteWindow : TileWindow
	{
		private FileFormat _fmt;
		public FileFormat FormatToLoad { set { _fmt = value; } }

		private Edge[] _edges;
		private Edge _hlEdge;

		private readonly SolidBrush mouseOverBrush;
		private readonly Color outlineColor = Utils.FromRGB(0x303030);
		private const float outlineFactor = 4f;

		private const float scrollFactor = 5f;
		private const float zoomFactor = 5f;
		private const float zoomMin = 0.01f;
		private const float zoomMax = 1000f;

		private const int initialTopGap = 4;

		// 'zoom' = number of screen pixels that fit into the width of a scaled collage pixel
		private float _zoom;
		public float InitialZoom
		{
			get {
				return _window.Height / (float)((initialTopGap * 2 + _cl.Rows) * _cl.TileH);
			}
		}

		// 'xOff' and 'yOff' are measures in units of collage pixels
		private float _xOff, _yOff;

		public override bool Selected
		{
			get {
				return _isSel;
			}
			set {
				_isSel = value;
				if (_isSel)
					_hlEdge = null;
			}
		}

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
					(int)((float)_window.Width / _zoom) + 1,
					(int)((float)_window.Height / _zoom) + 1
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

		public string ExportExtension { get { return ".png"; } }

		public SpriteWindow(MainForm main)
			: base(main)
		{
			mouseOverBrush = new SolidBrush(SystemColors.ScrollBar);

			// Initialise all edges, excluding the invalid (centre) one
			_edges = new Edge[9];
			for (int i = 0; i < 9; i++)
			{
				var kind = (EdgeKind)i;
				_edges[i] = kind != EdgeKind.None ? new Edge(kind) : null;
			}
		}

		public void Export(string fullPath, int scale)
		{
			string ext = this.ExportExtension;
			if (fullPath.Substring(fullPath.Length - ext.Length) != ext)
				fullPath += ext;

			_cl.Bitmap.Scale(scale).Save(fullPath);
		}

		public override EdgeKind EdgeOf(Position loc)
		{
			if (_cl == null)
				return EdgeKind.None;

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

			EdgeKind kind = EdgeOf(loc);
			if (kind == EdgeKind.None)
				return _cl.TileAt(loc);

			return _edges[(int)kind];
		}

		public bool ClearMousedEdge()
		{
			bool changed = _hlEdge != null;
			_hlEdge = null;
			return changed;
		}

		public bool HighlightEdgeAt(int x, int y)
		{
			bool wasOob;
			Position pos = GetPosition(x, y, out wasOob);
			EdgeKind kind = wasOob ? EdgeKind.None : EdgeOf(pos);

			Edge e = _edges[(int)kind];
			bool changed = _hlEdge != e;
			_hlEdge = e;
			return changed;
		}

		public override void ResizeCollage(Edge msg)
		{
			if (msg.EdgeKind == EdgeKind.None)
				return;

			int x, y;
			Edge.GetCoords(msg.EdgeKind, out x, out y);
			var dist = msg.Distance;

			Func<int, int, int, Func<int, int>, Func<int, int>, int> resizeAxis = (side, delta, max, insert, delete) =>
			{
				int shift = 0;
				int length = Math.Abs(delta);
				for (int i = 0; i < length; i++)
				{
					if (side < 0)
					{
						if (delta < 0)
							shift += insert(0);
						else
							shift += delete(0);
					}
					if (side > 0)
					{
						if (delta < 0)
							shift += delete(--max);
						else
							shift += insert(max);
					}
				}

				return shift;
			};

			int shiftX = resizeAxis(x, dist.col, _cl.Columns, _cl.InsertColumn, _cl.DeleteColumn);
			int shiftY = resizeAxis(y, dist.row, _cl.Rows, _cl.InsertRow, _cl.DeleteRow);

			ShiftCamera(shiftX, shiftY);
			msg.Distance = new Position(0, 0);

			Render();
		}

		public override void ReceiveTile(Tile obj, Position loc)
		{
			if (_cl == null)
			{
				_cl = new Collage(_fmt, 1, false);
				_cl.AddBlankTiles(1);
				Activate();
			}

			EdgeKind kind = EdgeOf(loc);
			if (kind != EdgeKind.None)
			{
				int x, y;
				Edge.GetCoords(kind, out x, out y);

				Edge e = _edges[(int)kind];
				e.Distance = new Position(x, y);
				ResizeCollage(e);

				if (x == -1)
					loc.col = 0;
				if (y == -1)
					loc.row = 0;
			}

			_selPos = loc;
			_cl.SetTile(_selPos, obj as Tile);
			Render();
		}
		public override void ReceiveTile(Tile obj)
		{
			ReceiveTile(obj, _selPos);
		}

		public void EraseTile()
		{
			if (_cl == null || !_isSel)
				return;

			_cl.SetTile(_selPos, _fmt.NewTile());
			Render();
			Draw();
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

		public void Centre()
		{
			if (_cl == null)
				return;

			float wndW = (float)_window.Width / _zoom;
			float wndH = (float)_window.Height / _zoom;

			ScrollTo(
				(_cl.Width / 2f) - (wndW / 2f),
				(_cl.Height / 2f) - (wndH / 2f)
			);

			UpdateBars();
		}

		private void ShiftCamera(int cols, int rows)
		{
			float x = _xOff + (float)(cols * _cl.TileW) / 2f;
			float y = _yOff + (float)(rows * _cl.TileH) / 2f;
			ScrollTo(x, y);

			/*
				Whenever ShiftCamera() gets called, it is usually the case that the collage was resized.
				As such, the current selection position may now be out-of-bounds.
				We call MoveSelection() here as it will check to see whether it is still valid.
			*/
			MoveSelection(0, 0);

			bool useWidth = true;
			int delta = cols;
			if (cols == 0)
			{
				useWidth = false;
				delta = rows;
			}
			ZoomByTiles(delta / -2f, useWidth);
		}
		private void ShiftCamera(EdgeKind e)
		{
			int x, y;
			Edge.GetCoords(e, out x, out y);
			ShiftCamera(Math.Abs(x), Math.Abs(y));
		}

		private void Zoom(float factor, int x, int y)
		{
			float xPos = _xOff + ((float)x / _zoom);
			float yPos = _yOff + ((float)y / _zoom);

			float z = _zoom * factor;
			if (z < zoomMin || z > zoomMax)
				return;

			_xOff = xPos - ((float)x / z);
			_yOff = yPos - ((float)y / z);
			_zoom = z;
		}

		public void ZoomOver(int delta, int x, int y)
		{
			float n = Math.Abs(delta);
			float amount = 1f + (n / zoomFactor);
			if (delta < 0)
				amount = 1 / amount;

			Zoom(amount, x, y);
		}
		public void ZoomIn(int delta)
		{
			ZoomOver(delta, _window.Width / 2, _window.Height / 2);
		}

		public void ZoomByTiles(float delta, bool useWidth = true)
		{
			if (_cl == null)
				return;

			float length = useWidth ? _cl.Columns : _cl.Rows;
			float amount = (length + Math.Abs(delta)) / length;
			if (delta < 0)
				amount = 1 / amount;

			Zoom(amount, _window.Width / 2, _window.Height / 2);
		}

		public override void UpdateBars()
		{
			if (_cl == null || _window == null)
				return;

			int wndW = _window.Width;
			int wndH = _window.Height;

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

		public override Position GetPosition(int x, int y, out bool wasOob)
		{
			wasOob = false;

			if (_cl == null)
				return new Position(0, 0);

			float xPos = _xOff + ((float)x / _zoom);
			float yPos = _yOff + ((float)y / _zoom);

			int col = (int)(xPos / (float)_cl.TileW);
			int row = (int)(yPos / (float)_cl.TileH);

			col -= xPos < 0 ? 1 : 0;
			row -= yPos < 0 ? 1 : 0;

			wasOob = (col < -1 || col > _cl.Columns || row < -1 || row > _cl.Rows);

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
			Pen outline = new Pen(outlineColor, _zoom / outlineFactor);

			foreach (Edge e in _edges)
			{
				PointF[] tri = this.ShapeEdge(e);
				if (tri != null)
				{
					if (e == _hlEdge)
						g.FillPolygon(mouseOverBrush, tri);

					g.DrawPolygon(outline, tri);
				}
			}

			g.SmoothingMode = SmoothingMode.None;
		}

		private static float PadOffset(float value, float unit)
		{
			return -(value % unit);
		}
		public override void DrawGrid(Graphics g)
		{
			float wndW = _window.Width;
			float wndH = _window.Height;
			SizeF tileSc = TileDimensions;

			float xLn = PadOffset(_xOff * _zoom, tileSc.Width);
			float yLn = PadOffset(_yOff * _zoom, tileSc.Height);

			Pen p = _cl.GridPen;
			while (xLn < wndW)
			{
				g.DrawLine(p, xLn, 0, xLn, wndH);
				xLn += tileSc.Width;
			}
			while (yLn < wndH)
			{
				g.DrawLine(p, 0, yLn, wndW, yLn);
				yLn += tileSc.Height;
			}
		}

		protected override void windowScrollAction(object sender, MouseEventArgs e)
		{
			Keys mod = Control.ModifierKeys;
			bool ctrlKey = (mod & Keys.Control) != 0;
			bool shiftKey = (mod & Keys.Shift) != 0;

			if (ctrlKey)
			{
				ZoomOver(e.Delta / 120, e.X, e.Y);
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

		protected override void xScrollAction(object sender, ScrollEventArgs e)
		{
			ScrollTo((float)e.NewValue / _zoom, _yOff);
			Draw();
		}
		protected override void yScrollAction(object sender, ScrollEventArgs e)
		{
			ScrollTo(_xOff, (float)e.NewValue / _zoom);
			Draw();
		}

		private void InsertCollageColumn(int pos)
		{
			int shift = _cl.InsertColumn(pos);
			Render();
			ShiftCamera(shift, 0);
			Draw();
		}
		private void InsertCollageRow(int pos)
		{
			int shift = _cl.InsertRow(pos);
			Render();
			ShiftCamera(0, shift);
			Draw();
		}
		private void DeleteCollageColumn(int pos)
		{
			int shift = _cl.DeleteColumn(pos);
			Render();
			ShiftCamera(shift, 0);
			Draw();
		}
		private void DeleteCollageRow(int pos)
		{
			int shift = _cl.DeleteRow(pos);
			Render();
			ShiftCamera(0, shift);
			Draw();
		}

		public void InsertEdge(EdgeKind kind)
		{
			if (_cl == null)
				return;

			int x, y;
			Edge.GetCoords(kind, out x, out y);

			if (x == -1)
				InsertCollageColumn(0);
			else if (x == 1)
				InsertCollageColumn(_cl.Columns);

			if (y == -1)
				InsertCollageRow(0);
			else if (y == 1)
				InsertCollageRow(_cl.Rows);
		}
		public void DeleteEdge(EdgeKind kind)
		{
			if (_cl == null)
				return;

			int x, y;
			Edge.GetCoords(kind, out x, out y);

			if (x == -1)
				DeleteCollageColumn(0);
			else if (x == 1)
				DeleteCollageColumn(_cl.Columns - 1);

			if (y == -1)
				DeleteCollageRow(0);
			else if (y == 1)
				DeleteCollageRow(_cl.Rows - 1);
		}

		public void FlipTile(Translation tr)
		{
			if (_cl == null || !_isSel)
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
	}
}
