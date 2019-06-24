using System;

namespace SpriteWave
{
	public static class Transfer
	{
		private static ISelection _src, _dst;
		public static ISelection Source { get { return _src; } set { if (value.IsActive) _src = value; } }
		public static ISelection Dest { get { return _dst; } set { _dst = value; } }

		private static IPiece _obj;
		public static bool HasPiece { get { return _obj != null; } }

		public static void Start()
		{
			if (_src == null)
				return;

			if (_src.Piece is Edge)
				_obj = _src.Piece;
			else
				_obj = _src.Piece.Clone();
		}

		public static void Clear()
		{
			_src = null;
			_dst = null;
			_obj = null;
		}

		public static void Paste()
		{
			if (_obj == null || _src == null || _dst == null)
				return;

			_dst.Receive(_obj);
		}
		/*
		public static void Swap()
		{
			if (_obj == null || _src == null || _dst == null)
				return;

			_src.Receive(_dst.Piece.Clone());
			_dst.Receive(_obj);
		}
		*/
	}
}
