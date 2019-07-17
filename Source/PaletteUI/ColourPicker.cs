using System;
using System.Drawing;
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
		private float[] _chn;
		private int[] _order;

		private float[] _alphaShades;

		private Bitmap _slider;
		private Bitmap _dot;

		private Control _owner;
		private bool _mouseDownBoxA;
		private bool _mouseDownBoxXY;
		private bool _mouseDownBoxZ;

		private PictureBox _boxA;
		private PictureBox _boxXY;
		private PictureBox _boxZ;

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

			_boxA = new PictureBox();
			_boxA.Name = "alphaBox";
			_boxA.Location = new Point(20, 20);
			_boxA.Size = new Size(20, 200);
			_boxA.Paint += this.paintLeftSlider;
			_boxA.MouseDown += this.moveLeftSlider;
			_boxA.MouseMove += (s, e) => { if (_mouseDownBoxA) moveLeftSlider(s, e); };
			_boxA.MouseUp += (s, e) => _mouseDownBoxA = false;

			_boxXY = new PictureBox();
			_boxXY.Name = "xyBox";
			_boxXY.Location = new Point(60, 20);
			_boxXY.Size = new Size(200, 200);
			_boxXY.Paint += this.paintDot;
			_boxXY.MouseDown += this.moveDot;
			_boxXY.MouseMove += (s, e) => { if (_mouseDownBoxXY) moveDot(s, e); };
			_boxXY.MouseUp += (s, e) => _mouseDownBoxXY = false;

			_boxZ = new PictureBox();
			_boxZ.Name = "zBox";
			_boxZ.Location = new Point(280, 20);
			_boxZ.Size = new Size(20, 200);
			_boxZ.Paint += this.paintRightSlider;
			_boxZ.MouseDown += this.moveRightSlider;
			_boxZ.MouseMove += (s, e) => { if (_mouseDownBoxZ) moveRightSlider(s, e); };
			_boxZ.MouseUp += (s, e) => _mouseDownBoxZ = false;

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

		public void Cycle()
		{
			Action<int[], int> tick = (arr, idx) => arr[idx] = (arr[idx] + 1) % 3;
			tick(_order, 1);
			tick(_order, 2);
			tick(_order, 3);
		}

		public void Render(Size sizeA, Size sizeXY, Size sizeZ)
		{
			// Hopefully this speeds things up?
			// Not sure if this would eliminate any potential JIT optimisations to do with memory caching...
			int pos0 = _order[0];
			int pos1 = _order[1];
			int pos2 = _order[2];
			int pos3 = _order[3];

			// Render the alpha bar
			int aW = sizeA.Width;
			int aH = sizeA.Height;
			var aBuf = new byte[aW * aH * 4];

			int blockSize = (aW + 1) / 2;
			for (int i = 0; i < aBuf.Length; i += 4)
			{
				int xOdd = (((i / 4) % aW) / blockSize) % 2;
				int yOdd = (((i / 4) / aW) / blockSize) % 2;
				float shade = _alphaShades[xOdd ^ yOdd];

				float y = (float)((i / 4) / aW) / (float)aH;

				aBuf[i + pos0] = 255;
				aBuf[i + pos1] = (byte)((shade + (_chn[pos1] - shade) * y) * 255f);
				aBuf[i + pos2] = (byte)((shade + (_chn[pos2] - shade) * y) * 255f);
				aBuf[i + pos3] = (byte)((shade + (_chn[pos3] - shade) * y) * 255f);
			}

			byte axis0 = (byte)(_chn[pos0] * 255f);
			byte axis1 = (byte)(_chn[pos1] * 255f);
			byte axis2 = (byte)(_chn[pos2] * 255f);
			byte axis3 = (byte)(_chn[pos3] * 255f);

			// Render the main colour box
			int xyW = sizeXY.Width;
			int xyH = sizeXY.Height;
			var xyBuf = new byte[xyW * xyH * 4];

			for (int i = 0; i < xyBuf.Length; i += 4)
			{
				float x = (float)((i / 4) % xyW) / (float)xyW;
				float y = (float)((i / 4) / xyW) / (float)xyH;
				x = 1f - x;

				xyBuf[i + pos0] = 255; // axis0
				xyBuf[i + pos1] = (byte)(x * 255f);
				xyBuf[i + pos2] = (byte)(y * 255f);
				xyBuf[i + pos3] = axis3;
			}

			// Render the bar for the third colour channel
			int zW = sizeZ.Width;
			int zH = sizeZ.Height;
			var zBuf = new byte[zW * zH * 4];

			for (int i = 0; i < zBuf.Length; i += 4)
			{
				float y = (float)((i / 4) / zW) / (float)zH;

				zBuf[i + pos0] = 255; // axis0
				zBuf[i + pos1] = axis1;
				zBuf[i + pos2] = axis2;
				zBuf[i + pos3] = (byte)(y * 255f);
			}

			_boxA.Image = Utils.BitmapFrom(aBuf, aW, aH);
			_boxXY.Image = Utils.BitmapFrom(xyBuf, xyW, xyH);
			_boxZ.Image = Utils.BitmapFrom(zBuf, zW, zH);
		}

		public void Render()
		{
			Render(_boxA.Size, _boxXY.Size, _boxZ.Size);
			_owner.Invalidate();
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

			Func<PictureBox, Rectangle, Rectangle> addLoc = (ctrl, box)
			=> new Rectangle(
				   ctrl.Location.X + box.X,
				   ctrl.Location.Y + box.Y,
				   box.Width,
				   box.Height
			   );

			g.DrawImage(_slider, addLoc(_boxA, SliderRect(_boxA, 0)));
			g.DrawImage(_dot, addLoc(_boxXY, this.DotRect));
			g.DrawImage(_slider, addLoc(_boxZ, SliderRect(_boxZ, 3)));
		}

		private void moveLeftSlider(object sender, MouseEventArgs e)
		{
			_mouseDownBoxA = true;
			SetAxis(0, e.Y, _boxA.Size.Height);
			Render();
		}
		private void moveDot(object sender, MouseEventArgs e)
		{
			_mouseDownBoxXY = true;
			SetAxis(1, e.X, _boxXY.Size.Height);
			SetAxis(2, e.Y, _boxXY.Size.Width);
			Render();
		}
		private void moveRightSlider(object sender, MouseEventArgs e)
		{
			_mouseDownBoxZ = true;
			SetAxis(3, e.Y, _boxZ.Size.Height);
			Render();
		}
	}
}
