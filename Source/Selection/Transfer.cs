using System;

namespace SpriteWave
{
	public static class Transfer
	{
		private static Selection _src, _dst;
		public static Selection Source { get { return _src; } set { _src = value; } }
		public static Selection Dest { get { return _dst; } set { _dst = value; } }

		private static IPiece _obj;
		public static bool HasPiece { get { return _obj != null; } }

		public static void Start()
		{
			if (_src == null)
				return;

			Tile t = _src.Piece as Tile;
			if (t != null)
				_obj = t.Clone();
			else
				_obj = _src.Piece;
		}

		public static void Clear()
		{
			_src = null;
			_dst = null;
			_obj = null;
		}

		public static void Paste()
		{
			if (_obj == null || _src == null || _dst == null || _dst.Window == null)
				return;

			TileWindow wnd = _dst.Window;
			Edge e = _obj as Edge;
			if (e != null)
				_dst.Window.ResizeCollage(e);
			else
				_dst.Window.ReceiveTile(_obj as Tile);
		}

		public static void Swap()
		{
			Tile cur = _obj as Tile;
			if (cur == null || _src == null || _dst == null)
				return;

			Tile other = _dst.Piece as Tile;
			if (other == null)
				return;

			TileWindow recver = _dst.Window;
			TileWindow sender = _src.Window;
			if (recver == null || sender == null)
				return;

			sender.ReceiveTile(other.Clone() as Tile, _src.Location);
			recver.ReceiveTile(cur, _dst.Location);
		}
	}
}
