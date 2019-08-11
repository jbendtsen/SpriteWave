using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public class InputControlsTab : ITab
	{
		private string _name;
		private string _id;
		private Button _tabButton;
		private Panel _panel;
		private readonly InputWindow _wnd;

		private Label _offsetLabel;
		private TextBox _offsetBox;
		private Label _sizeLabel;
		private Button _sendTile;
		private PictureBox _tileSample;

		private Bitmap _sampleBmp;

		public string Name { get { return _name; } }
		public string ID { get { return _id; } }
		public Button TabButton { get { return _tabButton; } }
		public Panel Panel { get { return _panel; } }
		public TileWindow Window { get { return _wnd as TileWindow; } set {} }

		public Size Minimum
		{
			get {
				int h = 70;
				if (_panel.Visible && _sendTile.Location.X <= _sizeLabel.Location.X + _sizeLabel.Width)
					h += 50;

				return new Size(200, h);
			}
		}

		public int X { set { _panel.Location = new Point(value, _panel.Location.Y); } }

		public Bitmap Sample
		{
			set {
				_sampleBmp = value;
				bool state = _sampleBmp != null;

				_sendTile.Enabled = state;
				_tileSample.Enabled = state;

				//AdjustContents();
			}
		}

		public bool IsSampleVisible { get { return this.Panel.Visible && _sampleBmp != null; } }

		public EventHandler SendTileAction { set { _sendTile.Click += value; } }

		public int SizeText { set { _sizeLabel.Text = "/ 0x" + value.ToString("X"); } }

		public InputControlsTab(InputWindow wnd)
		{
			_wnd = wnd;
			_id = "inputControlsTab";
			_name = "Controls";

			_tabButton = new ToolBoxButton(_name);
			_tabButton.Tag = this;

			_panel = new Panel();
			_panel.Name = "inputControlsPanel";
			//_panel.UseVisualStyleBackColor = true;

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
			_offsetBox.TextChanged += this.editOffsetBox;

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
			_tileSample.Paint += this.paintSample;

			_panel.Controls.Add(_offsetLabel);
			_panel.Controls.Add(_offsetBox);
			_panel.Controls.Add(_sizeLabel);
			_panel.Controls.Add(_sendTile);
			_panel.Controls.Add(_tileSample);
		}

		public void AdjustContents(Size size, ToolBoxOrientation layout)
		{
			int w = size.Width;
			int h = this.Minimum.Height;

			_panel.Size = new Size(w, h);

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
