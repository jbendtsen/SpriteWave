using System;
using System.Drawing;

namespace SpriteWave
{
	public enum EdgeKind
	{
		TL,
		Top,
		TR,
		Left,
		None,
		Right,
		BL,
		Bottom,
		BR
	}

	public class Edge : IPiece
	{
		private EdgeKind _kind;
		public EdgeKind EdgeKind { get { return _kind; } }

		private PointF[] _shape;
		public PointF[] Shape { get { return _shape; } }

		private Position _dist;
		public Position Distance
		{
			get {
				return _dist;
			}
			set {
				_dist = value;

				int x, y;
				GetCoords(_kind, out x, out y);
				if (x == 0)
					_dist.col = 0;
				if (y == 0)
					_dist.row = 0;
			}
		}

		public Edge(EdgeKind kind)
		{
			_kind = kind;
			_dist = new Position(0, 0);
		}

		public static EdgeKind Direction(int x, int y)
		{
			if (x < -1 || x > 1 || y < -1 || y > 1)
				return EdgeKind.None;

			return (EdgeKind)((y + 1) * 3 + (x + 1));
		}

		public static void GetCoords(EdgeKind e, out int x, out int y)
		{
			x = ((int)e % 3) - 1;
			y = ((int)e / 3) - 1;
		}

		private PointF[] ReflectTriangle(PointF[] input, Collage cl, bool rfX, bool rfY)
		{
			int clW = cl.Width;
			int clH = cl.Height;
			int offW = _dist.col * cl.TileW;
			int offH = _dist.row * cl.TileH;

			var output = new PointF[3];
			for (int i = 0; i < 3; i++)
			{
				if (rfX)
					output[i].X = offW + clW - input[i].X;
				else
					output[i].X = offW + input[i].X;

				if (rfY)
					output[i].Y = offH + clH - input[i].Y;
				else
					output[i].Y = offH + input[i].Y;
			}

			return output;
		}

		public void Render(Collage cl)
		{
			if (_kind == EdgeKind.None)
				return;

			float clWidth = cl.Width;
			float clHeight = cl.Height;

			float taperW = (float)cl.TileW / 2f;
			float taperH = (float)cl.TileH / 2f;
			float gapW = taperW / 2f;
			float gapH = taperH / 2f;
			float fitW = gapW / 2f;
			float fitH = gapH / 2f;

			int x, y;
			GetCoords(_kind, out x, out y);

			// Corner Edge
			if (x != 0 && y != 0)
			{
				_shape = new[] {
					new PointF(-taperW, fitH),
					new PointF(fitW, -taperH),
					new PointF(-taperW, -taperH)
				};
			}
			// Vertical Edge
			else if (x != 0)
			{
				_shape = new[] {
					new PointF(-gapW, taperH),
					new PointF(-gapW, clHeight - taperH),
					new PointF(-gapW - taperW, clHeight / 2f)
				};
			}
			// Horizontal Edge
			else
			{
				_shape = new[] {
					new PointF(taperW, -gapH),
					new PointF(clWidth - taperW, -gapH),
					new PointF(clWidth / 2f, -taperH - gapH)
				};
			}

			_shape = ReflectTriangle(_shape, cl, x == 1, y == 1);
		}
	}
}
