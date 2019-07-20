using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public class PaletteBox : Panel
	{
		private const int scrollW = 17;

		private PictureBox _box;
		private VScrollBar _scroll;

		private Brush _br;
		public Color Fill { set { _br = new SolidBrush(value); } }

		public PaletteBox(Point loc, Size size)
		{
			this.Name = "paletteBox";
			this.Location = loc;
			this.Size = size;

			_box = new PictureBox();
			_scroll = new VScrollBar();
			AdjustContents();

			this.Controls.Add(_box);
			this.Controls.Add(_scroll);
		}

		public void AdjustContents()
		{
			_scroll.Location = new Point(this.Size.Width - scrollW - 1, 0);
			_scroll.Size = new Size(scrollW, this.Size.Height);

			_box.Location = new Point(0, 0);
			_box.Size = new Size(this.Size.Width - scrollW, this.Size.Height);
			_box.Image = new Bitmap(_box.Size.Width, _box.Size.Height);
		}

		public void Draw()
		{
			using (var g = Graphics.FromImage(_box.Image))
			{
				Size s = _box.Image.Size;
				g.FillRectangle(_br, 0, 0, s.Width, s.Height);
			}
		}
	}
}
