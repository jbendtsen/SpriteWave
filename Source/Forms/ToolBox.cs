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
		private const float HeightFraction = 0.4f;

		private bool _isOpen;

		private bool _isActive;
		private bool Active
		{
			get { return _isActive; }
			set {
				_isActive = value;
				_tabs.Visible = value;
				_switch.Visible = value;
				_minimise.Visible = value;
			}
		}

		private TileWindow _wnd;
		public TileWindow ActiveWindow
		{
			get {
				return _isActive ? _wnd : null;
			}
			set {
				bool ctrlTabOpen = _tabs.SelectedTab == _wnd.ControlsTab;
				_tabs.Controls.Remove(_wnd.ControlsTab);

				_wnd = value;
				_tabs.Controls.Add(_wnd.ControlsTab);
				if (ctrlTabOpen)
					_tabs.SelectedTab = _wnd.ControlsTab;
			}
		}

		private TabControl _tabs;

		private Button _switch;
		private Button _minimise;

		private Bitmap _imgMinimise;
		private Bitmap _imgMaximise;

		public ToolBox(TabControl box, TileWindow initialWnd, Button switchWindow, Button minimiseTb)
		{
			_tabs = box;
			_wnd = initialWnd;
			_switch = switchWindow;
			_minimise = minimiseTb;

			_tabs.Controls.Add(_wnd.ControlsTab);

			_imgMinimise = new Bitmap(10, 16);
			_imgMaximise = new Bitmap(10, 16);

			using (var pen = new Pen(Color.Black))
			{
				using (var g = Graphics.FromImage(_imgMinimise))
					g.DrawLine(pen, 0, 7, 10, 7);

				using (var g = Graphics.FromImage(_imgMaximise))
				{
					g.DrawLine(pen, 0, 2, 9, 2);   // top
					g.DrawLine(pen, 0, 11, 9, 11); // bottom
					g.DrawLine(pen, 0, 2, 0, 11);   // left
					g.DrawLine(pen, 9, 2, 9, 11); // right
				}
			}

			_minimise.Image = _imgMinimise;

			_isActive = true;
			_isOpen = true;
		}

		public void Minimise()
		{
			_isOpen = !_isOpen;
			_minimise.Image = _isOpen ? _imgMinimise : _imgMaximise;
		}

		public void UpdateLayout(ToolBoxOrientation layout, Size clientSize)
		{
			if (layout == ToolBoxOrientation.None)
			{
				this.Active = false;
				return;
			}
			if (!_isActive)
				this.Active = true;

			int tileWndWidth = _wnd.CanvasSize.Width;
			int tileWndX = _wnd.CanvasPos.X;

			const int tbBottomGap = 0;
			int tbTabH = _tabs.Padding.Y + _tabs.ItemSize.Height;

			int tbX = tileWndX;
			int tbW = tileWndWidth - _switch.Size.Width;

			int tbH, tbSwitchGap;
			if (_isOpen)
			{
				tbH = (int)((float)clientSize.Height * HeightFraction);
				tbSwitchGap = tbTabH;
			}
			else
			{
				tbH = tbTabH;
				tbSwitchGap = 0;
			}

			if (tbH > MaxHeight)
				tbH = MaxHeight;

			int tbY = clientSize.Height - (tbH + tbBottomGap);

			int swX, minX;
			if (layout == ToolBoxOrientation.Left)
			{
				//tbX += 3;
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

			_tabs.Location = new Point(tbX, tbY);
			_switch.Location = new Point(swX, tbY + tbSwitchGap);
			_minimise.Location = new Point(minX, tbY);

			_tabs.Size = new Size(tbW, tbH);
			_switch.Size = new Size(20, tbH - tbSwitchGap - 1);

			_wnd.AdjustControlsTab();
		}
	}
}