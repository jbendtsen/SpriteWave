using System;
using System.Runtime.CompilerServices;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace SpriteWave
{
	public enum BGRA
	{
		Blue, Green, Red, Alpha
	};

	public enum ColorMode
	{
		RGB, HSV, Invalid
	};

	public class ColorPicker : Panel
	{
		public delegate void CursorHandler(int x, int y);
		public delegate void ScrollHandler(int delta);

		private const int nChans = 4;
		private const float dimmed = 0.8f;
		private const float scrollUnit = 40f;

		private float[] _chn; // ordered by BGRA
		private int[] _order;
		private ColorMode _mode;

		private readonly string[] _labels;
		private readonly float[] _alphaShades = {0.6f, 0.8f};

		private Bitmap _slider;
		private Bitmap _dot;
		private Bitmap[] _modeImg;

		private ColorBox _boxA;
		private ColorBox _boxXY;
		private ColorBox _boxZ;
		private ColorBox _boxSample;

		private Label[] _axisLabel;

		private Label[] _chnLabel;
		private TextBox[] _chnBox;
		private bool _allowTextEvent = true;

		private Button _cycle;
		private Button _switchMode;

		public Color Color
		{
			get {
				byte b = (byte)(_chn[0] * 255f);
				byte g = (byte)(_chn[1] * 255f);
				byte r = (byte)(_chn[2] * 255f);
				return Color.FromArgb(255, r, g, b);
			}
		}

		public Rectangle DotRect
		{
			get {
				int dotX = (int)((1f - _chn[_order[1]]) * (float)_boxXY.Width);
				int dotY = (int)((1f - _chn[_order[2]]) * (float)_boxXY.Height);
				int off = _dot.Width / -2;

				return new Rectangle(
					dotX + off,
					dotY + off,
					_dot.Width,
					_dot.Height
				);
			}
		}

		public Rectangle SliderRect(PictureBox ctrl, int idx)
		{
			int h = (int)((1f - _chn[_order[idx]]) * (float)ctrl.Height);
			int off = (ctrl.Width - _slider.Width) / 2;

			return new Rectangle(
				off,
				h + off,
				_slider.Width,
				_slider.Height
			);
		}

		public ColorPicker(int boxSize)
		{
			_mode = ColorMode.RGB;

			_chn = new[] {
				1f, 1f, 1f, 1f
			};

			_order = new[] {
				(int)BGRA.Alpha,
				(int)BGRA.Red,
				(int)BGRA.Green,
				(int)BGRA.Blue
			};

			_labels = new string[nChans];
			_labels[(int)BGRA.Red] = "R";
			_labels[(int)BGRA.Green] = "G";
			_labels[(int)BGRA.Blue] = "B";
			_labels[(int)BGRA.Alpha] = "A";

			this.Name = "colorPicker";
			this.Location = new Point(0, 0);
			this.Size = new Size(200 + boxSize, 40 + boxSize);

			var resources = new ComponentResourceManager(typeof(ColorPicker));
			_slider = (Bitmap)(resources.GetObject("slider"));
			_dot = (Bitmap)(resources.GetObject("dot"));

			_modeImg = new Bitmap[2];
			_modeImg[(int)ColorMode.RGB] = (Bitmap)(resources.GetObject("rgb"));
			_modeImg[(int)ColorMode.HSV] = (Bitmap)(resources.GetObject("hsv"));

			_boxA = new ColorBox(this.moveLeftSlider, this.scrollLeftSlider);
			_boxA.Name = "alphaBox";
			_boxA.Location = new Point(20, 20);
			_boxA.Size = new Size(20, boxSize);
			_boxA.Paint += this.paintLeftSlider;

			_boxXY = new ColorBox(this.moveDot, null);
			_boxXY.Name = "xyBox";
			_boxXY.Location = new Point(60, 20);
			_boxXY.Size = new Size(boxSize, boxSize);
			_boxXY.Paint += this.paintDot;

			_boxZ = new ColorBox(this.moveRightSlider, this.scrollRightSlider);
			_boxZ.Name = "zBox";
			_boxZ.Location = new Point(80 + boxSize, 20);
			_boxZ.Size = new Size(20, boxSize);
			_boxZ.Paint += this.paintRightSlider;

			_boxSample = new ColorBox(null, null);
			_boxSample.Name = "sampleBox";
			_boxSample.Location = new Point(114 + boxSize, 20 + boxSize - 72);
			_boxSample.Size = new Size(72, 72);

			_cycle = new Button();
			_cycle.Name = "cycleChans";
			_cycle.Location = new Point(114 + boxSize, 20);
			_cycle.Size = new Size(32, 32);
			_cycle.Image = (Bitmap)resources.GetObject("cycle");
			_cycle.Click += (s, e) => Cycle();

			_switchMode = new Button();
			_switchMode.Name = "switchMode";
			_switchMode.Location = new Point(152 + boxSize, 20);
			_switchMode.Size = new Size(32, 32);
			_switchMode.Image = _modeImg[(int)ColorMode.HSV];
			_switchMode.Click += (s, e) => SwitchMode();

			_axisLabel = new Label[nChans];
			for (int i = 0; i < nChans; i++)
			{
				_axisLabel[i] = new Label();
				_axisLabel[i].Name = "axisLabel" + i;
				_axisLabel[i].Font = new Font(Control.DefaultFont, FontStyle.Bold);
				_axisLabel[i].Size = new Size(15, 20);
				this.Controls.Add(_axisLabel[i]);
			}

			int midY = 13 + (boxSize / 2);
			_axisLabel[0].Location = new Point(4, midY);
			_axisLabel[1].Location = new Point(52 + (boxSize / 2), 24 + boxSize);
			_axisLabel[2].Location = new Point(44, midY);
			_axisLabel[3].Location = new Point(64 + boxSize, midY);

			_chnLabel = new Label[nChans];
			for (int i = 0; i < nChans; i++)
			{
				_chnLabel[i] = new Label();
				_chnLabel[i].Name = "chnLabel" + i;
				_chnLabel[i].Font = new Font(Control.DefaultFont, FontStyle.Bold);
				_chnLabel[i].Location = new Point(114 + boxSize, 67 + i * 32);
				_chnLabel[i].Size = new Size(15, 20);
				this.Controls.Add(_chnLabel[i]);
			}

			_chnBox = new TextBox[nChans];
			for (int i = 0; i < nChans; i++)
			{
				_chnBox[i] = new TextBox();
				_chnBox[i].Name = "chnBox" + i;
				_chnBox[i].Location = new Point(138 + boxSize, 64 + i * 32);
				_chnBox[i].Size = new Size(45, 20);
				_chnBox[i].TextChanged += this.updateField;
				this.Controls.Add(_chnBox[i]);
			}

			RefreshInputFields();

			this.Controls.Add(_boxA);
			this.Controls.Add(_boxXY);
			this.Controls.Add(_boxZ);
			this.Controls.Add(_boxSample);

			this.Controls.Add(_cycle);
			this.Controls.Add(_switchMode);
		}

		public void RefreshInputFields()
		{
			_allowTextEvent = false;
			for (int i = 0; i < nChans; i++)
			{
				_axisLabel[i].Text = _labels[_order[i]];
				_chnLabel[3-i].Text = _labels[_order[i]];
				_chnBox[3-i].Text = Math.Floor(_chn[_order[i]] * 255f).ToString();
			}
			_allowTextEvent = true;
		}

		public void SetAxis(int idx, float f)
		{
			_chn[_order[idx]] = Math.Min(Math.Max(f, 0), 1);
		}
		public void SetAxis(int idx, int num, int denom)
		{
			SetAxis(idx, 1f - ((float)num / (float)denom));
		}

		public void updateField(object sender, EventArgs e)
		{
			if (!_allowTextEvent)
				return;

			string name = (sender as Control).Name;
			if (name.Length != 7 || name.Substring(0, 6) != "chnBox")
				return;

			int idx = name[6] - '0';
			if (idx < 0 || idx > 3)
				return;

			float f = 0f;
			try {
				int n = Convert.ToInt32(_chnBox[idx].Text);
				if (n < 0 || n > 255)
					throw new InvalidOperationException();

				f = (float)n / 255f;
			}
			catch (Exception ex) {
				if (ex is FormatException ||
					ex is OverflowException ||
					ex is InvalidOperationException
				)
					return;

				throw;
			}

			_chn[_order[3 - idx]] = f;
			Render();
		}

		public void Cycle()
		{
			Action<int[], int> tick = (arr, idx) => arr[idx] = (arr[idx] + 1) % 3;
			tick(_order, 1);
			tick(_order, 2);
			tick(_order, 3);

			RefreshInputFields();
			Render();
		}

		public void SwitchMode()
		{
			Func<ColorMode, ColorMode> cycleMode = (m) =>
			{
				m++;
				if (m == ColorMode.Invalid)
					m = 0;

				return m;
			};

			_mode = cycleMode(_mode);
			_switchMode.Image = _modeImg[(int)cycleMode(_mode)];
		}

		public void Render()
		{
			RenderAlphaBar();
			RenderMainBox();
			RenderRightBar();
			RenderSampleBox();
			this.Invalidate();
		}

		private struct AlphaPixel
		{
			public int blockSize;

			public float[] shades;

			public float blue;
			public float green;
			public float red;
			public float alpha;

			public AlphaPixel(int size, float[] sh, float b, float g, float r, float a)
			{
				blockSize = size;
				shades = new[] { sh[0], sh[1] };
				blue = b;
				green = g;
				red = r;
				alpha = a;
			}
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void RenderAlphaPixel(AlphaPixel info, byte[] buf, int w, int h, int idx)
		{
			const int frame = ColorBox.Border;
			int x = idx % w;
			int y = idx / w;

			int xOdd = (x / info.blockSize) % 2;
			int yOdd = (y / info.blockSize) % 2;
			float shade = info.shades[xOdd ^ yOdd];

			float lum = 255f;
			if (x < frame || x >= w - frame || y < frame || y >= h - frame)
				lum = 255f * dimmed;

			int i = idx * 4;
			buf[i + (int)BGRA.Blue] = (byte)((shade + (info.blue - shade) * info.alpha) * lum);
			buf[i + (int)BGRA.Green] = (byte)((shade + (info.green - shade) * info.alpha) * lum);
			buf[i + (int)BGRA.Red] = (byte)((shade + (info.red - shade) * info.alpha) * lum);
			buf[i + (int)BGRA.Alpha] = 255;
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void RenderOpaquePixel(float b, float g, float r, byte[] buf, int w, int h, int idx)
		{
			const int frame = ColorBox.Border;
			int x = idx % w;
			int y = idx / w;

			if (x < frame || x >= w - frame || y < frame || y >= h - frame)
			{
				b *= dimmed;
				g *= dimmed;
				r *= dimmed;
			}

			int i = idx * 4;
			buf[i + (int)BGRA.Blue] = (byte)(b * 255f);
			buf[i + (int)BGRA.Green] = (byte)(g * 255f);
			buf[i + (int)BGRA.Red] = (byte)(r * 255f);
			buf[i + (int)BGRA.Alpha] = 255; // axis0
		}

		private void RenderSampleBox()
		{
			_boxSample.Lock();

			var buf = _boxSample.Buffer;
			int w = _boxSample.Width;
			int h = _boxSample.Height;

			var info = new AlphaPixel(
				w / 8,
				_alphaShades,
				_chn[0],
				_chn[1],
				_chn[2],
				_chn[3]
			);

			for (int idx = 0; idx < buf.Length / 4; idx++)
				RenderAlphaPixel(info, buf, w, h, idx);

			_boxSample.Unlock();
		}

		private void RenderAlphaBar()
		{
			_boxA.Lock();

			var buf = _boxA.Buffer;
			int w = _boxA.Width;
			int h = _boxA.Height;

			var info = new AlphaPixel(
				(w + 1) / 2,
				_alphaShades,
				_chn[0],
				_chn[1],
				_chn[2],
				1f
			);

			for (int i = 0; i < buf.Length; i += 4)
			{
				int idx = i / 4;
				info.alpha = 1f - (float)(idx / w) / (float)h;
				RenderAlphaPixel(info, buf, w, h, idx);
			}

			_boxA.Unlock();
		}

		private void RenderMainBox()
		{
			_boxXY.Lock();

			var buf = _boxXY.Buffer;
			int w = _boxXY.Width;
			int h = _boxXY.Height;

			float[] axes = new float[3];
			int a1 = _order[1];
			int a2 = _order[2];
			axes[_order[3]] = _chn[_order[3]];

			for (int i = 0; i < buf.Length; i += 4)
			{
				axes[a1] = 1f - (float)((i / 4) % w) / (float)w;
				axes[a2] = 1f - (float)((i / 4) / w) / (float)h;

				RenderOpaquePixel(axes[0], axes[1], axes[2], buf, w, h, i / 4);
			}

			_boxXY.Unlock();
		}

		private void RenderRightBar()
		{
			_boxZ.Lock();

			var buf = _boxZ.Buffer;
			int w = _boxZ.Width;
			int h = _boxZ.Height;

			float[] axes = new float[3];
			axes[_order[1]] = _chn[_order[1]];
			axes[_order[2]] = _chn[_order[2]];
			int a3 = _order[3];

			for (int i = 0; i < buf.Length; i += 4)
			{
				axes[a3] = 1f - (float)((i / 4) / w) / (float)h;

				RenderOpaquePixel(axes[0], axes[1], axes[2], buf, w, h, i / 4);
			}

			_boxZ.Unlock();
		}

		private void RefreshLeftSlider()
		{
			RenderSampleBox();
			RefreshInputFields();

			_boxA.Invalidate();
			this.Invalidate();
		}
		private void RefreshDot()
		{
			RenderAlphaBar();
			RenderRightBar();
			RenderSampleBox();

			RefreshInputFields();

			_boxXY.Invalidate();
			this.Invalidate();
		}
		private void RefreshRightSlider()
		{
			RenderAlphaBar();
			RenderMainBox();
			RenderSampleBox();

			RefreshInputFields();

			_boxZ.Invalidate();
			this.Invalidate();
		}

		private void moveLeftSlider(int x, int y)
		{
			SetAxis(0, y, _boxA.Height);
			RefreshLeftSlider();
		}
		private void moveDot(int x, int y)
		{
			SetAxis(1, x, _boxXY.Height);
			SetAxis(2, y, _boxXY.Width);
			RefreshDot();
		}
		private void moveRightSlider(int x, int y)
		{
			SetAxis(3, y, _boxZ.Height);
			RefreshRightSlider();
		}

		private void scrollLeftSlider(int amount)
		{
			float delta = (float)amount / scrollUnit;
			SetAxis(0, _chn[_order[0]] + delta);
			RefreshLeftSlider();
		}
		private void scrollRightSlider(int amount)
		{
			float delta = (float)amount / scrollUnit;
			SetAxis(3, _chn[_order[3]] + delta);
			RefreshRightSlider();
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

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			PaintUnderUI(e.Graphics);
		}
	}
}
