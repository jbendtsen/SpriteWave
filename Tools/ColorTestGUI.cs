using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	class Display : Form
	{
		const int nTries = 10;

		Brush[][] _brushes;

		protected override void Dispose(bool disposing)
		{
			DisposeBrushes();
			base.Dispose(disposing);
		}

		public Display()
		{
			GenerateBrushes();

			this.Text = "RGB To HSL Accuracy Test";
			this.Size = new Size(300, 600);
			this.MinimumSize = new Size(10, 40);
			this.ResizeRedraw = true;
			this.KeyDown += (s, e) => { GenerateBrushes(); this.Invalidate(); };
		}

		static Vec3[] GenerateVec3List(int length)
		{
			Vec3[] list = new Vec3[length];
			Random rng = new Random();
			for (int i = 0; i < length; i++)
				list[i] = new Vec3((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble());

			return list;
		}

		static Vec3[] GenerateHSLList(int length)
		{
			Vec3[] list = new Vec3[length];
			Random rng = new Random();
			for (int i = 0; i < length; i++)
				list[i] = ColorSpace.HSLtoRGB(new Vec3((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble()));

			return list;
		}

		static Vec3[] RGBtoRGB(Vec3[] input)
		{
			Vec3[] output = new Vec3[input.Length];
			for (int i = 0; i < input.Length; i++)
				output[i] = ColorSpace.HSLtoRGB(ColorSpace.RGBtoHSL(input[i]));

			return output;
		}

		void GenerateBrushes()
		{
			if (_brushes != null)
				DisposeBrushes();

			Vec3[][] colors = new Vec3[4][];

			colors[0] = GenerateVec3List(nTries);
			colors[1] = RGBtoRGB(colors[0]);

			colors[2] = GenerateHSLList(nTries);
			colors[3] = RGBtoRGB(colors[2]);

			_brushes = new Brush[4][];

			for (int i = 0; i < 4; i++)
			{
				_brushes[i] = new Brush[colors[i].Length];

				for (int j = 0; j < colors[i].Length; j++)
				{
					Vec3 rgb = colors[i][j];
					Color color = Color.FromArgb(
						255,
						(int)(rgb.X * 255f),
						(int)(rgb.Y * 255f),
						(int)(rgb.Z * 255f)
					);
					_brushes[i][j] = new SolidBrush(color);
				}
			}
		}

		void DisposeBrushes()
		{
			foreach (Brush[] col in _brushes)
			{
				foreach (Brush cell in col)
					cell.Dispose();
			}
			_brushes = null;
		}

		void DrawColorAt(int col, int row, int w, int h, Graphics g)
		{
			int x = col * w;
			int y = row * h;
			var rect = new Rectangle(x, y, w, h);
			g.FillRectangle(_brushes[col][row], rect);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			int w = this.ClientSize.Width;
			int h = this.ClientSize.Height;

			for (int i = 0; i < 4; i++)
			{
				int len = _brushes[i].Length;
				for (int j = 0; j < len; j++)
				{
					DrawColorAt(i, j, w/4, h/len, e.Graphics);
				}
			}
		}
	}

	class MainClass
	{
		static void Main()
		{
			Application.Run(new Display());
		}
	}
}