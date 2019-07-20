using System;

namespace SpriteWave
{
	public interface IPiece
	{
		EdgeKind EdgeKind { get; }
	}

	public class Selection : IEquatable<Selection>
	{
		protected TileWindow _wnd;
		public TileWindow Window { get { return _wnd; } }

		protected Position _loc;
		public Position Location { get { return _loc; } }

		protected IPiece _piece;
		public IPiece Piece { get { return _piece; } }

		public Selection(IPiece selObj, TileWindow wnd, Position loc)
		{
			_piece = selObj;
			_wnd = wnd;
			_loc = loc;
		}

		public bool Equals(Selection obj)
		{
			var sel = obj as Selection;
			if (sel == null)
				return true;

			return
				sel.Window == _wnd &&
				sel.Location.col == _loc.col &&
				sel.Location.row == _loc.row
			;
		}
	}
}