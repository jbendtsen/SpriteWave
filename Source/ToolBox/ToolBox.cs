using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public enum ToolBoxOrientation
	{
		Left, Right, None
	}

	public class ToolBox
	{
		private const int MaxHeight = 300;
		private const int MinMinWidth = 100;
		private const float HeightFraction = 0.4f;

		private bool _isOpen;

		private TileWindow _wnd;

		private TabControl _tabs;
		private bool _tabChanged;

		private Button _switch;
		private Button _minimise;

		private Bitmap _imgMinimise;
		private Bitmap _imgMaximise;

		private Action _refresh;

		public bool IsOpen { get { return _isOpen; } }

		public TileWindow CurrentWindow
		{
			get {
				return _wnd;
			}
			set {
				bool ctrlTabOpen = _tabs.SelectedTab == _wnd.ControlsTab;
				_tabs.TabPages.Remove(_wnd.ControlsTab);

				_wnd = value;
				_tabs.TabPages.Add(_wnd.ControlsTab);

				if (ctrlTabOpen)
					_tabs.SelectedTab = _wnd.ControlsTab;

				_tabChanged = false;
				Refresh();
			}
		}

		public string CurrentTab
		{
			set {
				_tabs.SelectTab(value);
			}
		}

		public int MinimumWidth
		{
			get {
				int w = MinMinWidth;

				ITab tab = _tabs.SelectedTab as ITab;
				if (_isOpen && tab != null)
					w = tab.MinimumWidth;

				return w + _switch.Size.Width;
			}
		}
		public int MinimumHeight
		{
			get {
				int h = 0;

				ITab tab = _tabs.SelectedTab as ITab;
				if (_isOpen && tab != null)
					h = tab.MinimumHeight;

				return h + _tabs.ItemSize.Height + _tabs.Padding.Y - 1;
			}
		}

		public int Height { get { return _tabs.Size.Height; } }

		public EventHandler SwitchWindowAction { set { _switch.Click += value; } }

		public ToolBox(TabControl box, TileWindow initialWnd, Button switchWindow, Button minimiseTb, Action refresh)
		{
			_tabs = box;
			_wnd = initialWnd;
			_switch = switchWindow;
			_minimise = minimiseTb;
			_refresh = refresh;

			_tabs.Controls.Add(new PaletteTab(_wnd));
			_tabs.Controls.Add(_wnd.ControlsTab);

			_tabs.Deselected += (s, e) => TogglePage(e.TabPageIndex, false);
			_tabs.Selected += (s, e) => { TogglePage(e.TabPageIndex, true); _tabChanged = true; };
			_minimise.Click += (s, e) => { Minimise(); Refresh(); };

			_imgMinimise = new Bitmap(10, 16);
			_imgMaximise = new Bitmap(10, 16);

			using (var pen = new Pen(Color.Black))
			{
				// single line for the minimise icon
				using (var g = Graphics.FromImage(_imgMinimise))
					g.DrawLine(pen, 0, 7, 10, 7);

				// four lines make a box for the maximise icon
				using (var g = Graphics.FromImage(_imgMaximise))
				{
					g.DrawLine(pen, 0, 2, 9, 2);   // top
					g.DrawLine(pen, 0, 11, 9, 11); // bottom
					g.DrawLine(pen, 0, 2, 0, 11);  // left
					g.DrawLine(pen, 9, 2, 9, 11);  // right
				}
			}

			_minimise.Image = _imgMinimise;

			_tabChanged = false;
			_isOpen = true;
		}

		public void TogglePage(int idx, bool state)
		{
			if (idx >= 0 && idx < _tabs.Controls.Count)
			{
				System.Diagnostics.Debug.WriteLine("_tabs[{0}].Visible = {1}", idx, state);
				_tabs.Controls[idx].Visible = state;
			}
		}

		public void Refresh()
		{
			if (_refresh != null)
				_refresh();
		}

		public Control GetControl(string name)
		{
			if (!_isOpen || _wnd == null)
				return null;

			Control c = Utils.FindControl(_tabs.SelectedTab, name);
			if (c != null)
				c.Visible = true;

			return c;
		}

		public void Minimise()
		{
			_isOpen = !_isOpen;
			_minimise.Image = _isOpen ? _imgMinimise : _imgMaximise;
		}

		public void UpdateLayout(ToolBoxOrientation layout, Size clientSize)
		{
			int tileWndWidth = _wnd.CanvasSize.Width;
			int tileWndX = _wnd.CanvasPos.X;

			int tbW = tileWndWidth - _switch.Size.Width;
			int tbH = this.MinimumHeight;

			int tbX = tileWndX;
			int tbY = clientSize.Height - tbH;

			int swX, minX;
			if (layout == ToolBoxOrientation.Left)
			{
				swX = tbX + tbW;
				minX = tbX;

				_tabs.RightToLeft = RightToLeft.Yes;
				_tabs.RightToLeftLayout = true;
				foreach (Control c in _tabs.Controls)
					c.RightToLeft = RightToLeft.No;

				_switch.Text = ">";
			}
			else
			{
				swX = tbX;
				tbX += _switch.Size.Width;
				minX = tbX + tbW - _minimise.Size.Width;

				_tabs.RightToLeft = RightToLeft.No;
				_tabs.RightToLeftLayout = false;

				_switch.Text = "<";
			}

			int tbTabH = _tabs.Padding.Y + _tabs.ItemSize.Height;
			_minimise.Location = new Point(minX, tbY + tbH - tbTabH);

			_tabs.Location = new Point(tbX, tbY);
			_switch.Location = new Point(swX, tbY);

			_tabs.Size = new Size(tbW, tbH);
			_switch.Size = new Size(20, tbH - 1);

			ITab tab = _tabs.SelectedTab as ITab;
			if (tab != null)
				tab.AdjustContents();
		}

		public void HandleTabClick()
		{
			if (!_isOpen)
				Minimise();
			else if (!_tabChanged)
				Minimise();

			_tabChanged = false;
		}
	}
}