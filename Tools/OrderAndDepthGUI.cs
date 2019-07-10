using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	class InputModule
	{
		private string _name;

		private uint _value;
		public uint Value
		{
			get {
				return _value;
			}
			set {
				_value = value;
				string lbl = _name + ": 0x" + _value.ToString("X8");
				SetText(lbl, "", null);
			}
		}

		private Label _label;
		private TextBox _box;
		private Button _submit;

		private Action<InputModule> _action;
		public Action<InputModule> OnSubmit { set { _action = value; } }

		public uint Input
		{
			get {
				uint res;

				string text = _box.Text;
				if (text == null || text.Length < 1)
					res = this.Value;
				else
					res = Convert.ToUInt32(text, 16);

				return res;
			}
		}

		public bool Visible
		{
			set {
				_label.Visible = value;
				_box.Visible = value;
				_submit.Visible = value;
			}
		}

		public InputModule(OrderAndDepthGUI mainForm, string name, int x, int y)
		{
			_name = name;
			_value = 0;

			_label = new Label();
			_label.Location = new Point(x - 5, y);
			_label.BackColor = Color.White;
			_label.TextAlign = ContentAlignment.MiddleCenter;
			_label.Visible = false;

			_box = new TextBox();
			_box.Location = new Point(x, y + 30);
			_box.BackColor = Color.White;
			_box.KeyDown += new KeyEventHandler(EditSpecBox);
			_box.Visible = false;

			_submit = new Button();
			_submit.Location = new Point(x + 12, y + 60);
			_submit.BackColor = Color.White;
			_submit.Click += new EventHandler(ClickSpecButton);
			_submit.Text = "Submit";
			_submit.Visible = false;

			_label.Size = new Size(_box.Width + 10, _label.Height);

			mainForm.Controls.Add(_label);
			mainForm.Controls.Add(_box);
			mainForm.Controls.Add(_submit);
		}

		public void SetText(string lblText, string boxText, string btnText)
		{
			if (lblText != null)
				_label.Text = lblText;
			if (boxText != null)
				_box.Text = boxText;
			if (btnText != null)
				_submit.Text = btnText;
		}

		private void Submit()
		{
			if (_action == null)
				return;

			try {
				_action(this);
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message, "Uh Oh");
			}
		}

		private void EditSpecBox(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				Submit();
		}

		private void ClickSpecButton(object sender, EventArgs e)
		{
			Submit();
		}
	}

	class OrderAndDepthGUI : Form
	{
		ColourTable _table;
		InputModule _spec, _rgba, _native;

		public OrderAndDepthGUI()
		{
			this.Text = "Order And Depth GUI";

			_spec = new InputModule(this, "Spec", 95, 25);
			_spec.SetText("No spec given", "01238888", "New Spec");
			_spec.OnSubmit = NewSpec;
			_spec.Visible = true;

			_rgba = new InputModule(this, "RGBA", 30, 145);
			_rgba.SetText("RGBA Colour Field", null, "--->");
			_rgba.OnSubmit = RGBAToNative;

			_native = new InputModule(this, "Native", 160, 145);
			_native.SetText("Native Colour Field", null, "<---");
			_native.OnSubmit = NativeToRGBA;
		}

		public void SetColour(uint rgba)
		{
			int rgb = (int)(rgba >> 8);
			this.BackColor = Color.FromArgb(255, Color.FromArgb(rgb));
		}

		public void NewSpec(InputModule mod)
		{
			uint seed = mod.Input;
			_table = new ColourTable(seed, null);

			mod.Value = seed;

			_rgba.SetText(null, "", null);
			_rgba.Visible = true;

			_native.SetText(null, "", null);
			_native.Visible = true;
		}

		public void RGBAToNative(InputModule mod)
		{
			uint rgba = mod.Input;
			uint nat = _table.RGBAToNative(rgba);

			_rgba.Value = rgba;
			_native.Value = nat;

			SetColour(rgba);
		}

		public void NativeToRGBA(InputModule mod)
		{
			uint nat = mod.Input;
			uint rgba = _table.NativeToRGBA(nat);

			_native.Value = nat;
			_rgba.Value = rgba;

			SetColour(rgba);
		}
	}

	class OrderAndDepthMain
	{
		static void Main()
		{
			Application.Run(new OrderAndDepthGUI());
		}
	}
}