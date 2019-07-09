using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace SpriteWave
{
	public class SpriteControlsTab : TabPage, ITab
	{
		private readonly SpriteWindow _wnd;
		public TileWindow Window { get { return _wnd; } set {} }

		public int MinimumWidth { get { return 320; } }
		public int MinimumHeight { get { return 180; } }

		private Rectangle _divider;
		private readonly Pen _dividerPen;

		private Label _tileLabel;
		private Label _outputLabel;

		private Button _rotateLeftBtn;
		private Button _rotateRightBtn;
		private Button _mirrorHoriBtn;
		private Button _mirrorVertBtn;
		private Button _eraseBtn;

		private Label _scaleLabel;
		private NumericUpDown _scaleBox;
		private Label _folderLabel;
		private Button _folderBtn;
		private Label _nameLabel;
		private TextBox _nameBox;
		private RadioButton _overwriteBtn;
		private RadioButton _appendBtn;
		private TextBox _appendBox;
		private Button _saveButton;

		public SpriteControlsTab(SpriteWindow wnd)
		{
			_wnd = wnd;
			this.SetupTab("Controls");

			var resources = new ComponentResourceManager(typeof(SpriteControlsTab));

			_divider = new Rectangle(120, 5, 0, 160);
			_dividerPen = new Pen(Color.Silver);

			_tileLabel = new Label();
			_tileLabel.Font = new Font(Label.DefaultFont, FontStyle.Bold);
			_tileLabel.Name = "tileLabel";
			_tileLabel.Text = "Tile";
			_tileLabel.AutoSize = true;
			_tileLabel.Location = new Point(20, 8);

			_outputLabel = new Label();
			_outputLabel.Font = new Font(Label.DefaultFont, FontStyle.Bold);
			_outputLabel.Name = "outputLabel";
			_outputLabel.Text = "Output";
			_outputLabel.AutoSize = true;
			_outputLabel.Location = new Point(136, 8);

			_rotateLeftBtn = new Button();
			_rotateLeftBtn.Name = "rotateLeftBtn";
			_rotateLeftBtn.Size = new Size(32, 32);
			_rotateLeftBtn.Location = new Point(26, 30);
			_rotateLeftBtn.Image = (Image)(resources.GetObject("rotateLeftImg"));
			_rotateLeftBtn.Click += (s, e) => _wnd.FlipTile(Translation.Left);

			_rotateRightBtn = new Button();
			_rotateRightBtn.Name = "rotateRightBtn";
			_rotateRightBtn.Size = new Size(32, 32);
			_rotateRightBtn.Location = new Point(66, 30);
			_rotateRightBtn.Image = (Image)(resources.GetObject("rotateRightImg"));
			_rotateRightBtn.Click += (s, e) => _wnd.FlipTile(Translation.Right);

			_mirrorHoriBtn = new Button();
			_mirrorHoriBtn.Name = "mirrorHoriBtn";
			_mirrorHoriBtn.Size = new Size(32, 32);
			_mirrorHoriBtn.Location = new Point(26, 70);
			_mirrorHoriBtn.Image = (Image)(resources.GetObject("mirrorHoriImg"));
			_mirrorHoriBtn.Click += (s, e) => _wnd.FlipTile(Translation.Horizontal);

			_mirrorVertBtn = new Button();
			_mirrorVertBtn.Name = "mirrorVertBtn";
			_mirrorVertBtn.Size = new Size(32, 32);
			_mirrorVertBtn.Location = new Point(66, 70);
			_mirrorVertBtn.Image = (Image)(resources.GetObject("mirrorVertImg"));
			_mirrorVertBtn.Click += (s, e) => _wnd.FlipTile(Translation.Vertical);

			_eraseBtn = new Button();
			_eraseBtn.Name = "eraseBtn";
			_eraseBtn.Text = "Erase";
			_eraseBtn.AutoSize = true;
			_eraseBtn.Location = new Point(25, 110);
			_eraseBtn.Click += (s, e) => _wnd.EraseTile();

			_scaleLabel = new Label();
			_scaleLabel.Name = "scaleLabel";
			_scaleLabel.Text = "Scale:  x";
			_scaleLabel.AutoSize = true;
			_scaleLabel.Location = new Point(142, 32);

			_scaleBox = new NumericUpDown();
			_scaleBox.Name = "scaleBox";
			_scaleBox.Size = new Size(40, 20);
			_scaleBox.Location = new Point(190, 30);
			_scaleBox.Increment = 1;
			_scaleBox.Minimum = 1;
			_scaleBox.Maximum = 256;
			_scaleBox.Value = 16;

			_folderLabel = new Label();
			_folderLabel.Name = "folderLabel";
			_folderLabel.Text = "Folder: ";
			_folderLabel.AutoSize = true;
			_folderLabel.Location = new Point(142, 58);

			_folderBtn = new Button();
			_folderBtn.Name = "folderBtn";
			_folderBtn.Text = "...";
			_folderBtn.Size = new Size(30, 20);
			_folderBtn.Location = new Point(190, 56);

			_nameLabel = new Label();
			_nameLabel.Name = "nameLabel";
			_nameLabel.Text = "Name: ";
			_nameLabel.AutoSize = true;
			_nameLabel.Location = new Point(142, 86);

			_nameBox = new TextBox();
			_nameBox.Name = "nameBox";
			_nameBox.Text = "sprite";
			_nameBox.AutoSize = true;
			_nameBox.Location = new Point(190, 84);

			_overwriteBtn = new RadioButton();
			_overwriteBtn.Name = "overwriteBtn";
			_overwriteBtn.Text = "Overwrite";
			_overwriteBtn.AutoSize = true;
			_overwriteBtn.Location = new Point(158, 114);

			_appendBtn = new RadioButton();
			_appendBtn.Name = "appendBtn";
			_appendBtn.Text = "Append";
			_appendBtn.AutoSize = true;
			_appendBtn.Location = new Point(158, 134);

			_appendBox = new TextBox();
			_appendBox.Name = "appendBox";
			_appendBox.Text = "_nnnn";
			_appendBox.Size = new Size(50, 20);
			_appendBox.Location = new Point(222, 133);
			_appendBox.Enabled = false;

			_saveButton = new Button();
			_saveButton.Name = "saveButton";
			//_saveButton.Text = "Save";
			_saveButton.Size = new Size(32, 32);
			//_saveButton.AutoSize = true;
			_saveButton.Location = new Point(259, 8);
			_saveButton.Image = (Image)(resources.GetObject("saveImg"));

			this.Controls.Add(_tileLabel);
			this.Controls.Add(_outputLabel);

			this.Controls.Add(_rotateRightBtn);
			this.Controls.Add(_rotateLeftBtn);
			this.Controls.Add(_mirrorHoriBtn);
			this.Controls.Add(_mirrorVertBtn);
			this.Controls.Add(_eraseBtn);

			this.Controls.Add(_scaleLabel);
			this.Controls.Add(_scaleBox);
			this.Controls.Add(_folderLabel);
			this.Controls.Add(_folderBtn);
			this.Controls.Add(_nameLabel);
			this.Controls.Add(_nameBox);
			this.Controls.Add(_overwriteBtn);
			this.Controls.Add(_appendBtn);
			this.Controls.Add(_appendBox);
			this.Controls.Add(_saveButton);
		}

		public void AdjustContents()
		{
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			e.Graphics.DrawLine(
				_dividerPen,
				_divider.X,
				_divider.Y,
				_divider.X + _divider.Width,
				_divider.Y + _divider.Height
			);
		}
	}
}
