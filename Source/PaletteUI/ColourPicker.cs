using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.ComponentModel;

namespace SpriteWave
{
	public enum BGRA
	{
		Blue, Green, Red, Alpha
	};

	public class ColourPicker
	{
		public delegate void CursorHandler(int x, int y);

		private const int nChans = 4;
		private const float dimmed = 0.7f;

		private float[] _chn;
		private int[] _order;

		private float[] _alphaShades;

		private Bitmap _slider;
		private Bitmap _dot;

		private Control _owner;

		private ColourBox _boxA;
		private ColourBox _boxXY;
		private ColourBox _boxZ;

		private Label[] _chnLabel;
		private TextBox[] _chnBox;

		public int Red { get; set; }
		public int Green { get; set; }
		public int Blue { get; set; }
		public int Alpha { get; set; }

		public Rectangle DotRect
		{
			get {
				int dotX = (int)((1f - _chn[_order[1]]) * (float)_boxXY.Size.Width);
				int dotY = (int)((1f - _chn[_order[2]]) * (float)_boxXY.Size.Height);
				int off = _dot.Size.Width / -2;

				return new Rectangle(
					dotX + off,
					dotY + off,
					_dot.Size.Width,
					_dot.Size.Height
				);
			}
		}

		public Rectangle SliderRect(PictureBox ctrl, int idx)
		{
			int h = (int)((1f - _chn[_order[idx]]) * (float)ctrl.Size.Height);
			int off = (ctrl.Size.Width - _slider.Size.Width) / 2;

			return new Rectangle(
				off,
				h + off,
				_slider.Size.Width,
				_slider.Size.Height
			);
		}

		public ColourPicker(Control owner)
		{
			_owner = owner;

			_chn = new[] {
				1f, 1f, 1f, 1f
			};

			_order = new[] {
				(int)BGRA.Alpha,
				(int)BGRA.Red,
				(int)BGRA.Green,
				(int)BGRA.Blue
			};

			_alphaShades = new[] {
				0.6f,
				0.8f
			};

			var resources = new ComponentResourceManager(typeof(ColourPicker));
			_slider = (Bitmap)(resources.GetObject("slider"));
			_dot = (Bitmap)(resources.GetObject("dot"));

			_boxA = new ColourBox(this.moveLeftSlider);
			_boxA.Name = "alphaBox";
			_boxA.Location = new Point(20, 20);
			_boxA.Size = new Size(20, 200);
			_boxA.Paint += this.paintLeftSlider;

			_boxXY = new ColourBox(this.moveDot);
			_boxXY.Name = "xyBox";
			_boxXY.Location = new Point(60, 20);
			_boxXY.Size = new Size(200, 200);
			_boxXY.Paint += this.paintDot;

			_boxZ = new ColourBox(this.moveRightSlider);
			_boxZ.Name = "zBox";
			_boxZ.Location = new Point(280, 20);
			_boxZ.Size = new Size(20, 200);
			_boxZ.Paint += this.paintRightSlider;

			_chnLabel = new Label[nChans];
			for (int i = 0; i < nChans; i++)
			{
				_chnLabel[i] = new Label();
				_chnLabel[i].Name = "chnLabel" + i;
				_chnLabel[i].Location = new Point(312, 40 + i * 40);
				_chnLabel[i].Size = new Size(10, 20);
				_owner.Controls.Add(_chnLabel[i]);
			}

			_chnLabel[0].Text = "R";
			_chnLabel[1].Text = "G";
			_chnLabel[2].Text = "B";
			_chnLabel[3].Text = "A";

			_chnBox = new TextBox[nChans];
			for (int i = 0; i < nChans; i++)
			{
				_chnBox[i] = new TextBox();
				_chnBox[i].Name = "chnBox" + i;
				_chnBox[i].Location = new Point(330, 37 + i * 40);
				_chnBox[i].Size = new Size(50, 20);
				_owner.Controls.Add(_chnBox[i]);
			}

			_owner.Controls.Add(_boxA);
			_owner.Controls.Add(_boxXY);
			_owner.Controls.Add(_boxZ);
		}

		public void SetAxis(int idx, float f)
		{
			_chn[_order[idx]] = Math.Min(Math.Max(f, 0), 1);
		}
		public void SetAxis(int idx, int num, int denom)
		{
			SetAxis(idx, 1f - ((float)num / (float)denom));
		}

		public void RefreshInputFields()
		{
			for (int i = 0; i < nChans; i++)
				_chnBox[_order[i]].Text = Math.Floor(_chn[_order[i]] * 255f).ToString();
		}

		public void Cycle()
		{
			Action<int[], int> tick = (arr, idx) => arr[idx] = (arr[idx] + 1) % 3;
			tick(_order, 1);
			tick(_order, 2);
			tick(_order, 3);
		}

		public void Render()
		{
			RenderAlphaBar();
			RenderMainBox();
			RenderRightBar();
			_owner.Invalidate();
		}

		private void RenderAlphaBar()
		{
			_boxA.Lock();

			var buf = _boxA.Buffer;
			int w = _boxA.Size.Width;
			int h = _boxA.Size.Height;
			int frame = ColourBox.Border;

			float axis1 = _chn[_order[1]];
			float axis2 = _chn[_order[2]];
			float axis3 = _chn[_order[3]];

			int blockSize = (w + 1) / 2;
			for (int i = 0; i < buf.Length; i += 4)
			{
				int xOdd = (((i / 4) % w) / blockSize) % 2;
				int yOdd = (((i / 4) / w) / blockSize) % 2;
				float shade = _alphaShades[xOdd ^ yOdd];

				float y = 1f - (float)((i / 4) / w) / (float)h;

				float lum = 255f;
				int p = i/4;
				if (p%w < frame || p%w >= w - frame || p/w < frame || p/w >= h - frame)
					lum = 255f * dimmed;

				buf[i + _order[0]] = 255;
				buf[i + _order[1]] = (byte)((shade + (axis1 - shade) * y) * lum);
				buf[i + _order[2]] = (byte)((shade + (axis2 - shade) * y) * lum);
				buf[i + _order[3]] = (byte)((shade + (axis3 - shade) * y) * lum);
			}

			_boxA.Unlock();
		}

		private void RenderMainBox()
		{
			_boxXY.Lock();

			var buf = _boxXY.Buffer;
			int w = _boxXY.Size.Width;
			int h = _boxXY.Size.Height;
			int frame = ColourBox.Border;

			float axis3 = _chn[_order[3]];

			for (int i = 0; i < buf.Length; i += 4)
			{
				float x = 1f - (float)((i / 4) % w) / (float)w;
				float y = 1f - (float)((i / 4) / w) / (float)h;
				float chn3 = axis3;

				int p = i/4;
				if (p%w < frame || p%w >= w - frame || p/w < frame || p/w >= h - frame)
				{
					x *= dimmed;
					y *= dimmed;
					chn3 *= dimmed;
				}

				buf[i + _order[0]] = 255; // axis0
				buf[i + _order[1]] = (byte)(x * 255f);
				buf[i + _order[2]] = (byte)(y * 255f);
				buf[i + _order[3]] = (byte)(chn3 * 255f);
			}

			_boxXY.Unlock();
		}

		private void RenderRightBar()
		{
			_boxZ.Lock();

			var buf = _boxZ.Buffer;
			int w = _boxZ.Size.Width;
			int h = _boxZ.Size.Height;
			int frame = ColourBox.Border;

			float axis1 = _chn[_order[1]];
			float axis2 = _chn[_order[2]];

			for (int i = 0; i < buf.Length; i += 4)
			{
				float y = 1f - (float)((i / 4) / w) / (float)h;
				float chn1 = axis1;
				float chn2 = axis2;

				int p = i/4;
				if (p%w < frame || p%w >= w - frame || p/w < frame || p/w >= h - frame)
				{
					y *= dimmed;
					chn1 *= dimmed;
					chn2 *= dimmed;
				}

				buf[i + _order[0]] = 255; // axis0
				buf[i + _order[1]] = (byte)(chn1 * 255f);
				buf[i + _order[2]] = (byte)(chn2 * 255f);
				buf[i + _order[3]] = (byte)(y * 255f);
			}

			_boxZ.Unlock();
		}

		private void paintLeftSlider(object sender, PaintEventArgs e)
		{
			e.Graphics.DrawImage(_slider, SliderRect(_boxA, 0));
		}
		private void paintDot(object sender, PaintEventArgs e)
		{
			e.Graphics.DrawImage(_dot, DotRect);
		}
		private void paintRightSlider(object sender, PaintEventArgs e)
		{
			e.Graphics.DrawImage(_slider, SliderRect(_boxZ, 3));
		}

		public void PaintUnderUI(Graphics g)
		{
			/*
			Action<Graphics, Pen, Pen, PictureBox> drawOutline = (gx, light, dark, ctrl) =>
			{
				int x1 = ctrl.Location.X - 1;
				int y1 = ctrl.Location.Y - 1;
				int x2 = x1 + ctrl.Size.Width + 1;
				int y2 = y1 + ctrl.Size.Height + 1;

				gx.DrawLine(dark, x1, y1, x2, y1);
				gx.DrawLine(dark, x1, y1, x1, y2);
				gx.DrawLine(light, x2, y2, x2, y1);
				gx.DrawLine(light, x2, y2, x1, y2);
			};

			using (var white = new Pen(Color.Silver))
			using (var outline = new Pen(Color.Silver))
			{
				drawOutline(g, white, outline, _boxA);
				drawOutline(g, white, outline, _boxXY);
				drawOutline(g, white, outline, _boxZ);
			}
			*/

			Func<PictureBox, Rectangle, Rectangle> addLoc = (ctrl, box)
			=> new Rectangle(
				   ctrl.Location.X + box.X,
				   ctrl.Location.Y + box.Y,
				   box.Width,
				   box.Height
			   );

			g.DrawImage(_slider, addLoc(_boxA, SliderRect(_boxA, 0)));
			g.DrawImage(_dot, addLoc(_boxXY, DotRect));
			g.DrawImage(_slider, addLoc(_boxZ, SliderRect(_boxZ, 3)));
		}

		private void moveLeftSlider(int x, int y)
		{
			SetAxis(0, y, _boxA.Size.Height);

			RefreshInputFields();

			_boxA.Invalidate();
			_owner.Invalidate();
		}
		private void moveDot(int x, int y)
		{
			SetAxis(1, x, _boxXY.Size.Height);
			SetAxis(2, y, _boxXY.Size.Width);

			RenderAlphaBar();
			RenderRightBar();

			RefreshInputFields();

			_boxXY.Invalidate();
			_owner.Invalidate();
		}
		private void moveRightSlider(int x, int y)
		{
			SetAxis(3, y, _boxZ.Size.Height);

			RenderAlphaBar();
			RenderMainBox();

			RefreshInputFields();

			_boxZ.Invalidate();
			_owner.Invalidate();
		}
	}

	// If this class is situated above the ColourPicker class in this file,
	//  the ColourPicker's resource manager fails to load the UI icons.
	public class ColourBox : PictureBox
	{
		private bool _mouseHeld;
		private Size _oldSize;

		private byte[] _pixbuf;
		private BitmapData _data;

		public Size OldSize { get { return _oldSize; } }
		public byte[] Buffer { get { return _pixbuf; } }

		public const int Border = 2;

		public ColourBox(ColourPicker.CursorHandler moveCursor)
		{
			_mouseHeld = false;
			_oldSize = new Size(0, 0);

			this.MouseDown += (s, e) => { _mouseHeld = true; moveCursor(e.X, e.Y); };
			this.MouseMove += (s, e) => { if (_mouseHeld) moveCursor(e.X, e.Y); };
			this.MouseUp += (s, e) => _mouseHeld = false;
		}

		public void Lock()
		{
			if (_oldSize != this.Size || this.Image == null)
			{
				this.Image = new Bitmap(this.Size.Width, this.Size.Height);
				_pixbuf = new byte[this.Size.Width * this.Size.Height * 4];
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

			Marshal.Copy(_pixbuf, 0, _data.Scan0, _pixbuf.Length);
			(this.Image as Bitmap).UnlockBits(_data);
			this.Invalidate();

			_oldSize = this.Size;
		}
	}
}
