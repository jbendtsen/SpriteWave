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

		private readonly InputWindow _wnd;
		public TileWindow Window { get { return _wnd as TileWindow; } set {} }

		public int MinimumWidth { get { return _sizeLabel.Location.X + _sizeLabel.Size.Width + 20; } }
		public int MinimumHeight { get { return _tileSample.Location.Y + _tileSample.Size.Height + 30; } }

		public EventHandler SendTileAction { set { _sendTile.Click += value; } }

		public InputControlsTab(InputWindow wnd)
			: base()
		{
			_wnd = wnd;

			this.SetupTab("Controls");
			this.Layout += this.layoutHandler;

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

		public int SizeText { set { _sizeLabel.Text = "/ 0x" + value.ToString("X"); } }

		public void ToggleSample(bool state)
		{
			_sendTile.Visible = state;
			_tileSample.Visible = state;
		}

		private void layoutHandler(object sender, LayoutEventArgs e)
		{
			Func<int, int, int, int> centre = (off, cont, obj) => off + (cont - obj) / 2;

			int space = 50;
			int zone = this.Size.Height - space;

			int raise = 0;
			if (_sendTile.Location.X <= _sizeLabel.Location.X + _sizeLabel.Size.Width)
				raise = -space;

			_offsetLabel.Location = new Point(_offsetLabel.Location.X, raise + centre(zone, space, _offsetLabel.Size.Height));
			_offsetBox.Location = new Point(_offsetBox.Location.X, -1 + raise + centre(zone, space, _offsetBox.Size.Height));
			_sizeLabel.Location = new Point(_sizeLabel.Location.X, raise + centre(zone, space, _sizeLabel.Size.Height));

			_sendTile.Location = new Point(this.Size.Width - 150, -1 + centre(zone, space, _sendTile.Size.Height));
			_tileSample.Location = new Point(this.Size.Width - 50, -1 + centre(zone, space, _tileSample.Size.Height));
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
			Bitmap sample = _wnd.SampleImage;
			if (sample == null)
				return;

			e.Graphics.ToggleSmoothing(false);
			e.Graphics.DrawImage(sample, 0, 0, _tileSample.Width - 1, _tileSample.Height - 1);
		}
	}
}
