using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public interface ISuffixPanel
	{
		int MinimumHeight { get; }
		void AdjustContents();
	}

	public class TextSubmit
	{
		TextBox _text;
		Button _submit;

		public string Text { get { return _text.Text; } set { _text.Text = value; } }

		public bool Enabled
		{
			get {
				return _text.Enabled || _submit.Enabled;
			}
			set {
				_text.Enabled = value;
				_submit.Enabled = value;
			}
		}

		public int X
		{
			set {
				_text.Location = new Point(value - (_text.Size.Width / 2), _text.Location.Y);
				_submit.Location = new Point(value - (_submit.Size.Width / 2) - 1, _submit.Location.Y);
			}
		}

		public TextSubmit(Panel p, string name, int y, string btnText, EventHandler action)
		{
			_text = new TextBox();
			_text.Name = name + "Box";
			_text.Text = "";
			_text.Location = new Point(0, y);
			_text.Enabled = false;

			_submit = new Button();
			_submit.Name = name + "Button";
			_submit.Text = btnText;
			_submit.Location = new Point(0, y + 30);
			_submit.Click += action;
			_submit.Enabled = false;

			p.Controls.Add(_text);
			p.Controls.Add(_submit);
		}
	}

	public class TestSuffix : Panel, ISuffixPanel
	{
		Suffix _suff;

		Label _desc;
		TextSubmit _main;
		TextSubmit _value;
		TextSubmit _string;

		public int MinimumHeight { get { return 200; } }

		public TestSuffix()
		{
			this.Location = new Point(0, 0);

			_desc = new Label();
			_desc.Text = "Testing Area";
			_desc.Font = new Font(Label.DefaultFont, FontStyle.Bold);
			_desc.Location = new Point(15, 15);

			_main = new TextSubmit(this, "main", 40, "New Suffix", this.newSuffix);
			_main.Text = "example_{d2}";
			_main.Enabled = true;

			_value = new TextSubmit(this, "value", 120, "--->", this.generate);
			_value.Text = "8";

			_string = new TextSubmit(this, "string", 120, "<---", this.valueOf);

			this.Controls.Add(_desc);
		}

		public void AdjustContents()
		{
			int mid = this.Size.Width / 2;
			_main.X = mid;
			_value.X = mid - 70;
			_string.X = mid + 70;
		}

		void newSuffix(object sender, EventArgs e)
		{
			bool success = true;
			try {
				_suff = new Suffix(_main.Text);
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message);
				success = false;
			}

			_value.Enabled = success;
			_string.Enabled = success;
		}

		void generate(object sender, EventArgs e)
		{
			try {
				_string.Text = _suff.Generate(Convert.ToInt32(_value.Text));
				_value.Text = _suff.ValueOf(_string.Text).ToString();
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message);
				_string.Text = "";
			}
		}

		void valueOf(object sender, EventArgs e)
		{
			try {
				_value.Text = _suff.ValueOf(_string.Text).ToString();
				_string.Text = _suff.Generate(Convert.ToInt32(_value.Text));
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message);
				_value.Text = "";
			}
		}
	}

	public class InputField
	{
		Label _desc;
		TextBox _input;

		public string Text
		{
			get {
				return _input.Text;
			}
		}

		public InputField(Panel p, string name, int y, string text, Action textChanged)
		{
			_desc = new Label();
			_desc.Name = name + "Label";
			_desc.Text = text;
			_desc.AutoSize = true;
			_desc.Location = new Point(0, y);

			_input = new TextBox();
			_input.Name = name + "Box";
			_input.Text = "";
			_input.AutoSize = true;
			_input.Location = new Point(0, y);
			_input.TextChanged += (s, e) => textChanged();

			p.Controls.Add(_desc);
			p.Controls.Add(_input);
		}

		public void SetPosition(int labelX, int boxX, int y)
		{
			_desc.Location = new Point(
				labelX,
				y - (_desc.Size.Height / 2)
			);
			_input.Location = new Point(
				boxX,
				y - (_input.Size.Height / 2)
			);
		}
	}

	public class RenamePanel : Panel
	{
		Label _desc;
		InputField _srcField;
		InputField _dstField;

		Label _folderDesc;
		Button _folderBtn;
		Label _folderText;

		Label _resultText;
		Button _submit;

		FolderBrowserDialog _browse;

		public int MinimumHeight { get { return 230; } }

		string Output
		{
			set {
				_resultText.Text = value;
				_resultText.Location = new Point(
					(this.Size.Width - _resultText.Size.Width) / 2,
					_resultText.Location.Y
				);
			}
		}

		public RenamePanel()
		{
			_browse = new FolderBrowserDialog();

			_desc = new Label();
			_desc.Text = "Batch Rename";
			_desc.Font = new Font(Label.DefaultFont, FontStyle.Bold);
			_desc.AutoSize = true;
			_desc.Location = new Point(15, 15);

			_srcField = new InputField(this, "source", 40, "Suffix To Match", this.UpdateUI);

			_folderDesc = new Label();
			_folderDesc.Text = "Folder To Process";
			_folderDesc.AutoSize = true;

			_folderBtn = new Button();
			_folderBtn.Text = "...";
			_folderBtn.Size = new Size(40, 20);
			_folderBtn.Click += this.browseFolderHandler;

			_folderText = new Label();
			_folderText.Text = "";
			_folderText.AutoEllipsis = true;
			_folderText.Font = new Font(Label.DefaultFont, FontStyle.Italic);
			//_folderText.Size = new Size(60, 17);

			_dstField = new InputField(this, "dest", 120, "Replacement Suffix", this.UpdateUI);

			_resultText = new Label();
			_resultText.Text = "";
			_resultText.ForeColor = Color.Navy;
			_resultText.AutoSize = true;

			_submit = new Button();
			_submit.Text = "Process";
			_submit.Enabled = false;
			_submit.Click += this.process;

			this.Controls.Add(_desc);
			this.Controls.Add(_folderDesc);
			this.Controls.Add(_folderBtn);
			this.Controls.Add(_folderText);
			this.Controls.Add(_resultText);
			this.Controls.Add(_submit);
		}

		public void AdjustContents()
		{
			const int labelX = -115;
			const int boxX = 15;

			int midX = this.Size.Width / 2;
			int midY = this.Size.Height / 2;
			Action<Control, int, int> centre = (ctrl, x, y) =>
			{
				ctrl.Location = new Point(
					midX + x,
					midY + y
				);
			};

			_srcField.SetPosition(midX + labelX, midX + boxX, midY - 50);
			//_srcField.Position = new Point(mid, 50);

			centre(_folderDesc, labelX, -17);
			centre(_folderBtn, boxX, -20);
			centre(_folderText, 65, -17);

			_folderText.Size = new Size(this.Size.Width - _folderText.Location.X - 10, 17);

			//_dstField.Position = new Point(mid, 130);
			_dstField.SetPosition(midX + labelX, midX + boxX, midY + 30);

			centre(_resultText, _resultText.Size.Width / -2, 55);
			centre(_submit, _submit.Size.Width / -2, 75);
		}

		void UpdateUI()
		{
			_submit.Enabled =
				_srcField.Text.Length > 0 &&
				_folderText.Text.Length > 0 &&
				_dstField.Text.Length > 0
			;
		}

		void browseFolderHandler(object sender, EventArgs e)
		{
			var result = _browse.ShowDialog();
			if (result != DialogResult.OK)
				return;

			_folderText.Text = _browse.SelectedPath;
			UpdateUI();
		}

		void process(object sender, EventArgs e)
		{
			this.Output = "";

			string path = _folderText.Text + "\\";
			string[] fileList = Directory.GetFiles(path);
			string msg = fileList[0] + "\n";

			int nFiles = 0;
			string plural = "";

			for (int i = 0; i < fileList.Length; i++)
				fileList[i] = Path.GetFileName(fileList[i]);

			try {
				Suffix src = new Suffix(_srcField.Text);
				Suffix dst = new Suffix(_dstField.Text);
				if (!src.HasInsert || !dst.HasInsert)
					throw new ArgumentException("Both suffix inputs must contain an insert (eg. {d2})");

				int[] values = src.ListOfValues(fileList);
				if (values == null)
					throw new ArgumentException("No files matching \"" + _srcField.Text + "\" were found");

				nFiles = values.Length;
				plural = nFiles != 1 ? "s" : "";

				Action<int> rename = (idx) =>
				{
					string outName = dst.Generate(values[idx]);
					string srcFile = path + fileList[idx];
					string dstFile = path + outName;
					
					if (!File.Exists(srcFile))
						return;

					if (File.Exists(dstFile))
					{
						if (outName.ToLower() != src.Generate(values[idx]).ToLower())
							throw new InvalidOperationException("i cant code :)");
						else
							return;
					}

					File.Move(srcFile, dstFile);
				};

				for (int i = 0; i < nFiles; i++)
				{
					// This ensures that there aren't any rename conflicts,
					//  where the destination file has the same name as the source file,
					//  yet does not refer to the same number
					int idx = src.Base > dst.Base ? nFiles-i-1 : i;
					rename(idx);
				}
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message);
				return;
			}

			this.Output = "Successfully renamed " + nFiles + " file" + plural;
		}
	}

	class SuffixTool : Form
	{
		Pen _divPen;
		int _divY;
		const float dividerGap = 0.0625f;

		TestSuffix _testSuffix;
		RenamePanel _renamePanel;

		public SuffixTool()
		{
			this.Text = "Suffix Tool";
			this.Size = new Size(350, 420);
			this.ResizeRedraw = true;
			this.Layout += this.layoutHandler;

			_divPen = new Pen(Color.Silver);

			_testSuffix = new TestSuffix();
			_testSuffix.Name = "testSuffix";

			_renamePanel = new RenamePanel();
			_renamePanel.Name = "renamePanel";

			this.MinimumSize = new Size(300, 20 + _testSuffix.MinimumHeight + _renamePanel.MinimumHeight);

			this.Controls.Add(_testSuffix);
			this.Controls.Add(_renamePanel);
		}

		void layoutHandler(object sender, LayoutEventArgs e)
		{
			_divY = _testSuffix.MinimumHeight;

			_testSuffix.Size = new Size(this.ClientSize.Width, _divY - 1);
			_testSuffix.AdjustContents();

			_renamePanel.Location = new Point(0, _divY + 2);
			_renamePanel.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - _renamePanel.Location.Y);
			_renamePanel.AdjustContents();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			float w = this.ClientSize.Width;
			float h = this.ClientSize.Height;
			float gap = w * dividerGap;

			e.Graphics.DrawLine(
				_divPen,
				gap,
				_divY,
				w - gap,
				_divY
			);
		}
	}

	class SuffixToolMain
	{
		[STAThread]
		static void Main()
		{
			Application.Run(new SuffixTool());
		}
	}
}