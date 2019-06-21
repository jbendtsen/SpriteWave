using System;
using System.Drawing;

namespace SpriteWave
{
	public interface IPiece
	{
		IPiece Clone();
	}

	public interface ISelection
	{
		void Receive(IPiece isel);
		void DrawSelection(TileWindow wnd, Graphics g);
		void Delete();

		ISelection Selection { get; set; }
		Position Location { get; set; }
		IPiece Piece { get; }

		//void Move(int dCol, int dRow);
		//void SetPos(int )
	}

	public static class Transfer
	{
		private static ISelection _src, _dst;
		public static ISelection Source { get { return _src; } set { Set(ref _src, value); } }
		public static ISelection Dest { get { return _dst; } set { Set(ref _dst, value); } }

		private static IPiece _obj;
		public static bool HasPiece { get { return _obj != null; } }

		private static void Set(ref ISelection point, ISelection isel)
		{
			if (isel == null)
			{
				if (point != null)
					point.Selection = null;

				point = null;
			}
			else
			{
				point = isel;
				point.Selection = isel;
			}
		}

		public static void Start()
		{
			_obj = _src.Piece.Clone();
		}

		public static void Clear()
		{
			Source = null;
			Dest = null;
			_obj = null;
		}

		public static void Paste()
		{
			if (_obj == null || _src == null || _dst == null)
				return;

			_dst.Receive(_obj);
		}

		public static void Swap()
		{
			if (_obj == null || _src == null || _dst == null)
				return;

			_src.Receive(_dst.Piece.Clone());
			_dst.Receive(_obj);
		}
	}
}
