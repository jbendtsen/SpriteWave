using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	class TestSuffix : Panel
	{
		Suffix _suff;

		TextBox _mainBox;
		Button _mainButton;

		TextBox _valueBox;
		TextBox _stringBox;

		Button _generateButton;
		Button _valueOfButton;

		public TestSuffix()
		{
			this.Location = new Point(0, 0);

			_mainBox = new TextBox();
			_mainBox.Name = "mainBox";
			_mainBox.Text = "_{d2}";
			_mainBox.Location = new Point(0, 20);

			_mainButton = new Button();
			_mainButton.Name = "mainButton";
			_mainButton.Text = "New Suffix";
			_mainButton.Location = new Point(0, 50);
			_mainButton.Click += this.newSuffix;

			_valueBox = new TextBox();
			_valueBox.Name = "valueBox";
			_valueBox.Text = "8";
			_valueBox.Location = new Point(0, 90);
			_valueBox.Enabled = false;

			_stringBox = new TextBox();
			_stringBox.Name = "stringBox";
			_stringBox.Text = "";
			_stringBox.Location = new Point(0, 90);
			_stringBox.Enabled = false;

			_generateButton = new Button();
			_generateButton.Name = "generateButton";
			_generateButton.Text = "--->";
			_generateButton.Location = new Point(0, 120);
			_generateButton.Click += this.generate;
			_generateButton.Enabled = false;

			_valueOfButton = new Button();
			_valueOfButton.Name = "valueOfButton";
			_valueOfButton.Text = "<---";
			_valueOfButton.Location = new Point(0, 120);
			_valueOfButton.Click += this.valueOf;
			_valueOfButton.Enabled = false;

			this.Controls.Add(_mainBox);
			this.Controls.Add(_mainButton);
			this.Controls.Add(_valueBox);
			this.Controls.Add(_stringBox);
			this.Controls.Add(_generateButton);
			this.Controls.Add(_valueOfButton);
		}

		public void AdjustContents()
		{
			Action<Control, float> centre = (ctrl, fac) =>
			{
				int x = (int)((float)this.Size.Width * Math.Abs(fac) - (float)ctrl.Size.Width) / 2;
				if (fac < 0)
					x += this.Size.Width / 2;

				ctrl.Location = new Point(x, ctrl.Location.Y);
			};

			centre(_mainBox, 1f);
			centre(_mainButton, 1f);
			centre(_valueBox, 0.5f);
			centre(_stringBox, -0.5f);
			centre(_generateButton, 0.5f);
			centre(_valueOfButton, -0.5f);
		}

		void newSuffix(object sender, EventArgs e)
		{
			_suff = new Suffix(_mainBox.Text);
			_valueBox.Enabled = true;
			_stringBox.Enabled = true;
			_generateButton.Enabled = true;
			_valueOfButton.Enabled = true;
		}

		void generate(object sender, EventArgs e)
		{
			_stringBox.Text = _suff.Generate(Convert.ToInt32(_valueBox.Text));
			_valueBox.Text = _suff.ValueOf(_stringBox.Text).ToString();
		}

		void valueOf(object sender, EventArgs e)
		{
			_valueBox.Text = _suff.ValueOf(_stringBox.Text).ToString();
			_stringBox.Text = _suff.Generate(Convert.ToInt32(_valueBox.Text));
		}
	}

	class SuffixTool : Form
	{
		Pen _dividerPen;
		const float dividerGap = 0.0625f;

		TestSuffix _testSuffix;

		public SuffixTool()
		{
			this.Name = "Suffix Tool";
			this.Size = new Size(350, 450);
			this.ResizeRedraw = true;
			this.Layout += this.layoutHandler;

			_dividerPen = new Pen(Color.Silver);

			_testSuffix = new TestSuffix();
			_testSuffix.Name = "testSuffix";

			this.Controls.Add(_testSuffix);
		}

		void layoutHandler(object sender, LayoutEventArgs e)
		{
			_testSuffix.Size = new Size(this.ClientSize.Width, this.ClientSize.Height / 2);
			_testSuffix.AdjustContents();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			float w = this.ClientSize.Width;
			float h = this.ClientSize.Height;
			float gap = w * dividerGap;

			e.Graphics.DrawLine(
				_dividerPen,
				gap,
				h / 2f,
				w - gap,
				h / 2f
			);
		}
	}

	class SuffixToolMain
	{
		static void Main()
		{
			Application.Run(new SuffixTool());
		}
	}
}