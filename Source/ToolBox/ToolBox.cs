using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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

		private readonly Color TabColour = Color.FromArgb(250, 250, 250);

		private bool _isActive;
		private bool _isOpen;
		private bool _canSwitch;

		private TileWindow _wnd;

		private List<ITab> _generalTabs;
		private int _curTab;

		private Panel _ui;
		private ToolBoxButton _switch;
		private ToolBoxButton _minimise;

		private Action _refresh;

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
				_wnd = value;
				Select(_curTab);
				Refresh();
			}
		}

		public ITab CurrentTab
		{
			get {
				return TabAt(_curTab);
			}
		}

		public Size Minimum
		{
			get {
				Size s = new Size(MinMinWidth, 0);

				ITab t = CurrentTab;
				if (this.IsOpen && t != null)
					s = t.Minimum;

				s.Width += _switch.Width;
				s.Height += _minimise.Height;
				return s;
			}
		}

		public static void ConfigureTabs(List<ITab> tabs, Utils.ControlAction method)
		{
			foreach (ITab t in tabs)
				Utils.ApplyRecursiveControlAction(t.Panel, method);
		}

		public ToolBox(MainForm main, TileWindow initialWnd)
		{
			_wnd = initialWnd;
			_refresh = main.PerformLayout;

			_generalTabs = new List<ITab>();
			_generalTabs.Add(new PaletteTab(_wnd));

			_switch = new ToolBoxButton(_switchShapes, new Size(20, 140));
			_switch.Name = "toolBoxSwitchWindow";
			_switch.Click += (s, e) => main.SwitchToolBoxWindow();

			_minimise = new ToolBoxButton(_minimiseShapes, new Size(40, 21), 1);
			_minimise.Name = "toolBoxMinimise";
			_minimise.Click += (s, e) => { Minimise(); Refresh(); };

			_ui = new Panel();
			_ui.Controls.Add(_switch);
			_ui.Controls.Add(_minimise);

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

		public ITab TabAt(int idx)
		{
			if (idx < 0)
				return null;

			int gLen = _generalTabs.Count;
			if (idx < gLen)
				return _generalTabs[idx];

			idx -= gLen;
			if (_wnd == null || idx >= _wnd.Tabs.Count)
				return null;

			return _wnd.Tabs[idx];
		}

		public int TabIndex(string name)
		{
			int i;
			for (i = 0; i < _generalTabs.Count; i++)
			{
				if (_generalTabs[i].Name == name)
					return i;
			}

			int idx = _wnd.TabIndex(name);
			if (idx >= 0)
				idx += _generalTabs.Count;

			return idx;
		}

		public void Select(int idx)
		{
			if (idx < 0)
				return;

			CurrentTab.Panel.Visible = false;

			_curTab = idx;
			var t = CurrentTab;
			t.Window = _wnd;
			t.Panel.BackColor = TabColour;
			t.Panel.Visible = true;

			foreach (Control c in _ui.Controls)
			{
				if (c is Panel)
					_ui.Controls.Remove(c);
			}

			_ui.Controls.Add(t.Panel);
		}
		public void Select(string name)
		{
			Select(TabIndex(name));
		}

		public Control GetControl(string name)
		{
			if (!this.IsOpen)
				return null;

			Control c = Utils.FindControl(CurrentTab.Panel, name);
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

			CurrentTab.Panel.Visible = _isOpen;

			UpdateSwitch();
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
			if (layout == ToolBoxOrientation.Left)
			{
				swX = _ui.Width - _switch.Width;
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

			var tab = this.CurrentTab;
			tab.X = tabX;

			tab.AdjustContents(
				new Size(
					_ui.Width - _switch.Width,
					_ui.Height - _minimise.Height
				)
			);
		}

		public void HandleTabClick()
		{
			if (!_isOpen)
				Minimise();
			else
				Refresh();
		}

		public void Cycle(int dir)
		{
			var tabs = _wnd.Tabs;
			_curTab = (_curTab + tabs.Count + dir) % tabs.Count;
		}

		Point[][] _minimiseShapes = {
			new[] {
				new Point(12, 2),
				new Point(22, 2),
				new Point(22, 11),
				new Point(12, 11),
				new Point(12, 2)
			},
			new[] {
				new Point(12, 7),
				new Point(22, 7)
			}
		};

		PointF[][] _switchShapes = {
			new[] {
				new PointF(0.05f, 0.25f),
				new PointF(0.85f, 0.50f),
				new PointF(0.05f, 0.75f)
			},
			new[] {
				new PointF(0.85f, 0.25f),
				new PointF(0.05f, 0.50f),
				new PointF(0.85f, 0.75f)
			}
		};
	}

	public class ToolBoxButton : Button
	{
		private readonly Pen _pen = new Pen(Color.Black);

		private PointF[][] _shapes;
		private Bitmap[] _imgs;

		private int _idx;
		public int State
		{
			set {
				_idx = value;
				this.Image = _imgs[_idx];
			}
		}

		public ToolBoxButton(PointF[][] fracPoints, Size size, int startIdx = 0)
		{
			_shapes = fracPoints;
			Initialise(size, startIdx);
		}

		public ToolBoxButton(Point[][] pixPoints, Size size, int startIdx = 0)
		{
			_shapes = new PointF[pixPoints.Length][];
			Size client = Interior(size);

			int i = 0, j = 0;
			foreach (Point[] shp in pixPoints)
			{
				_shapes[i] = new PointF[shp.Length];
				foreach (Point p in shp)
					_shapes[i][j++] = new PointF(
						(float)p.X / (float)client.Width,
						(float)p.Y / (float)client.Height
					);

				j = 0;
				i++;
			}

			Initialise(size, startIdx);
		}

		private void Initialise(Size size, int startIdx)
		{
			this.BackColor = SystemColors.ControlLight;
			this.FlatAppearance.BorderSize = 0;
			this.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.UseVisualStyleBackColor = false;
			this.SetStyle(ControlStyles.Selectable, false);

			_imgs = new Bitmap[_shapes.Length];
			_idx = startIdx;
			this.Size = size; // Calls Render()
		}

		public void Render()
		{
			if (_shapes == null)
				return;

			Size client = Interior(this.Size);

			if (client.Width < 1 || client.Height < 1)
				return;

			for (int i = 0; i < _shapes.Length; i++)
				_imgs[i] = CreateShape(_shapes[i], client);

			this.Image = _imgs[_idx];
		}

		private static Size Interior(Size s)
		{
			return new Size(
				s.Width - 4,
				s.Height - 5
			);
		}

		private Bitmap CreateShape(PointF[] poly, Size area)
		{
			int w = area.Width;
			int h = area.Height;

			Point[] scaled = new Point[poly.Length];
			int i = 0;
			foreach (PointF p in poly)
				scaled[i++] = new Point(
					(int)(p.X * (float)w),
					(int)(p.Y * (float)h)
				);

			Bitmap canvas = new Bitmap(area.Width, area.Height);
			using (var g = Graphics.FromImage(canvas))
			{
				g.SmoothingMode = SmoothingMode.AntiAlias;
				g.DrawLines(_pen, scaled);
			}

			return canvas;
		}

		protected override void OnLayout(LayoutEventArgs e)
		{
			base.OnLayout(e);
			Render();
		}
	}
}