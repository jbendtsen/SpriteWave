using System;

namespace SpriteWave
{
	public static class Transfer
	{
		public static ISelection Source, Dest;

		private static IPiece _obj;
		public static bool HasPiece { get { return _obj != null; } }

		public static void Start()
		{
			if (Source == null)
				return;

			if (Source.Piece is Edge)
				_obj = Source.Piece;
			else
				_obj = Source.Piece.Clone();
		}

		public static void Clear()
		{
			Source = null;
			Dest = null;
			_obj = null;
		}

		public static void Paste()
		{
			if (_obj == null || Source == null || Dest == null)
				return;

			Dest.Receive(_obj);
		}
/*
		public static void Swap()
		{
			if (_obj == null || Source == null || Dest == null)
				return;

			Source.Receive(Dest.Piece.Clone());
			Dest.Receive(_obj);
		}
*/
	}
}
