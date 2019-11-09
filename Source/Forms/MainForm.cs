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
		public InputWindow inputWnd;
		public SpriteWindow spriteWnd;
		public ToolBox toolBox;

		private Dictionary<FormatKind, FileFormat> _formatList;

		private DragObject _drag;
		private bool _tempSpriteSel = false;

		private EventPair[] _sendTileEvents;

		public delegate void TileAction(TileWindow tw);

		public MainForm()
		{
			_formatList = new Dictionary<FormatKind, FileFormat>();

			_formatList[FormatKind.NES] = new FileFormat(
				"NES",
				Utils.TileType("NESTile"),
				new string[] { "nes", "fds", "chr", "bin" },
				new ColorList(Utils.NESPalette, Utils.NESDefSel)
			);

			_formatList[FormatKind.SNES] = new FileFormat(
				"SNES",
				Utils.TileType("SNESTile"),
				new string[] { "smc", "sfc", "chr", "bin" },
				new ColorPattern(Utils.SNESRGBAOrderAndDepth, Utils.SNESDefSel)
			);

			_formatList[FormatKind.MD] = new FileFormat(
				"Genesis",
				Utils.TileType("MDTile"),
				new string[] { "smd", "md", "bin" },
				new ColorPattern(Utils.MDRGBAOrderAndDepth, Utils.MDDefSel)
			);

			InitializeComponent();

			string filter = "";
			foreach (var fmt in _formatList)
				filter += fmt.Value.Filter;

			filter = filter.Remove(filter.Length-1);
			this.openFileDialog1.Filter = filter;

			inputWnd = new InputWindow(this);
			spriteWnd = new SpriteWindow(this);
			toolBox = new ToolBox(this, inputWnd);

			// Setup MainForm events
			this.KeyPreview = true;
			this.KeyUp += this.keyUpHandler;
			this.KeyDown += this.keysHandler;
			this.Resize += this.catchWindowState;
			this.Layout += this.layoutHandler;
			Utils.ApplyRecursiveControlFunc(this, this.ConfigureControls);

			_sendTileEvents = new[]
			{
				new EventPair("Click", (s, e) => { CopyTile(inputWnd); PasteTile(spriteWnd); _tempSpriteSel = true; }),
				new EventPair("MouseEnter", (s, e) => { _tempSpriteSel = spriteWnd.Selected; spriteWnd.Selected = true; spriteWnd.Draw(); }),
				new EventPair("MouseLeave", (s, e) => { spriteWnd.Selected = _tempSpriteSel; spriteWnd.Draw(); })
			};

			UpdateMinimumSize();
			inputWnd.Focus(this);
		}

		private void Draw()
		{
			inputWnd.Draw();
			spriteWnd.Draw();
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

			if (wnd == spriteWnd && Transfer.Completed)
				toolBox.Switch = true;

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
			inputWnd.Selected = false;
			spriteWnd.Selected = false;
			Transfer.Clear();
		}

		private void SetSample(Tile t)
		{
			inputWnd.TileSample = t;
			this.PerformLayout();
		}

		public object ConfigureControls(Control ctrl, object args)
		{
			ctrl.TabStop = false;
			ctrl.MouseDown += this.mouseDownHandler;
			ctrl.MouseMove += this.mouseMoveHandler;
			ctrl.MouseUp   += this.mouseUpHandler;
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

			if (_drag != null && wnd == inputWnd)
			{
				Tile t = _drag.Current().Piece as Tile;
				if (t != null)
					SetSample(t);
			}
		}

		private void ShowMenuAt(TileWindow wnd, int x, int y)
		{
			bool wasOob;
			wnd.Position = wnd.GetPosition(x, y, out wasOob);
			if (wasOob) {
				ClearSelection();
				SetSample(null);
				return;
			}

			if (wnd == inputWnd)
				SetSample(inputWnd.PieceAt(inputWnd.Position) as Tile);

			Transfer.Source = wnd.CurrentSelection();
			if (wnd.EdgeOf(wnd.Position) == EdgeKind.None)
				wnd.ShowMenu(x, y);
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
			var curCtrl = Utils.ApplyRecursiveControlFunc(this, FindControlWithMouse) as Control;

			TileWindow wnd = null;
			if (inputWnd.WindowIs(curCtrl))
				wnd = inputWnd;
			else if (spriteWnd.WindowIs(curCtrl))
				wnd = spriteWnd;

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
				if (wnd == spriteWnd)
					changed = spriteWnd.HighlightEdgeAt(x, y);
				else
					changed = spriteWnd.ClearMousedEdge();
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
				toolBox.RefreshTab();
				this.PerformLayout();
			}
			else if (_drag == null)
			{
				if (ctrl is PictureBox)
					this.ActiveControl = ctrl;
	
				TileWindow wnd = null;
				if (inputWnd.WindowIs(ctrl))
					wnd = inputWnd;
				else if (spriteWnd.WindowIs(ctrl))
					wnd = spriteWnd;
	
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
					else if (Transfer.Dest.Window == spriteWnd)
						toolBox.Switch = true;

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
					if (Transfer.Source != null || Transfer.Dest != null)
					{
						ClearSelection();
						SetSample(null);
					}
					else if (toolBox.IsOpen && e.KeyCode == Keys.Escape)
					{
						if (!toolBox.HandleEscapeKey(this))
						{
							toolBox.Minimise();
							this.PerformLayout();
						}
					}
				}

				inputWnd.Focus(this);
			}

			if (active is TextBox)
			{
				Draw();
				_keyHeld = true;
				return;
			}

			if (e.KeyCode == Keys.Delete)
			{
				spriteWnd.EraseTile();
			}

			if (e.KeyCode == Keys.Enter)
			{
				CopyTile(inputWnd);
				PasteTile(spriteWnd);
				spriteWnd.MoveSelection(1, 0);
				spriteWnd.Draw();
			}

			Keys mod = Control.ModifierKeys;

			if ((mod & Keys.Control) != 0)
			{
				if (e.KeyCode == Keys.G && inputWnd.IsActive)
				{
					toolBox.CurrentWindow = inputWnd;
					toolBox.Select("Controls");
					if (!toolBox.IsOpen)
						toolBox.Minimise();

					spriteWnd.Centre();
					this.PerformLayout();
					this.ActiveControl = toolBox.GetControl("inputOffset");
				}

				if (e.KeyCode == Keys.D0)
					spriteWnd.Centre();

				int zoom = 0;
				if (e.KeyCode == Keys.OemMinus)
					zoom = -1;
				else if (e.KeyCode == Keys.Oemplus)
					zoom = 1;

				if (zoom != 0)
					spriteWnd.ZoomIn(zoom);

				Action<EdgeKind> moveEdge = spriteWnd.InsertEdge;
				if ((mod & Keys.Shift) != 0)
					moveEdge = spriteWnd.DeleteEdge;

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
					CopyTile(spriteWnd);
					spriteWnd.MoveSelection(x, y);
					SwapTile(spriteWnd);
				}

				if (e.KeyCode == Keys.Tab)
				{
					if (!toolBox.IsOpen)
						toolBox.Minimise();

					SwitchToolBoxWindow();
				}
			}
			else
			{
				bool move = true;

				if (e.KeyCode == Keys.W)
					inputWnd.MoveSelection(0, -1);
				else if (e.KeyCode == Keys.S)
					inputWnd.MoveSelection(0, 1);
				else if (e.KeyCode == Keys.A)
					inputWnd.MoveSelection(-1, 0);
				else if (e.KeyCode == Keys.D)
					inputWnd.MoveSelection(1, 0);

				else if (e.KeyCode == Keys.I)
					spriteWnd.MoveSelection(0, -1);
				else if (e.KeyCode == Keys.K)
					spriteWnd.MoveSelection(0, 1);
				else if (e.KeyCode == Keys.J)
					spriteWnd.MoveSelection(-1, 0);
				else if (e.KeyCode == Keys.L)
					spriteWnd.MoveSelection(1, 0);

				else
					move = false;

				if (move)
				{
					Transfer.Source = inputWnd.CurrentSelection();
					Transfer.Dest = spriteWnd.CurrentSelection();
					Draw();
				}

				if (e.KeyCode == Keys.Tab)
				{
					if (!toolBox.IsOpen)
						toolBox.Minimise();
					else
					{
						toolBox.Cycle(1);
						toolBox.RefreshTab();
						spriteWnd.Centre();
					}

					this.PerformLayout();
				}
			}

			_keyHeld = true;
		}

		/*
			Thank you StackOverflow!
			https://stackoverflow.com/a/16626928
		*/
		FormWindowState _prevState = FormWindowState.Minimized;
		private void catchWindowState(object sender, EventArgs e)
		{
			if (this.WindowState != _prevState)
			{
				_prevState = this.WindowState;
				spriteWnd.Centre();
				spriteWnd.Draw();
			}
		}

		private void UpdateMinimumSize()
		{
			int minW = inputWnd.ScrollYWidth + spriteWnd.ScrollYWidth + (toolBox.Minimum.Width * 2);

			this.MinimumSize = new Size(minW, this.MinimumSize.Height);
		}

		private void layoutHandler(object sender, LayoutEventArgs e)
		{
			int totalH = this.ClientSize.Height;
			int menuH = this.menuStrip1.Height;
			int tileBoxW = this.ClientSize.Width / 2;

			var tbLayout = ToolBoxOrientation.None;

			TileWindow tbWnd = toolBox.CurrentWindow;
			if (tbWnd == inputWnd)
				tbLayout = ToolBoxOrientation.Left;
			else if (tbWnd == spriteWnd)
				tbLayout = ToolBoxOrientation.Right;

			inputWnd.UpdateLayout(0, tileBoxW, totalH, menuH);
			spriteWnd.UpdateLayout(tileBoxW, tileBoxW, totalH, menuH);

			toolBox.UpdateLayout(tbLayout, this.ClientSize);
			if (toolBox.IsActive && tbWnd != null)
			{
				int wndMaxH = totalH - (menuH + tbWnd.ScrollXHeight);
				tbWnd.ReduceWindowTo(wndMaxH - toolBox.Minimum.Height);
			}

			UpdateMinimumSize();

			inputWnd.UpdateBars();
			spriteWnd.UpdateBars();
			Draw();
		}

		public bool SwitchToolBoxWindow()
		{
			TileWindow wnd;
			if (toolBox.CurrentWindow == inputWnd)
				wnd = spriteWnd;
			else if (toolBox.CurrentWindow == spriteWnd)
				wnd = inputWnd;
			else
				return false;

			if (!wnd.IsActive)
				return false;

			toolBox.CurrentWindow = wnd;
			this.PerformLayout();

			spriteWnd.Centre();
			spriteWnd.Draw();

			return true;
		}

		private void openRom(FormatKind format)
		{
			openFileDialog1.FilterIndex = (int)format + 1;
			openFileDialog1.ShowDialog();
		}

		private void closeWorkspace(object sender, EventArgs e)
		{
			Transfer.Clear();
			inputWnd.Close(this);
			spriteWnd.Close(this);
			toolBox.IsActive = false;
			this.PerformLayout();
		}

		private void quit(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void editColorTable(object sender, EventArgs e)
		{
			var pal = inputWnd.Collage.Format.ColorTable as IPalette;
			if (pal != null)
				new ColorPicker(this, 256, pal, -1).Show(this);
		}

		private void openFileDialog1FileOk(object sender, CancelEventArgs e)
		{
			_keyHeld = false; // the hackiest hack

			var kind = (FormatKind)(openFileDialog1.FilterIndex - 1);
			var data = File.ReadAllBytes(openFileDialog1.FileName);
			FileFormat fmt = _formatList[kind];

			closeWorkspace(null, null);

			this.colorTableToolStripMenuItem.Enabled = fmt.ColorTable is ColorList;

			inputWnd.Load(this, fmt, data);
			Control c = Utils.FindControl(inputWnd["Controls"].Panel, "inputSend");
			Utils.ApplyEvents(c, _sendTileEvents);

			spriteWnd.FormatToLoad = fmt;
			spriteWnd.Prompt = "Drag or send a tile to begin!";

			inputWnd.ToggleMenu(true);
			spriteWnd.EnablePaste();

			toolBox.Activate(inputWnd);

			this.PerformLayout();
		}

		private ColorPicker _picker;
		public void OpenColorPicker(IPalette pal, int palIdx)
		{
			if (pal == null)
				return;

			if (_picker == null)
			{
				_picker = new ColorPicker(this, 256, pal, palIdx);
				_picker.Show(this);
			}
			else
			{
				_picker.SelectColorFrom(pal, palIdx);
				if (_picker.WindowState == FormWindowState.Minimized)
					_picker.WindowState = FormWindowState.Normal;
			}
		}
		public void ClearColorPicker()
		{
			_picker = null;
			(toolBox["Palette"] as PaletteTab).HandleEscapeKey(this);
		}
	}
}
