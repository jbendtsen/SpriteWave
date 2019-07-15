using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public class DragObject
	{
		private bool _escaped = false;
		public bool Started { get { return _escaped; } }

		private const float curScale = 4f;
		private const float curAlpha = 0.6f;
		private Cursor _cur;

		private Position _orgPos;
		private TileWindow _orgWnd;

		private int _lastX, _lastY;
		private TileWindow _lastWnd;
		public TileWindow Window { get { return _lastWnd; } }

		private IPiece _selObj;
		public bool IsEdge { get { return _selObj is Edge; } }

		public Selection Current()
		{
			if (_lastWnd == null)
				return null;

			Position p;
			try {
				bool allowOob = _selObj is Edge;
				p = _lastWnd.GetPosition(_lastX, _lastY, allowOob);
				_lastWnd.Selected = true;
			}
			catch {
				_lastWnd.Selected = false;
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

			return new Selection(_selObj, _lastWnd, p);
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

			_orgWnd.Cursor = new Selection(_selObj, _orgWnd, _orgPos);
			_orgWnd.AdoptCursor();
			Transfer.Source = _orgWnd.Cursor;
		}

		public void End()
		{
			if (_lastWnd == null)
				return;

			_lastWnd.AdoptCursor();
			_lastWnd.Cursor = null;
		}

		public Selection Cancel()
		{
			// maybe clear inputcontrolstab sample?
			_orgWnd.Cursor = null;
			_orgWnd.Selected = true;
			return _orgWnd.CurrentSelection();
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

		public Selection Update(TileWindow wnd, int x, int y)
		{
			_lastX = x;
			_lastY = y;

			if (!_escaped && this.HasLeft(wnd))
				Escape();

			if (_selObj is Edge)
				wnd = _orgWnd;

			if (wnd != _lastWnd && _lastWnd != null)
			{
				_lastWnd.Cursor = null;
				_lastWnd.Selected = false;
			}

			_lastWnd = wnd;

			Selection sel = this.Current();
			if (_lastWnd != null)
				_lastWnd.Cursor = sel;

			return sel;
		}
	}
}
