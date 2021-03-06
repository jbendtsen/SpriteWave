﻿using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace SpriteWave
{
	public class SpriteControlsTab : ITab
	{
		private readonly Color infoColor = Color.FromArgb(255, 48, 48, 48);

		private Rectangle _divider = new Rectangle(120, 5, 0, 174);
		private readonly Pen _dividerPen = new Pen(Color.Silver);

		private string _name;
		private string _id;
		private Button _tabButton;
		private Panel _panel;
		private readonly SpriteWindow _wnd;

		private Label _tileLabel;
		private Label _outputLabel;

		private Button _rotateLeftBtn;
		private Button _rotateRightBtn;
		private Button _mirrorHoriBtn;
		private Button _mirrorVertBtn;
		private Button _eraseBtn;

		private Label _scaleLabel;
		private NumericUpDown _scaleBox;
		private Label _scaleHint;

		private Label _folderLabel;
		private Button _folderBtn;
		private Label _folderText;

		private Label _nameLabel;
		private TextBox _nameBox;
		private Label _nameHint;

		private RadioButton _overwriteBtn;
		private RadioButton _appendBtn;
		private TextBox _appendBox;
		private Button _saveButton;
		private Label _saveMsg;

		private FolderBrowserDialog _folderBrowser;
		private bool _pathSelected = false;

		public string Name { get { return _name; } }
		public string ID { get { return _id; } }
		public Button TabButton { get { return _tabButton; } }
		public Panel Panel { get { return _panel; } }
		public TileWindow Window { get { return _wnd; } set {} }

		public Size Minimum { get { return new Size(320, 195); } }

		public int X { set { _panel.Location = new Point(value, _panel.Location.Y); } }

		public SpriteControlsTab(SpriteWindow wnd)
		{
			_wnd = wnd;
			_id = "spriteControlsTab";
			_name = "Controls";

			_tabButton = new ToolBoxButton(_name);
			_tabButton.Tag = this;

			_panel = new Panel();
			_panel.Name = "spriteControlsPanel";
			_panel.Paint += this.drawDivider;

			_folderBrowser = new FolderBrowserDialog();
			var resources = new ComponentResourceManager(typeof(SpriteControlsTab));

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
			_rotateLeftBtn.BackColor = Color.Transparent;
			_rotateLeftBtn.Image = (Image)(resources.GetObject("rotateLeftImg"));
			_rotateLeftBtn.Click += (s, e) => _wnd.FlipTile(Translation.Left);

			_rotateRightBtn = new Button();
			_rotateRightBtn.Name = "rotateRightBtn";
			_rotateRightBtn.Size = new Size(32, 32);
			_rotateRightBtn.Location = new Point(66, 30);
			_rotateRightBtn.BackColor = Color.Transparent;
			_rotateRightBtn.Image = (Image)(resources.GetObject("rotateRightImg"));
			_rotateRightBtn.Click += (s, e) => _wnd.FlipTile(Translation.Right);

			_mirrorHoriBtn = new Button();
			_mirrorHoriBtn.Name = "mirrorHoriBtn";
			_mirrorHoriBtn.Size = new Size(32, 32);
			_mirrorHoriBtn.Location = new Point(26, 70);
			_mirrorHoriBtn.BackColor = Color.Transparent;
			_mirrorHoriBtn.Image = (Image)(resources.GetObject("mirrorHoriImg"));
			_mirrorHoriBtn.Click += (s, e) => _wnd.FlipTile(Translation.Horizontal);

			_mirrorVertBtn = new Button();
			_mirrorVertBtn.Name = "mirrorVertBtn";
			_mirrorVertBtn.Size = new Size(32, 32);
			_mirrorVertBtn.Location = new Point(66, 70);
			_mirrorVertBtn.BackColor = Color.Transparent;
			_mirrorVertBtn.Image = (Image)(resources.GetObject("mirrorVertImg"));
			_mirrorVertBtn.Click += (s, e) => _wnd.FlipTile(Translation.Vertical);

			_eraseBtn = new Button();
			_eraseBtn.Name = "eraseBtn";
			_eraseBtn.Text = "Erase";
			_eraseBtn.AutoSize = true;
			_eraseBtn.Location = new Point(25, 110);
			_eraseBtn.BackColor = Color.Transparent;
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
			_scaleBox.Text = "";
			_scaleBox.ValueChanged += (s, e) => UpdateUI();
			_scaleBox.Leave += (s, e) => UpdateUI();

			_scaleHint = new Label();
			_scaleHint.Name = "scaleHint";
			_scaleHint.Text = "No scale set";
			_scaleHint.Font = new Font(Label.DefaultFont, FontStyle.Italic);
			_scaleHint.ForeColor = infoColor;
			_scaleHint.AutoSize = true;
			_scaleHint.Location = new Point(234, 33);

			_folderLabel = new Label();
			_folderLabel.Name = "folderLabel";
			_folderLabel.Text = "Folder: ";
			_folderLabel.AutoSize = true;
			_folderLabel.Location = new Point(142, 58);

			_folderBtn = new Button();
			_folderBtn.Name = "folderBtn";
			_folderBtn.Text = "...";
			_folderBtn.Size = new Size(30, 20);
			_folderBtn.Location = new Point(189, 55);
			_folderBtn.BackColor = Color.Transparent;
			_folderBtn.Click += this.browseFolderHandler;

			_folderText = new Label();
			_folderText.Name = "folderText";
			_folderText.Text = "No path set";
			_folderText.Font = new Font(Label.DefaultFont, FontStyle.Italic);
			_folderText.ForeColor = infoColor;
			_folderText.AutoEllipsis = true;
			_folderText.Size = new Size(70, 13);
			_folderText.Location = new Point(226, 58);

			_nameLabel = new Label();
			_nameLabel.Name = "nameLabel";
			_nameLabel.Text = "Name: ";
			_nameLabel.AutoSize = true;
			_nameLabel.Location = new Point(142, 84);

			_nameBox = new TextBox();
			_nameBox.Name = "nameBox";
			_nameBox.Text = "";
			_nameBox.AutoSize = true;
			_nameBox.Location = new Point(190, 81);
			_nameBox.TextChanged += (s, e) => UpdateUI();
			_nameBox.Leave += (s, e) => UpdateUI();

			_nameHint = new Label();
			_nameHint.Name = "nameHint";
			_nameHint.Text = "No name set";
			_nameHint.Font = new Font(Label.DefaultFont, FontStyle.Italic);
			_nameHint.ForeColor = infoColor;
			_nameHint.AutoSize = true;
			_nameHint.Location = new Point(190, 104);

			_overwriteBtn = new RadioButton();
			_overwriteBtn.Name = "overwriteBtn";
			_overwriteBtn.Text = "Overwrite";
			_overwriteBtn.AutoSize = true;
			_overwriteBtn.Location = new Point(186, 124);
			_overwriteBtn.Checked = true;
			_overwriteBtn.CheckedChanged += this.writeModeCheck;

			_appendBtn = new RadioButton();
			_appendBtn.Name = "appendBtn";
			_appendBtn.Text = "Append:";
			_appendBtn.AutoSize = true;
			_appendBtn.Location = new Point(186, 144);
			_appendBtn.CheckedChanged += this.writeModeCheck;

			_appendBox = new TextBox();
			_appendBox.Name = "appendBox";
			_appendBox.Text = "_{d2}";
			_appendBox.Size = new Size(50, 20);
			_appendBox.Location = new Point(254, 143);
			_appendBox.Enabled = false;
			// No TextChanged event here ;)
			_appendBox.Leave += (s, e) => UpdateUI();

			_saveButton = new Button();
			_saveButton.Name = "saveButton";
			_saveButton.Size = new Size(32, 32);
			_saveButton.Location = new Point(140, 127);
			_saveButton.BackColor = Color.Transparent;
			_saveButton.Image = (Image)(resources.GetObject("saveImg"));
			_saveButton.Click += this.saveButtonHandler;
			_saveButton.Enabled = false;

			_saveMsg = new Label();
			_saveMsg.Name = "saveMsg";
			_saveMsg.Text = "";
			_saveMsg.Font = new Font(Label.DefaultFont, FontStyle.Italic);
			_saveMsg.ForeColor = infoColor;
			_saveMsg.AutoEllipsis = true;
			_saveMsg.Size = new Size(100, 13);
			_saveMsg.Location = new Point(142, 172);

			_panel.Controls.Add(_tileLabel);
			_panel.Controls.Add(_outputLabel);

			_panel.Controls.Add(_rotateRightBtn);
			_panel.Controls.Add(_rotateLeftBtn);
			_panel.Controls.Add(_mirrorHoriBtn);
			_panel.Controls.Add(_mirrorVertBtn);
			_panel.Controls.Add(_eraseBtn);

			_panel.Controls.Add(_scaleLabel);
			_panel.Controls.Add(_scaleBox);
			_panel.Controls.Add(_scaleHint);

			_panel.Controls.Add(_folderLabel);
			_panel.Controls.Add(_folderBtn);
			_panel.Controls.Add(_folderText);

			_panel.Controls.Add(_nameLabel);
			_panel.Controls.Add(_nameBox);
			_panel.Controls.Add(_nameHint);

			_panel.Controls.Add(_overwriteBtn);
			_panel.Controls.Add(_appendBtn);
			_panel.Controls.Add(_appendBox);
			_panel.Controls.Add(_saveButton);
			_panel.Controls.Add(_saveMsg);
		}

		public bool HandleEscapeKey(MainForm main)
		{
			bool textFocus = main.ActiveControl is TextBox;
			if (textFocus)
				_wnd.Focus(main);

			return textFocus;
		}

		public void AdjustContents(Size size, ToolBoxOrientation layout)
		{
			_panel.Size = size;

			Action<Control> stretch = (ctrl) => ctrl.Size = new Size(size.Width - ctrl.Location.X - 5, ctrl.Height);

			stretch(_folderText);
			stretch(_saveMsg);
		}
		
		public void Destruct() {}

		public void UpdateUI()
		{
			bool scaleOK = _scaleBox.Text.Length >= 1 && _scaleBox.Value >= _scaleBox.Minimum && _scaleBox.Value <= _scaleBox.Maximum;
			bool nameOK = _nameBox.Text.Length > 0;
			bool writeModeOK = (_overwriteBtn.Checked || (_appendBtn.Checked && _appendBox.Text.Length > 0));

			_scaleHint.Visible = !scaleOK;
			_nameHint.Visible = !nameOK;

			_saveButton.Enabled = scaleOK && _pathSelected && nameOK && writeModeOK;
		}

		protected void browseFolderHandler(object sender, EventArgs e)
		{
			var result = _folderBrowser.ShowDialog();
			if (result != DialogResult.OK)
				return;

			_folderText.Font = new Font(Label.DefaultFont, FontStyle.Regular);
			_folderText.Text = _folderBrowser.SelectedPath;
			_pathSelected = true;

			UpdateUI();
		}

		protected void writeModeCheck(object sender, EventArgs e)
		{
			_appendBox.Enabled = (sender == _appendBtn);

			UpdateUI();
		}

		protected void saveButtonHandler(object sender, EventArgs e)
		{
			string path = _folderText.Text;
			if (path[path.Length - 1] != '\\')
				path += '\\';

			string name = _nameBox.Text;
			if (_appendBtn.Checked)
			{
				name += _appendBox.Text + _wnd.ExportExtension;

				string[] fileList = Directory.GetFiles(path);
				for (int i = 0; i < fileList.Length; i++)
					fileList[i] = Path.GetFileName(fileList[i]);

				try {
					var suff = new Suffix(name);
					int[] exportList = suff.ListOfValues(fileList);

					int nextNum = 0;
					if (exportList != null)
						nextNum = exportList[exportList.Length - 1] + 1;
	
					name = suff.Generate(nextNum);
				}
				catch (Exception ex) {
					MessageBox.Show(ex.Message);
				}
			}
			else
				name += _wnd.ExportExtension;

			string fullPath = path + name;
			_wnd.Export(fullPath, (int)_scaleBox.Value);

			string abrvName;
			if (path.Length >= 10)
				abrvName = path.Substring(0, 7) + "...\\" + name;
			else
				abrvName = fullPath;

			_saveMsg.Text = "Saved " + abrvName;
		}

		protected void drawDivider(object sender, PaintEventArgs e)
		{
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
