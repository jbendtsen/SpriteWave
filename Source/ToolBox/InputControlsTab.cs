using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public class InputControlsTab : TabPage, ITab
	{
		private Label _offsetLabel;
		private TextBox _offsetBox;
		private Label _sizeLabel;
		private Button _sendTile;
		private PictureBox _tileSample;

		private Bitmap _sampleBmp;

		private MainForm.GrowWindowDelegate _growForm;

		private readonly InputWindow _wnd;
		public TileWindow Window { get { return _wnd as TileWindow; } set {} }

		public int MinimumWidth
		{
			get {
				return 200;
			}
		}
		public int MinimumHeight
		{
			get {
				int h = 70;
				if (this.Visible && _sendTile.Visible &&
				    _sendTile.Location.X <= _sizeLabel.Location.X + _sizeLabel.Size.Width)
				{
					h += 50;
				}

				return h;
			}
		}

		public Bitmap Sample
		{
			set {
				System.Diagnostics.Debug.WriteLine("ICT.Sample { set; }");

				_sampleBmp = value;
				bool state = _sampleBmp != null;

				int h = this.MinimumHeight;
				_sendTile.Visible = state;
				_tileSample.Visible = state;

				int newH = this.MinimumHeight;
				_growForm(0, newH - h);
			}
		}

		public EventHandler SendTileAction { set { _sendTile.Click += value; } }

		public int SizeText { set { _sizeLabel.Text = "/ 0x" + value.ToString("X"); } }

		public InputControlsTab(InputWindow wnd, MainForm.GrowWindowDelegate growForm)
		{
			_wnd = wnd;
			_growForm = growForm;
			this.SetupTab("Controls");

			_offsetLabel = new Label();
			_offsetLabel.Location = new System.Drawing.Point(5, 18);
			_offsetLabel.Name = "inputOffsetLabel";
			_offsetLabel.Size = new System.Drawing.Size(53, 15);
			_offsetLabel.Text = "Offset: 0x";

			_offsetBox = new TextBox();
			_offsetBox.Location = new System.Drawing.Point(60, 15);
			_offsetBox.Name = "inputOffset";
			_offsetBox.Size = new System.Drawing.Size(60, 20);
			_offsetBox.Text = "0";
			_offsetBox.TextChanged += new EventHandler(this.editOffsetBox);

			_sizeLabel = new Label();
			_sizeLabel.Location = new System.Drawing.Point(122, 18);
			_sizeLabel.Name = "inputSizeLabel";
			_sizeLabel.AutoSize = true;
			_sizeLabel.Text = "/";

			_sendTile = new Button();
			_sendTile.Location = new System.Drawing.Point(200, 12);
			_sendTile.Name = "inputSend";
			_sendTile.Size = new System.Drawing.Size(90, 24);
			_sendTile.Text = "Send To Sprite";
			_sendTile.UseVisualStyleBackColor = true;

			_tileSample = new PictureBox();
			_tileSample.BackColor = System.Drawing.SystemColors.ControlLight;
			_tileSample.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			_tileSample.Location = new System.Drawing.Point(300, 4);
			_tileSample.Name = "inputSample";
			_tileSample.Size = new System.Drawing.Size(40, 40);
			_tileSample.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			_tileSample.Paint += new PaintEventHandler(this.paintSample);

			this.Controls.Add(_offsetLabel);
			this.Controls.Add(_offsetBox);
			this.Controls.Add(_sizeLabel);
			this.Controls.Add(_sendTile);
			this.Controls.Add(_tileSample);
		}

		public void AdjustContents()
		{
			Func<int, int, int, int> centre = (off, cont, obj) => off + (cont - obj) / 2;

			Action<Control, int, int> position = (ctrl, x, gap) =>
			{
				int y = this.Size.Height - gap;
				ctrl.Location = new Point(x, y);
			};

			int w = this.Size.Width;
			int h = this.MinimumHeight;

			_offsetLabel.Location = new Point(_offsetLabel.Location.X, h - 45);
			_offsetBox.Location = new Point(_offsetBox.Location.X, h - 48);
			_sizeLabel.Location = new Point(_sizeLabel.Location.X, h - 45);

			_sendTile.Location = new Point(w - 150, 19);
			_tileSample.Location = new Point(w - 50, 11);
		}

		private void editOffsetBox(object sender, EventArgs e)
		{
			try
			{
				string text = _offsetBox.Text;
				int offset = 0;
				if (text.Length > 0)
					offset = Convert.ToInt32(text, 16);

				_wnd.Load(offset);
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
			if (_sampleBmp == null)
				return;

			e.Graphics.ToggleSmoothing(false);
			e.Graphics.DrawImage(_sampleBmp, 0, 0, _tileSample.Width - 1, _tileSample.Height - 1);
		}
	}
}
