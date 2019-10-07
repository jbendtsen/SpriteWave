using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace SpriteWave
{
	public class ColorBox : PictureBox
	{
		public const int Border = 2;
		public byte[] Buffer;

		private Size _oldSize;
		private BitmapData _data;

		public ColorBox(ColorPicker.CursorHandler moveCursor, Action setColor, ColorPicker.ScrollHandler scroll)
		{
			_oldSize = new Size(0, 0);

			if (moveCursor != null)
			{
				this.MouseDown += (s, e) => moveCursor(e.X, e.Y);
				this.MouseMove += (s, e) => { if (e.Button != MouseButtons.None && s == this) moveCursor(e.X, e.Y); };
				this.MouseUp   += (s, e) => { moveCursor(e.X, e.Y); setColor(); };
			}
			if (scroll != null)
				this.MouseWheel += (s, e) => scroll(e.Delta / 120);
		}
		public ColorBox() : this(null, null, null) {}

		public void Lock()
		{
			if (_oldSize != this.Size || this.Image == null)
			{
				this.Image = new Bitmap(this.Width, this.Height, PixelFormat.Format32bppArgb);
				Buffer = new byte[this.Width * this.Height * 4];
			}

			_data = (this.Image as Bitmap).LockBits(
				this.DisplayRectangle,
				ImageLockMode.ReadWrite,
				PixelFormat.Format32bppArgb
			);
		}

		public void Unlock()
		{
			if (_data == null)
				return;

			Marshal.Copy(Buffer, 0, _data.Scan0, Buffer.Length);
			(this.Image as Bitmap).UnlockBits(_data);
			this.Invalidate();

			_oldSize = this.Size;
		}
	}
}
