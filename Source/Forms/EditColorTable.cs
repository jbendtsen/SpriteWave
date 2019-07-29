using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public class EditColorTable : Form
	{
		private ColorPicker _picker;

		private Bitmap _sample;
		private Rectangle _sampleRect = new Rectangle(0, 0, 8, 8);

		public EditColorTable()
		{
			_sample = new Bitmap(_sampleRect.Width, _sampleRect.Height);

			this.Name = "editColorTable";
			this.Text = "Color Table";

			// When setting the size of the client area inside the window (.ClientSize),
			//  the overall window size (.Size) is also set, to a larger area.
			// .MinimumSize makes use of .Size, so we set it after .Size has been calculated.
			this.ClientSize = new Size(456, 300);
			this.MinimumSize = this.Size;

			_picker = new ColorPicker(256);

			this.Controls.Add(_picker);
			_picker.Render();

			this.KeyPreview = true;
			this.KeyUp += (s, e) => ResetIcon();

			Utils.ControlFunc updateFormIcon = (ctrl, args) => {ctrl.MouseUp += (s, e) => ResetIcon(); return null;};
			Utils.ApplyRecursiveControlFunc(_picker, updateFormIcon);

			ResetIcon();
		}

		public void ResetIcon()
		{
			Utils.ClearBitmap(_sample, _picker.Color, _sampleRect);
			this.Icon = Icon.FromHandle(_sample.GetHicon());
		}
	}
}
