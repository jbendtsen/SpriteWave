using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public class InputWindow : TileWindow
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

		public override HScrollBar ScrollX
		{
			set
			{
				return;
			}
		}

		public override VScrollBar ScrollY
		{
			set {
				_scrollY = value;
				_scrollY.Scroll += new ScrollEventHandler(this.barScrollAction);
			}
		}

		public override ContextMenuStrip Menu
		{
			set {
				_menu = value;
				//_menu.Items.Add(new ToolStripSeparator());
				//_menu.Items.Add("Edit Palette", null, null);
			}
		}

		public override PictureBox Canvas
		{
			set {
				_window = value;
				DeleteFrame();
				//_window.Paint += new PaintEventHandler(this.paintBox);
				_window.Resize += new EventHandler(this.adjustWindowSize);
				_window.MouseWheel += new MouseEventHandler(this.windowScrollAction);
			}
		}

		private Panel _infoPanel;
		private Label _offsetLabel;
		private TextBox _offsetBox;
		private Label _sizeLabel;
		private Button _sendTile;
		private PictureBox _tileSample;

		private Bitmap _tileSampleBmp;

		public Panel Panel
		{
			set {
				_infoPanel = value;
				//_infoPanel.Layout += new LayoutEventHandler(this.PanelLayout);

				//_splitInput = _infoPanel.Parent as SplitContainer;

				_offsetLabel = Utils.FindControl(_infoPanel, "inputOffsetLabel") as Label;

				_offsetBox = Utils.FindControl(_infoPanel, "inputOffset") as TextBox;
				_offsetBox.TextChanged += new EventHandler(this.editOffsetBox);

				_sizeLabel = Utils.FindControl(_infoPanel, "inputSizeLabel") as Label;

				_sendTile = Utils.FindControl(_infoPanel, "inputSend") as Button;

				_tileSample = Utils.FindControl(_infoPanel, "inputSample") as PictureBox;
				_tileSample.Paint += new PaintEventHandler(this.paintSample);
			}
		}
		
		public InputWindow() : base()
		{
			_row = 0;
			_vis = new Position(0, 0);
		}

		public override void Activate()
		{
			_vis.col = _cl.Columns;
			AdjustWindow();

			_infoPanel.Visible = true;

			_offsetLabel.Visible = true;

			_offsetBox.Visible = true;
			_offsetBox.Enabled = true;
			_offsetBox.Text = "0";

			_sizeLabel.Visible = true;

			base.Activate();
		}
		
		public override void Close()
		{
			base.Close();
			_infoPanel.Visible = false;
		}

		public void Load(FileFormat fmt, byte[] file, int offset = 0)
		{
			_contents = file;
			_cl = new Collage(fmt);
			_cl.LoadTiles(_contents, offset);
			Render();

			_sizeLabel.Text = "/ 0x" + file.Length.ToString("X");

			Activate();
			ResetScroll();
		}

		public void Load(int offset)
		{
			if (_cl != null && _contents != null)
			{
				_cl.LoadTiles(_contents, offset);
				Render();
			}

			ResetScroll();
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

		public override void ResetSample()
		{
			Tile t = null;
			if (_isSel)
				t = PieceAt(_selPos) as Tile;

			_sendTile.Visible = t != null;
			_tileSample.Visible = t != null;
			_tileSampleBmp = TileBitmap(t);
		}

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
			int wndW = width > 0 ? width : _infoPanel.Size.Width;
			int wndH = height > 0 ? height : _infoPanel.Location.Y;
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

		private void editOffsetBox(object sender, EventArgs e)
		{
			try {
				string text = _offsetBox.Text;
				int offset = 0;
				if (text.Length > 0)
					offset = Convert.ToInt32(text, 16);

				if (offset >= 0 && offset < _contents.Length)
				{
					Selected = false;
					_selPos = new Position(0, 0);

					Load(offset);
					Draw();
				}
			}
			catch (Exception ex)
			{
				if (ex is ArgumentOutOfRangeException ||
				    ex is FormatException ||
				    ex is OverflowException
				)
					return;

				throw;
			}
		}

		private void paintSample(object sender, PaintEventArgs e)
		{
			if (_tileSampleBmp == null)
				return;

			e.Graphics.ToggleSmoothing(false);
			e.Graphics.DrawImage(_tileSampleBmp, 0, 0, _tileSample.Width - 1, _tileSample.Height - 1);
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
