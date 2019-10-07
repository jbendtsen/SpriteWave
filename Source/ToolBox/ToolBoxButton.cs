using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SpriteWave
{
	public class ToolBoxButton : Button
	{
		private readonly Pen _pen = new Pen(Color.Black);

		private PointF[][] _shapes;
		private Bitmap[] _imgs;

		private int _idx;
		public int State
		{
			set {
				_idx = value;
				this.Image = _imgs[_idx];
			}
		}

		public ToolBoxButton(PointF[][] fracPoints, Size size, int startIdx = 0)
		{
			_shapes = fracPoints;

			_imgs = new Bitmap[_shapes.Length];
			_idx = startIdx;

			this.BackColor = SystemColors.ControlLight;
			Initialise(size);
		}

		public ToolBoxButton(Point[][] pixPoints, Size size, int startIdx = 0)
		{
			_shapes = new PointF[pixPoints.Length][];
			Size client = Interior(size);

			int i = 0, j = 0;
			foreach (Point[] shp in pixPoints)
			{
				_shapes[i] = new PointF[shp.Length];
				foreach (Point p in shp)
					_shapes[i][j++] = new PointF(
						(float)p.X / (float)client.Width,
						(float)p.Y / (float)client.Height
					);

				j = 0;
				i++;
			}

			_imgs = new Bitmap[_shapes.Length];
			_idx = startIdx;

			this.BackColor = SystemColors.ControlLight;
			Initialise(size);
		}

		public ToolBoxButton(string text)
		{
			_shapes = null;

			this.Text = text;
			this.Location = new Point(0, 0);

			float width = (int)TextRenderer.MeasureText(text, this.Font).Width;
			Initialise(new Size((int)width + 10, 20));
		}

		private void Initialise(Size size)
		{
			this.FlatAppearance.BorderSize = 0;
			this.FlatStyle = FlatStyle.Flat;
			this.UseVisualStyleBackColor = false;
			this.SetStyle(ControlStyles.Selectable, false);

			this.Size = size; // Calls Render()
		}

		public void Render()
		{
			if (_shapes == null)
				return;

			Size client = Interior(this.Size);

			if (client.Width < 1 || client.Height < 1)
				return;

			for (int i = 0; i < _shapes.Length; i++)
				_imgs[i] = CreateShape(_shapes[i], client);

			this.Image = _imgs[_idx];
		}

		private static Size Interior(Size s)
		{
			return new Size(
				s.Width - 4,
				s.Height - 5
			);
		}

		private Bitmap CreateShape(PointF[] poly, Size area)
		{
			int w = area.Width;
			int h = area.Height;

			Point[] scaled = new Point[poly.Length];
			int i = 0;
			foreach (PointF p in poly)
				scaled[i++] = new Point(
					(int)(p.X * (float)w),
					(int)(p.Y * (float)h)
				);

			Bitmap canvas = new Bitmap(area.Width, area.Height, PixelFormat.Format32bppArgb);
			using (var g = Graphics.FromImage(canvas))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.DrawLines(_pen, scaled);
			}

			return canvas;
		}

		protected override void OnLayout(LayoutEventArgs e)
		{
			base.OnLayout(e);
			Render();
		}
	}

	public static class ToolBoxShapes
	{
		public static Point[][] Minimise = {
			new[] {
				new Point(12, 2),
				new Point(22, 2),
				new Point(22, 11),
				new Point(12, 11),
				new Point(12, 2)
			},
			new[] {
				new Point(12, 7),
				new Point(22, 7)
			}
		};

		public static PointF[][] Switch = {
			new[] {
				new PointF(0.05f, 0.25f),
				new PointF(0.85f, 0.50f),
				new PointF(0.05f, 0.75f)
			},
			new[] {
				new PointF(0.85f, 0.25f),
				new PointF(0.05f, 0.50f),
				new PointF(0.85f, 0.75f)
			}
		};
	}
}