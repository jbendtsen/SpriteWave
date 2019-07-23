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

			EventHandler sendTileAction = (s, e) => {CopyTile(_inputWnd); PasteTile(_spriteWnd);};
			_inputWnd = new InputWindow(this, sendTileAction);

			_spriteWnd = new SpriteWindow(this);

			_toolBox = new ToolBox(this, _inputWnd);

			ToolBox.ConfigureTabs(_inputWnd.Tabs, this.ConfigureControls);
			ToolBox.ConfigureTabs(_spriteWnd.Tabs, this.ConfigureControls);
			//_toolBox.Configure(this.ConfigureControls);

			// Setup MainForm events
			this.KeyPreview = true;
			this.KeyUp += this.keyUpHandler;
			this.KeyDown += this.keysHandler;
			this.Layout += this.layoutHandler;
			Utils.ApplyRecursiveControlAction(this, this.ConfigureControls);

			UpdateMinimumSize();
			_inputWnd.Focus(this);
		}

		private void Draw()
		{
			_inputWnd.Draw();
			_spriteWnd.Draw();
		}

		public void CopyTile(TileWindow wnd)
		{
			wnd.Selected = true;
			Transfer.Source = wnd.CurrentSelection();
			Transfer.Start();
		}
		public void PasteTile(TileWindow wnd)
		{
			wnd.Selected = true;
			Transfer.Dest = wnd.CurrentSelection();
			Transfer.Paste();

			if (wnd == _spriteWnd && Transfer.Completed)
				_toolBox.Switch = true;

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

		private void SetSample(Tile t)
		{
			_inputWnd.TileSample = t;
			this.PerformLayout();
		}

		private object ConfigureControls(Control ctrl, object args)
		{
			ctrl.TabStop = false;
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
				SetSample(null);
			}

			if (_drag != null && wnd == _inputWnd)
			{
				Tile t = _drag.Current().Piece as Tile;
				if (t != null)
					SetSample(t);
			}
		}

		private void ShowMenuAt(TileWindow wnd, int x, int y)
		{
			try {
				wnd.Position = wnd.GetPosition(x, y);
				if (wnd == _inputWnd)
					SetSample(_inputWnd.PieceAt(_inputWnd.Position) as Tile);

				Transfer.Source = wnd.CurrentSelection();
				if (wnd.EdgeOf(wnd.Position) == EdgeKind.None)
					wnd.ShowMenu(x, y);
			}
			catch (ArgumentOutOfRangeException) {
				ClearSelection();
				SetSample(null);
			}
		}

		private object FindControlWithMouse(Control c, object args)
		{
			if (c.HasMouse())
				args = c;

			return args;
		}

		private void mouseMoveHandler(object sender, MouseEventArgs e)
		{
			var startCtrl = sender as Control;
			var curCtrl = Utils.ApplyRecursiveControlAction(this, FindControlWithMouse) as Control;

			TileWindow wnd = null;
			if (_inputWnd.WindowIs(curCtrl))
				wnd = _inputWnd;
			else if (_spriteWnd.WindowIs(curCtrl))
				wnd = _spriteWnd;

			int x = 0, y = 0;
			if (_drag != null && _drag.IsEdge)
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

			bool changed;
			if (_drag == null)
			{
				if (wnd == _spriteWnd)
					changed = _spriteWnd.HighlightEdgeAt(x, y);
				else
					changed = _spriteWnd.ClearMousedEdge();
			}
			else
			{
				Selection sel = _drag.Update(wnd, x, y);

				if (Transfer.Dest == null)
					changed = sel != null;
				else
					changed = !Transfer.Dest.Equals(sel);

				Transfer.Dest = sel;
			}

			if (wnd != null && changed)
				wnd.Draw();
		}

		bool _mouseHeld = false;
		private void mouseDownHandler(object sender, MouseEventArgs e)
		{
			if (_mouseHeld)
				return;

			var ctrl = sender as Control;
			if (ctrl is TabControl)
			{
				_toolBox.HandleTabClick();
				this.PerformLayout();
			}
			else if (_drag == null)
			{
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
						StartDrag(wnd, e.X, e.Y);
					else if (e.Button == MouseButtons.Right)
						ShowMenuAt(wnd, e.X, e.Y);
				}
			}

			_mouseHeld = true;
			Draw();
		}

		private void mouseUpHandler(object sender, MouseEventArgs e)
		{
			if (_drag != null)
			{
				if (_drag.Started)
				{
					_drag.End();

					if ((Control.ModifierKeys & Keys.Shift) != 0)
						Transfer.Swap();
					else
						Transfer.Paste();

					if (!Transfer.Completed)
						SetSample(null);
					else if (Transfer.Dest.Window == _spriteWnd)
						_toolBox.Switch = true;

					ClearSelection();
				}
				else
				{
					Transfer.Source = _drag.Cancel();
				}
			}

			_drag = null;
			_mouseHeld = false;
			Draw();
		}

		private bool _keyHeld = false;
		private void keyUpHandler(object sender, KeyEventArgs e)
		{
			_keyHeld = false;
		}

		private void keysHandler(object sender, KeyEventArgs e)
		{
			if (_drag != null)
			{
				_keyHeld = true;
				return;
			}

			Control active = Utils.FindActiveControl(this);

			// ugly as heck
			if (!_keyHeld && (e.KeyCode == Keys.Escape || e.KeyCode == Keys.ControlKey))
			{
				if (!(active is TextBox))
				{
					// TODO: Employ ITab.HandleEscapeKey()
					if (Transfer.Source != null || Transfer.Dest != null/* || _inputWnd.IsTileSampleVisible*/)
					{
						ClearSelection();
						SetSample(null);
					}
					else if (_toolBox.IsOpen && e.KeyCode == Keys.Escape)
					{
						_toolBox.Minimise();
						this.PerformLayout();
					}
				}

				_inputWnd.Focus(this);
			}

			if (active is TextBox)
			{
				Draw();
				_keyHeld = true;
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
				_spriteWnd.Draw();
			}

			Keys mod = Control.ModifierKeys;

			if ((mod & Keys.Control) != 0)
			{
				if (e.KeyCode == Keys.G && _inputWnd.IsActive)
				{
					_toolBox.CurrentWindow = _inputWnd;
					_toolBox.Select("Controls");
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
					_spriteWnd.ZoomIn(zoom);

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
			}
			else if ((mod & Keys.Shift) != 0)
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
				}

				if (e.KeyCode == Keys.Tab)
				{
					if (SwitchToolBoxWindow() && !_toolBox.IsOpen)
						_toolBox.Minimise();
				}
			}
			else
			{
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
					Draw();
				}

				if (e.KeyCode == Keys.Tab)
				{
					if (!_toolBox.IsOpen)
						_toolBox.Minimise();
					else
					{
						_toolBox.Cycle(1);
						_toolBox.HandleTabClick();
					}

					this.PerformLayout();
				}
			}

			_keyHeld = true;
		}

		private void UpdateMinimumSize()
		{
			int minW = _inputWnd.ScrollYWidth + _spriteWnd.ScrollYWidth + (_toolBox.Minimum.Width * 2);

			this.MinimumSize = new Size(minW, this.MinimumSize.Height);
		}

		private void layoutHandler(object sender, LayoutEventArgs e)
		{
			int totalH = this.ClientSize.Height;
			int menuH = this.menuStrip1.Height;
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
			if (_toolBox.IsActive && tbWnd != null)
			{
				int wndMaxH = totalH - (menuH + tbWnd.ScrollXHeight);
				tbWnd.ReduceWindowTo(wndMaxH - _toolBox.Minimum.Height);
			}

			UpdateMinimumSize();

			_inputWnd.UpdateBars();
			_spriteWnd.UpdateBars();
			Draw();
		}

		public bool SwitchToolBoxWindow()
		{
			TileWindow wnd;
			if (_toolBox.CurrentWindow == _inputWnd)
				wnd = _spriteWnd;
			else if (_toolBox.CurrentWindow == _spriteWnd)
				wnd = _inputWnd;
			else
				return false;

			if (!wnd.IsActive)
				return false;

			_toolBox.CurrentWindow = wnd;
			this.PerformLayout();
			return true;
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
			_toolBox.IsActive = false;
			this.PerformLayout();
		}

		private void quit(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void editColourTable(object sender, EventArgs e)
		{
			var ctForm = new EditColourTable();
			ctForm.Show(this);
		}

		private void openFileDialog1FileOk(object sender, CancelEventArgs e)
		{
			_keyHeld = false; // the hackiest hack

			var kind = (FormatKind)(openFileDialog1.FilterIndex - 1);
			var data = File.ReadAllBytes(openFileDialog1.FileName);
			FileFormat fmt = _formatList[kind];

			closeWorkspace(null, null);

			this.colourTableToolStripMenuItem.Enabled = fmt.ColourTable.IsList;

			_inputWnd.Load(fmt, data);

			_spriteWnd.FormatToLoad = fmt;
			_spriteWnd.Prompt = "Drag or send a tile to begin!";

			_inputWnd.ToggleMenu(true);
			_spriteWnd.EnablePaste();

			_toolBox.Activate(_inputWnd);

			this.PerformLayout();
		}
	}
}
