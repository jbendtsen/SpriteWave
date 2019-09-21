using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public enum ToolBoxOrientation
	{
		Left, Right, None
	}

	public class ToolBox : ITabCollection
	{
		private const int MaxHeight = 300;
		private const int MinMinWidth = 100;

		private readonly Color TabPanelColor = Color.FromArgb(250, 250, 250);
		private readonly Color TabButtonColor = Color.FromArgb(210, 210, 210);

		private bool _isActive;
		private bool _isOpen;
		private bool _canSwitch;

		private TileWindow _wnd;

		private List<ITab> _generalTabs;
		private ITab _curTab;

		private Panel _ui;
		private ToolBoxButton _switch;
		private ToolBoxButton _minimise;
		private Panel _tabButtons;

		private Action _refresh;
		private Utils.ControlFunc _configure;

		public bool IsOpen { get { return _isOpen && _wnd != null && _wnd.IsActive; } }

		public bool IsActive
		{
			get {
				return _isActive;
			}
			set {
				_isActive = value;
				_minimise.Visible = value;

				if (!value)
					_switch.Visible = false;

				_canSwitch = false;
			}
		}

		public bool Switch
		{
			set {
				_canSwitch = value;
				UpdateSwitch();
				Refresh();
			}
		}

		public TileWindow CurrentWindow
		{
			get {
				return _wnd;
			}
			set {
				_wnd.RescindTabButtons(this);

				if (!_generalTabs.Contains(_curTab))
				{
					int idx = _wnd.TabIndex(_curTab);
					idx = idx >= 0 ? idx : 0;

					_wnd = value;
					Select(this[idx + _generalTabs.Count], reconfig: false);
				}
				else
				{
					_wnd = value;
					Select(_curTab); // updates current tab
				}

				_wnd.ProvideTabButtons(this);

				Utils.ApplyRecursiveControlFunc(_ui, _configure);
				Refresh();
			}
		}

		public ITab this[int idx]
		{
			get {
				if (idx < 0)
					return null;
	
				int gLen = _generalTabs.Count;
				if (idx < gLen)
					return _generalTabs[idx];
	
				idx -= gLen;
				if (_wnd == null || idx >= _wnd.TabCount)
					return null;

				return _wnd[idx];
			}
		}

		public ITab this[string name]
		{
			get {
				int i;
				for (i = 0; i < _generalTabs.Count; i++)
				{
					if (_generalTabs[i].Name == name)
						return _generalTabs[i];
				}
	
				return _wnd[name];
			}
		}

		public int TabCount
		{
			get {
				int n = _generalTabs.Count;
				if (_wnd != null)
					n += _wnd.TabCount;

				return n;
			}
		}

		public Size Minimum
		{
			get {
				Size s = new Size(MinMinWidth, 0);

				if (this.IsOpen && _curTab != null)
					s = _curTab.Minimum;

				s.Width += _switch.Width;
				s.Height += _minimise.Height;
				return s;
			}
		}

		public ToolBox(MainForm main, TileWindow initialWnd)
		{
			_wnd = initialWnd;
			_refresh = main.PerformLayout;
			_configure = main.ConfigureControls;

			_generalTabs = new List<ITab>();

			_switch = new ToolBoxButton(ToolBoxShapes.Switch, new Size(20, 140));
			_switch.Name = "toolBoxSwitchWindow";
			_switch.Click += (s, e) => main.SwitchToolBoxWindow();

			_minimise = new ToolBoxButton(ToolBoxShapes.Minimise, new Size(40, 21), 1);
			_minimise.Name = "toolBoxMinimise";
			_minimise.Click += (s, e) => { Minimise(); Refresh(); };

			_ui = new Panel();
			_ui.Controls.Add(_switch);
			_ui.Controls.Add(_minimise);

			_tabButtons = new Panel();
			_tabButtons.Size = new Size(0, 0);

			foreach (ITab t in _generalTabs)
				AddTabButton(t.TabButton);

			_wnd.ProvideTabButtons(this);

			_ui.Controls.Add(_tabButtons);
			main.Controls.Add(_ui);

			_switch.Visible = false;

			IsActive = false;
			_isOpen = true;
		}

		public void Refresh()
		{
			if (_refresh != null)
				_refresh();
		}

		public void Activate(TileWindow wnd)
		{
			this.IsActive = true;
			_wnd = wnd;
			Select(0);
			if (!_isOpen)
				Minimise();

			Refresh();
		}

		public void CloseGeneralTabs()
		{
			int i;
			for (i = 0; i < _generalTabs.Count; i++)
			{
				_ui.Controls.Remove(_generalTabs[i].Panel);
				_generalTabs[i] = null;
			}
		}

		public void RefreshTab()
		{
			if (!_isOpen)
				Minimise();
			else
				Refresh();
		}

		public bool HandleEscapeKey()
		{
			if (_curTab == null)
				return false;

			return _curTab.HandleEscapeKey();
		}

		// Implements ITabCollection.TabIndex
		public int TabIndex(ITab t)
		{
			int idx = _generalTabs.IndexOf(t);
			if (idx < 0)
			{
				idx = _wnd.TabIndex(t);
				idx = idx >= 0 ? idx + _generalTabs.Count : -1;
			}

			return idx;
		}

		private void Select(ITab t, bool reconfig = true)
		{
			if (t == null)
				t = _wnd[0];

			t.Window = _wnd;

			if (t == _curTab)
				return;

			if (_curTab != null)
			{
				_curTab.Panel.Visible = false;
				_curTab.TabButton.BackColor = Button.DefaultBackColor;
			}

			_curTab = t;

			t.Panel.Visible = true;
			t.TabButton.BackColor = TabButtonColor;

			foreach (Control c in _ui.Controls)
			{
				if (c != _tabButtons && c is Panel)
					_ui.Controls.Remove(c);
			}

			t.Panel.BackColor = TabPanelColor;
			_ui.Controls.Add(t.Panel);

			if (reconfig)
				Utils.ApplyRecursiveControlFunc(_ui, _configure);
		}
		public void Select(int idx)
		{
			Select(this[idx]);
		}
		public void Select(string name)
		{
			Select(this[name]);
		}

		public void Cycle(int dir)
		{
			int nTabs = this.TabCount;
			Select((this.TabIndex(_curTab) + nTabs + dir) % nTabs);
		}

		public Control GetControl(string name)
		{
			if (!this.IsOpen)
				return null;

			Control c = Utils.FindControl(_curTab.Panel, name);
			if (c != null)
				c.Visible = true;

			return c;
		}

		private void UpdateSwitch()
		{
			if (_canSwitch)
				_switch.Visible = _isOpen;
			else
				_switch.Visible = false;
		}

		public void Minimise()
		{
			_isOpen = !_isOpen;
			_minimise.State = _isOpen ? 1 : 0;

			_curTab.Panel.Visible = _isOpen;

			UpdateSwitch();
		}

		// a lil' bit hacky
		private void tabButtonClick(object sender, EventArgs e)
		{
			ITab old = _curTab;
			Select((sender as ToolBoxButton).Tag as ITab);

			bool needsRefresh = _curTab != old;
			if (!this.IsOpen)
			{
				Minimise();
				needsRefresh = true;
			}
			if (needsRefresh)
			{
				Refresh();
				Refresh(); // just a tad
			}
		}

		public void AddTabButton(Button btn)
		{
			btn.Visible = true;

			if (_tabButtons.Controls.Contains(btn))
				return;

			btn.Click += this.tabButtonClick;
			_tabButtons.Controls.Add(btn);
		}

		public void RemoveTabButton(Button btn)
		{
			if (_tabButtons.Controls.Contains(btn))
				btn.Visible = false;
		}

		public void AdjustTabButtons(Point loc)
		{
			int x = 0;
			int h = 0;
			foreach (Control c in _tabButtons.Controls)
			{
				if (!c.Visible)
					continue;

				int cH = c.Height;
				if (cH > h)
					h = cH;

				c.Location = new Point(x, 0 /*c.Location.Y*/);
				x += c.Width;
			}

			_tabButtons.Size = new Size(x, h);
			_tabButtons.Location = loc;
		}

		public void AdjustCurrentTab(ToolBoxOrientation layout)
		{
			_curTab.AdjustContents(
				new Size(
					_ui.Width - _switch.Width,
					_ui.Height - _minimise.Height
				),
				layout
			);
		}

		public void UpdateLayout(ToolBoxOrientation layout, Size clientSize)
		{
			if (!IsActive)
				return;

			int tileWndX = _wnd.CanvasPos.X;

			_ui.Size = new Size(_wnd.CanvasSize.Width, this.Minimum.Height);
			_ui.Location = new Point(_wnd.CanvasPos.X, clientSize.Height - _ui.Height);

			int swX = 0;
			int minX = 0;
			int tabX = 0;
			int btnsX = _switch.Width;

			if (layout == ToolBoxOrientation.Left)
			{
				swX = _ui.Width - _switch.Width;
				btnsX = swX - _tabButtons.Width;
				_switch.State = 0;
			}
			else
			{
				minX = _ui.Width - _minimise.Width;
				tabX = _switch.Width;
				_switch.State = 1;
			}

			_minimise.Location = new Point(minX, _ui.Height - _minimise.Height);

			_switch.Location = new Point(swX, 0);
			_switch.Size = new Size(20, _ui.Height - 1);

			if (_curTab != null)
			{
				_curTab.X = tabX;
				AdjustCurrentTab(layout);
				AdjustTabButtons(new Point(btnsX, _ui.Height - _minimise.Height));
			}
		}
	}
}