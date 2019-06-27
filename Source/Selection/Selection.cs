using System;
using System.Drawing;

namespace SpriteWave
{
	public interface IPiece
	{
		EdgeKind EdgeKind { get; }
	}

	public class Selection
	{
		protected TileWindow _wnd;
		public TileWindow Window { get { return _wnd; } }

		protected Position _loc;
		public Position Location { get { return _loc; } }

		protected IPiece _obj;
		public IPiece Piece { get { return _obj; } }

		public Selection(IPiece selObj, TileWindow wnd, Position loc)
		{
			_obj = selObj;
			_wnd = wnd;
			_loc = loc;
		}
	}
}