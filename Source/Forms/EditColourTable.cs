using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public class EditColourTable : Form
	{
		private ColourPicker _picker;

		public EditColourTable()
		{
			this.Name = "editColourTable";
			this.Text = "Colour Table";

			// When setting the size of the client area inside the window (.ClientSize),
			//  the overall window size (.Size) is also set, to a larger area.
			// .MinimumSize makes use of .Size, so we set it after .Size has been calculated.
			this.ClientSize = new Size(320, 300);
			this.MinimumSize = this.Size;

			_picker = new ColourPicker(this);
			_picker.Render();

			//this.PerformLayout();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			_picker.PaintUnderUI(e.Graphics);
		}
	}
}
