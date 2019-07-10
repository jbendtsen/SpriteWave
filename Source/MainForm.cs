using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace SpriteWave
{
	public partial class MainForm : Form
	{
		private InputWindow _inputWnd;
		private SpriteWindow _spriteWnd;
		private ToolBox _toolBox;

		private Dictionary<FormatKind, FileFormat> _formatList;

		private DragObject _drag;

		public delegate void LayoutDelegate();
		public delegate void TileAction(TileWindow tw);

		public MainForm()
		{
			_formatList = new Dictionary<FormatKind, FileFormat>();

			_formatList[FormatKind.NES] = new FileFormat(
				"NES",
				Utils.TileType("NESTile"),
				new string[] { "nes", "fds", "chr", "bin" },
				new ColourTable(Utils.NESPalette, Utils.NESDefSel)
			);

			_formatList[FormatKind.SNES] = new FileFormat(
				"SNES",
				Utils.TileType("SNESTile"),
				new string[] { "smc", "sfc", "chr", "bin" },
				new ColourTable(Utils.SNESRGBAOrderAndDepth, Utils.SNESDefSel)
			);

			InitializeComponent();

			string filter = "";
			foreach (var fmt in _formatList)
				filter += fmt.Value.Filter;

			filter = filter.Remove(filter.Length-1);
			this.openFileDialog1.Filter = filter;

			_inputWnd = new InputWindow(this, this.CopyTile);
			var inputTab = _inputWnd.ControlsTab as InputControlsTab;
			inputTab.SendTileAction = (s, e) => {CopyTile(_inputWnd); PasteTile(_spriteWnd);};

			_spriteWnd = new SpriteWindow(this, this.CopyTile, this.PasteTile);

			_toolBox = new ToolBox(this.toolBoxTabs, _spriteWnd, this.toolBoxSwitchWindow, this.toolBoxMinimise, this.PerformLayout);
			_toolBox.SwitchWindowAction = this.switchToolBoxWindow;

			// Setup MainForm events
			this.KeyPreview = true;
			this.KeyDown += this.keysHandler;
			this.Layout += this.layoutHandler;
			Utils.ApplyRecursiveControlAction(this, this.SetMouseEventHandler);

			UpdateMinimumSize();
			_inputWnd.Focus(this);
		}

		private void Draw()
		{
			this.Refresh();
			_inputWnd.Draw();
			_spriteWnd.Draw();
		}

		private void CopyTile(TileWindow wnd)
		{
			wnd.Selected = true;
			Transfer.Source = wnd.CurrentSelection();
			Transfer.Start();
		}
		private void PasteTile(TileWindow wnd)
		{
			wnd.Selected = true;
			Transfer.Dest = wnd.CurrentSelection();
			Transfer.Paste();
			Draw();
		}
		private void SwapTile(TileWindow wnd)
		{
			wnd.Selected = true;
			Transfer.Dest = wnd.CurrentSelection();
			Transfer.Swap();
			Draw();
		}

		private void ClearSelection()
		{
			_inputWnd.Selected = false;
			_spriteWnd.Selected = false;
			Transfer.Clear();
		}

		private object SetMouseEventHandler(Control ctrl, object args)
		{
			ctrl.MouseDown += new MouseEventHandler(this.mouseDownHandler);
			ctrl.MouseMove += new MouseEventHandler(this.mouseMoveHandler);
			ctrl.MouseUp   += new MouseEventHandler(this.mouseUpHandler);
			return null;
		}

		private void StartDrag(TileWindow wnd, int x, int y)
		{
			Transfer.Dest = null;

			try {
				_drag = new DragObject(wnd, x, y);
			}
			catch (ArgumentOutOfRangeException) {
				_drag = null;
				ClearSelection();
			}
		}

		private void ShowMenuAt(TileWindow wnd, int x, int y)
		{
			try {
				wnd.Position = wnd.GetPosition(x, y);
				Transfer.Source = wnd.CurrentSelection();
				if (wnd.EdgeOf(wnd.Position) == EdgeKind.None)
					wnd.ShowMenu(x, y);
			}
			catch (ArgumentOutOfRangeException) {
				ClearSelection();
			}
		}

		private void mouseDownHandler(object sender, MouseEventArgs e)
		{
			var ctrl = sender as Control;
			if (ctrl is TabControl)
			{
				_toolBox.HandleTabClick();
				this.PerformLayout();
				return;
			}

			if (_drag != null)
				return;

			if (ctrl is PictureBox)
				this.ActiveControl = ctrl;

			TileWindow wnd = null;
			if (_inputWnd.WindowIs(ctrl))
				wnd = _inputWnd;
			else if (_spriteWnd.WindowIs(ctrl))
				wnd = _spriteWnd;

			if (wnd != null)
			{
				if (e.Button == MouseButtons.Left)
				{
					StartDrag(wnd, e.X, e.Y);
				}
				else if (e.Button == MouseButtons.Right)
				{
					ShowMenuAt(wnd, e.X, e.Y);
				}
			}

			Draw();
		}

		private void mouseUpHandler(object sender, MouseEventArgs e)
		{
			if (_drag != null)
			{
				if (_drag.Started)
				{
					_drag.End();
/*
					if ((Control.ModifierKeys & Keys.Shift) != 0)
						Transfer.Swap();
					else
						Transfer.Paste();
*/
					Transfer.Paste();
					ClearSelection();
				}
				else
				{
					Transfer.Source = _drag.Cancel();
				}
			}

			_drag = null;
			Draw();
		}

		private object RefreshControl(Control c, object args)
		{
			if (c.HasMouse())
				args = c;
			if (c is ScrollBar || c is TextBox || c is Button)
				c.Refresh();

			return args;
		}
		
		private void mouseMoveHandler(object sender, MouseEventArgs e)
		{
			var startCtrl = sender as Control;
			var curCtrl = Utils.ApplyRecursiveControlAction(this, RefreshControl) as Control;

			if (_drag == null)
				return;

			TileWindow wnd = null;
			if (_inputWnd.WindowIs(curCtrl))
				wnd = _inputWnd;
			else if (_spriteWnd.WindowIs(curCtrl))
				wnd = _spriteWnd;

			int x = 0, y = 0;
			if (_drag.IsEdge)
			{
				x = e.X;
				y = e.Y;
			}
			else if (wnd != null)
			{
				// When the mouse is held, e.X and e.Y are relative to startCtrl.Location
				Point toCur = new Point(
					curCtrl.Location.X - startCtrl.Location.X,
					curCtrl.Location.Y - startCtrl.Location.Y
				);

				x = e.X - toCur.X;
				y = e.Y - toCur.Y;
			}

			Transfer.Dest = _drag.Update(wnd, x, y);
			Draw();
		}

		private void keysHandler(object sender, KeyEventArgs e)
		{
			if (_drag != null)
				return;

			Control active = Utils.FindActiveControl(this);

			if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.ControlKey)
			{
				if (active is TextBox)
					_inputWnd.Focus(this);
				else
				{
					ClearSelection();
					_drag = null;
				}
			}

			if (active is TextBox)
			{
				Draw();
				return;
			}

			if (e.KeyCode == Keys.Delete)
			{
				_spriteWnd.EraseTile();
			}

			if (e.KeyCode == Keys.Enter)
			{
				CopyTile(_inputWnd);
				PasteTile(_spriteWnd);
				_spriteWnd.MoveSelection(1, 0);
			}

			Keys mod = Control.ModifierKeys;

			if ((mod & Keys.Control) != 0)
			{
				if (e.KeyCode == Keys.G && _inputWnd.IsActive)
				{
					_toolBox.CurrentWindow = _inputWnd;
					_toolBox.CurrentTab = "controlsTab";
					if (!_toolBox.IsOpen)
						_toolBox.Minimise();

					this.PerformLayout();
					this.ActiveControl = _toolBox.GetControl("inputOffset");
				}

				if (e.KeyCode == Keys.D0)
					_spriteWnd.Centre();

				int zoom = 0;
				if (e.KeyCode == Keys.OemMinus)
					zoom = -1;
				else if (e.KeyCode == Keys.Oemplus)
					zoom = 1;

				if (zoom != 0)
					_spriteWnd.ZoomByTiles(zoom);

				Action<EdgeKind> moveEdge = _spriteWnd.InsertEdge;
				if ((mod & Keys.Shift) != 0)
					moveEdge = _spriteWnd.DeleteEdge;

				if (e.KeyCode == Keys.I)
					moveEdge(EdgeKind.Top);
				else if (e.KeyCode == Keys.K)
					moveEdge(EdgeKind.Bottom);
				else if (e.KeyCode == Keys.J)
					moveEdge(EdgeKind.Left);
				else if (e.KeyCode == Keys.L)
					moveEdge(EdgeKind.Right);

				Draw();
				return;
			}

			if ((mod & Keys.Shift) != 0)
			{
				bool swap = true;
				int x = 0, y = 0;

				if (e.KeyCode == Keys.I)
					y = -1;
				else if (e.KeyCode == Keys.K)
					y = 1;
				else if (e.KeyCode == Keys.J)
					x = -1;
				else if (e.KeyCode == Keys.L)
					x = 1;
				else
					swap = false;

				if (swap)
				{
					CopyTile(_spriteWnd);
					_spriteWnd.MoveSelection(x, y);
					SwapTile(_spriteWnd);
					return;
				}
			}

			bool move = true;

			if (e.KeyCode == Keys.W)
				_inputWnd.MoveSelection(0, -1);
			else if (e.KeyCode == Keys.S)
				_inputWnd.MoveSelection(0, 1);
			else if (e.KeyCode == Keys.A)
				_inputWnd.MoveSelection(-1, 0);
			else if (e.KeyCode == Keys.D)
				_inputWnd.MoveSelection(1, 0);

			else if (e.KeyCode == Keys.I)
				_spriteWnd.MoveSelection(0, -1);
			else if (e.KeyCode == Keys.K)
				_spriteWnd.MoveSelection(0, 1);
			else if (e.KeyCode == Keys.J)
				_spriteWnd.MoveSelection(-1, 0);
			else if (e.KeyCode == Keys.L)
				_spriteWnd.MoveSelection(1, 0);

			else
				move = false;
			
			if (move)
			{
				Transfer.Source = _inputWnd.CurrentSelection();
				Transfer.Dest = _spriteWnd.CurrentSelection();
			}

			Draw();
		}

		private void UpdateMinimumSize()
		{
			int minW = _inputWnd.ScrollYWidth + _spriteWnd.ScrollYWidth + (_toolBox.MinimumWidth * 2);

			this.MinimumSize = new Size(minW, this.MinimumSize.Height);
		}

		private void layoutHandler(object sender, LayoutEventArgs e)
		{
			int totalH = this.ClientSize.Height;
			int menuH = this.menuStrip1.Size.Height;
			int tileBoxW = this.ClientSize.Width / 2;

			var tbLayout = ToolBoxOrientation.None;

			TileWindow tbWnd = _toolBox.CurrentWindow;
			if (tbWnd == _inputWnd)
				tbLayout = ToolBoxOrientation.Left;
			else if (tbWnd == _spriteWnd)
				tbLayout = ToolBoxOrientation.Right;

			_inputWnd.UpdateLayout(0, tileBoxW, totalH, menuH);
			_spriteWnd.UpdateLayout(tileBoxW, tileBoxW, totalH, menuH);

			_toolBox.UpdateLayout(tbLayout, this.ClientSize);
			if (tbWnd != null)
			{
				int wndMaxH = totalH - (menuH + tbWnd.ScrollXHeight);
				tbWnd.ReduceWindowTo(wndMaxH - _toolBox.Height);
			}

			//_inputWnd.AdjustControlsTab();
			//_spriteWnd.AdjustControlsTab();
			
			UpdateMinimumSize();

			_inputWnd.UpdateBars();
			_spriteWnd.UpdateBars();
			Draw();
		}

		private void switchToolBoxWindow(object sender, EventArgs e)
		{
			if (_toolBox.CurrentWindow == _inputWnd)
				_toolBox.CurrentWindow = _spriteWnd;
			else if (_toolBox.CurrentWindow == _spriteWnd)
				_toolBox.CurrentWindow = _inputWnd;

			this.PerformLayout();
		}

		private void openBinary(object sender, EventArgs e)
		{
			openFileDialog1.ShowDialog();
		}
		private void openNES(object sender, EventArgs e)
		{
			openFileDialog1.FilterIndex = (int)FormatKind.NES + 1;
			openFileDialog1.ShowDialog();
		}
		private void openSNES(object sender, EventArgs e)
		{
			openFileDialog1.FilterIndex = (int)FormatKind.SNES + 1;
			openFileDialog1.ShowDialog();
		}

		private void closeWorkspace(object sender, EventArgs e)
		{
			Transfer.Clear();
			_inputWnd.Close();
			_spriteWnd.Close();
		}

		private void quit(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void openFileDialog1FileOk(object sender, CancelEventArgs e)
		{
			var kind = (FormatKind)(openFileDialog1.FilterIndex - 1);
			var data = File.ReadAllBytes(openFileDialog1.FileName);
			FileFormat fmt = _formatList[kind];

			closeWorkspace(null, null);

			_inputWnd.Load(fmt, data);
			_spriteWnd.FormatToLoad = fmt;
			//_spriteWnd.Prompt = "Drag or send a tile to begin!";

			_inputWnd.ToggleMenu(true);
			_spriteWnd.EnablePaste();

			Draw();
		}
	}
}
