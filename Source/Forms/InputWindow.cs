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

		private Label _offsetLabel;
		private TextBox _offsetBox;
		private Label _sizeLabel;
		private Button _sendTile;
		private PictureBox _tileSample;

		private Bitmap _tileSampleBmp;

		public override TabPage ControlsTab { get { return _controlsTab; } }

		public override void InitialiseControlsTab()
		{
			base.InitialiseControlsTab();

			_offsetLabel = new Label();
			//_offsetLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			//_offsetLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			_offsetLabel.Location = new System.Drawing.Point(5, 18);
			_offsetLabel.Name = "inputOffsetLabel";
			_offsetLabel.Size = new System.Drawing.Size(53, 15);
			//_offsetLabel.TabIndex = 0;
			_offsetLabel.Text = "Offset: 0x";
			_offsetLabel.Visible = false;

			_offsetBox = new TextBox();
			//_offsetBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			_offsetBox.Enabled = false;
			_offsetBox.Location = new System.Drawing.Point(60, 15);
			_offsetBox.Name = "inputOffset";
			_offsetBox.Size = new System.Drawing.Size(60, 20);
			//_offsetBox.TabIndex = 5;
			_offsetBox.Text = "0";
			_offsetBox.Visible = false;
			_offsetBox.TextChanged += new EventHandler(this.editOffsetBox);

			_sizeLabel = new Label();
			//_sizeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			//_sizeLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			_sizeLabel.Location = new System.Drawing.Point(122, 18);
			_sizeLabel.Name = "inputSizeLabel";
			_sizeLabel.Size = new System.Drawing.Size(58, 15);
			//_sizeLabel.TabIndex = 6;
			_sizeLabel.Text = "/";
			_sizeLabel.Visible = false;

			_sendTile = new Button();
			//_sendTile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			_sendTile.Location = new System.Drawing.Point(200, 12);
			_sendTile.Name = "inputSend";
			_sendTile.Size = new System.Drawing.Size(90, 24);
			//_sendTile.TabIndex = 8;
			_sendTile.Text = "Send To Sprite";
			_sendTile.UseVisualStyleBackColor = true;
			_sendTile.Visible = false;

			_tileSample = new PictureBox();
			//this.inputSample.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			_tileSample.BackColor = System.Drawing.SystemColors.ControlLight;
			_tileSample.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			_tileSample.Location = new System.Drawing.Point(300, 4);
			_tileSample.Name = "inputSample";
			_tileSample.Size = new System.Drawing.Size(40, 40);
			_tileSample.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			//_tileSample.TabIndex = 7;
			//_tileSample.TabStop = false;
			_tileSample.Visible = false;
			_tileSample.Paint += new PaintEventHandler(this.paintSample);

			_controlsTab.Controls.Add(_offsetLabel);
			_controlsTab.Controls.Add(_offsetBox);
			_controlsTab.Controls.Add(_sizeLabel);
			_controlsTab.Controls.Add(_sendTile);
			_controlsTab.Controls.Add(_tileSample);
		}

		public EventHandler SendTileAction { set { _sendTile.Click += value; } }

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

		public InputWindow() : base()
		{
			_row = 0;
			_vis = new Position(0, 0);
		}

		public override void Activate()
		{
			_vis.col = _cl.Columns;
			AdjustWindow();

			//_infoPanel.Visible = true;

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
			//_infoPanel.Visible = false;
		}

		public void Load(FileFormat fmt, byte[] file, int offset = 0)
		{
			_selPos = new Position(0, 0);
			Selected = false;

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
			_selPos = new Position(0, 0);
			Selected = false;

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

		public override void AdjustControlsTab()
		{
			Func<int, int, int, int> centre = (off, cont, obj) => off + (cont - obj) / 2;

			int space = 50;
			int y = _controlsTab.Size.Height - space;

			_offsetLabel.Location = new Point(_offsetLabel.Location.X, centre(y, space, _offsetLabel.Size.Height));
			_offsetBox.Location = new Point(_offsetBox.Location.X, -1 + centre(y, space, _offsetBox.Size.Height));
			_sizeLabel.Location = new Point(_sizeLabel.Location.X, centre(y, space, _sizeLabel.Size.Height));

			_sendTile.Location = new Point(_controlsTab.Size.Width - 150, -1 + centre(y, space, _sendTile.Size.Height));
			_tileSample.Location = new Point(_controlsTab.Size.Width - 50, -1 + centre(y, space, _tileSample.Size.Height));
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
