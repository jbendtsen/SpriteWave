using System;
using System.Drawing;
using System.Windows.Forms;

using System.Diagnostics;

namespace SpriteWave
{
	//public enum Edge { Nil, Top, Bottom, Left, Right }

	public class DragPoint : ISelection
	{
		private Brush _hl;
		private object _obj;
		private TileWindow _wnd;
		private Position _loc;

		public ISelection Selection { get { return null; } set { return; } }
		public Tile Tile { get { return _obj as Tile; } }
		public Position Location { get { return _loc; } set { _loc = value; } }

		public DragPoint(Brush hl, object selObj, TileWindow wnd, Position loc)
		{
			_hl = hl;
			_obj = selObj;
			_wnd = wnd;
			_loc = loc;
		}

		public void Receive(ISelection isel)
		{
			_wnd.Receive(isel);
		}

		public void DrawSelection(TileWindow wnd, Graphics g)
		{
			if (wnd != _wnd)
				return;

			Position vis = wnd.VisibleSelection;
			if (_loc.col < 0 || _loc.col >= vis.col ||
				_loc.row < 0 || _loc.row >= vis.row)
			{
				return;
			}

			Rectangle selRect = _loc.TileRect(wnd.TilePx);
			g.FillRectangle(_hl, selRect);
		}
	}

	public class DragObject
	{
		private bool _escaped;
		public bool Started { get { return _escaped; } }

		private Cursor _cur;

		// Stay classy, DragObject
		//private int _orgX, _orgY;
		private Position _orgPos;
		private TileWindow _orgWnd;

		private int _lastX, _lastY;
		private TileWindow _lastWnd;
		public TileWindow Window { get { return _lastWnd; } }

		private Brush _hl;
		private object _selObj;

		public ISelection Selection
		{
			get {
				if (_lastWnd == null)
					return null;

				//Debug.WriteLine("_lastX = {0}, _lastY = {1}", _lastX, _lastY);

				Position p = _lastWnd.GetPosition(_lastX, _lastY);
				return new DragPoint(_hl, _selObj, _lastWnd, p);
			}
		}

		public DragObject(TileWindow wnd, int x, int y)
		{
			_lastX = x;
			_lastY = y;

			_lastWnd = wnd;
			_orgWnd = wnd;
			_orgPos = _orgWnd.GetPosition(x, y);

			_selObj = _orgWnd.SelectTileAt(_orgPos);
			if (_selObj != null)
			{
				Bitmap img = _orgWnd.TileBitmap(_selObj as Tile);
				img = img.SetAlpha(0.6f).Scale(4);
				_cur = new Cursor(img.GetHicon());
			}

			_escaped = false;
			_hl = new SolidBrush(Color.FromArgb(96, 0, 255, 64));
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
			Cursor.Current = _cur;
		}

		public ISelection Update(TileWindow wnd, int x, int y)
		{
			_lastX = x;
			_lastY = y;

			if (!_escaped)
			{
				if (wnd == null || wnd != _orgWnd)
					Escape();
				else
				{
					Position loc = wnd.GetPosition(_lastX, _lastY);
					if (loc.col != _orgPos.col || loc.row != _orgPos.row)
						Escape();
				}
			}

			if (wnd != _lastWnd && _lastWnd != null)
				_lastWnd.Selection = null;

			_lastWnd = wnd;

			ISelection isel = Selection;
			if (_lastWnd != null)
				_lastWnd.Selection = isel;

			return isel;
		}
	}
}
