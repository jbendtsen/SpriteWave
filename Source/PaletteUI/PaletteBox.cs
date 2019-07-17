using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public class PaletteBox : PictureBox
	{
		private Brush _br;
		public Color Fill { set { _br = new SolidBrush(value); } }

		public PaletteBox()
		{
			
		}

		public void Draw()
		{
			using (var g = Graphics.FromImage(this.Image))
			{
				Size s = this.Image.Size;
				g.FillRectangle(_br, 0, 0, s.Width, s.Height);
			}
		}
	}
}
