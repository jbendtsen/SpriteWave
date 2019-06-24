using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	//public enum Edge { Nil, Top, Bottom, Left, Right }

	public class DragPoint : ISelection
	{
		private Brush _hl;
		private IPiece _obj;
		private TileWindow _wnd;
		private Position _loc;

		public IPiece Piece { get { return _obj; } }
		public Position Location { get { return _loc; } set { throw new NotImplementedException(); /*_loc = value;*/ } }
		public bool IsActive { get { return _wnd.IsActive; } }

		public DragPoint(Brush hl, IPiece selObj, TileWindow wnd, Position loc)
		{
			_hl = hl;
			_obj = selObj;
			_wnd = wnd;
			_loc = loc;
		}

		public void Receive(IPiece isel)
		{
			_wnd.Receive(isel);
		}

		public void DrawSelection(Graphics g)
		{
			Utils.DrawSelection(g, _wnd, _hl, _obj, _loc);
		}

		public void Delete() {}
	}

	public class DragObject
	{
		private bool _escaped;
		public bool Started { get { return _escaped; } }

		private const float curScale = 4f;
		private const float curAlpha = 0.6f;
		private Cursor _cur;

		private Position _orgPos;
		private TileWindow _orgWnd;

		private int _lastX, _lastY;
		private TileWindow _lastWnd;
		public TileWindow Window { get { return _lastWnd; } }

		private Brush _hl;
		private IPiece _selObj;
		public bool IsEdge { get { return _selObj is Edge; } }

		public ISelection Current()
		{
			if (_lastWnd == null)
				return null;

			Position p;
			try {
				bool allowOob = _selObj is Edge;
				p = _lastWnd.GetPosition(_lastX, _lastY, allowOob);
			}
			catch {
				return null;
			}

			Edge e = _selObj as Edge;
			if (e != null)
			{
				if (_lastWnd == _orgWnd)
					e.Distance = new Position(
						p.col - _orgPos.col,
						p.row - _orgPos.row
					);
				else
					e.Distance = new Position(0, 0);
			}

			return new DragPoint(_hl, _selObj, _lastWnd, p);
		}

		public DragObject(TileWindow wnd, int x, int y)
		{
			_lastX = x;
			_lastY = y;

			_lastWnd = wnd;
			_orgWnd = wnd;

			_orgPos = _orgWnd.GetPosition(x, y);

			_selObj = _orgWnd.PieceAt(_orgPos);
			if (_selObj is Tile)
			{
				Bitmap img = _orgWnd.TileBitmap(_selObj as Tile);
				img = img.SetAlpha(curAlpha).Scale(curScale);
				_cur = new Cursor(img.GetHicon());
			}

			_escaped = false;
			_hl = new SolidBrush(Color.FromArgb(96, 0, 255, 64));

			_orgWnd.Selection = new DragPoint(_hl, _selObj, _orgWnd, _orgPos);
			Transfer.Source = _orgWnd.Selection;
		}

		public ISelection Cancel()
		{
			_orgWnd.Selection = _orgWnd;
			_orgWnd.Location = _orgPos;
			return _orgWnd;
		}

		private void Escape()
		{
			_escaped = true;
			if (_cur != null)
				Cursor.Current = _cur;

			Transfer.Start();
		}

		private bool HasLeft(TileWindow wnd)
		{
			if (wnd == null || wnd != _orgWnd)
				return true;

			if (_selObj is Edge)
			{
				var dist = ((Edge)_selObj).Distance;
				return (dist.col != 0 || dist.row != 0);
			}

			try {
				Position loc = wnd.GetPosition(_lastX, _lastY);
				return (loc.col != _orgPos.col || loc.row != _orgPos.row);
			}
			catch (ArgumentOutOfRangeException) {}

			return false;
		}

		public ISelection Update(TileWindow wnd, int x, int y)
		{
			_lastX = x;
			_lastY = y;

			if (!_escaped && this.HasLeft(wnd))
				Escape();

			if (_selObj is Edge)
				wnd = _orgWnd;

			if (wnd != _lastWnd && _lastWnd != null)
				_lastWnd.Selection = null;

			_lastWnd = wnd;

			ISelection isel = this.Current();
			if (_lastWnd != null)
				_lastWnd.Selection = isel;

			return isel;
		}
	}
}
