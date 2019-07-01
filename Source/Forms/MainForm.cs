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

		private ToolStripMenuItem _pasteTile;
		private DragObject _drag;

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

			this.openFileDialog1.Filter = Utils.FilterBuilder(_formatList);

			this.Layout += new LayoutEventHandler(this.layoutHandler);
			this.ActiveControl = this.inputBox;

			this.KeyPreview = true;
			this.KeyDown += new KeyEventHandler(this.keysHandler);

			Utils.ApplyRecursiveControlAction(this, this.SetMouseEventHandler);

			this.inputMenu.Items.Add("Copy Tile", null, (s, e) => CopyTile(_inputWnd));
			this.spriteMenu.Items.Add("Copy Tile", null, (s, e) => CopyTile(_spriteWnd));
			
			_pasteTile = new ToolStripMenuItem("Paste Tile", null, (s, e) => PasteTile(_spriteWnd));
			_pasteTile.Enabled = false;
			this.spriteMenu.Items.Add(_pasteTile);

			var initialSpriteMenu = new ContextMenuStrip();
			initialSpriteMenu.Items.Add(new ToolStripMenuItem("Paste Tile", null, (s, e) => PasteTile(_spriteWnd)));

			_inputWnd = new InputWindow();
			_inputWnd.InitialiseControlsTab();
			_inputWnd.Canvas = this.inputBox;
			_inputWnd.ScrollY = this.inputScroll;
			_inputWnd.Menu = this.inputMenu;
			_inputWnd.SendTileAction = (s, e) => {CopyTile(_inputWnd); PasteTile(_spriteWnd);};

			_spriteWnd = new SpriteWindow();
			_spriteWnd.InitialiseControlsTab();
			_spriteWnd.Canvas = this.spriteBox;
			_spriteWnd.ScrollX = this.spriteScrollX;
			_spriteWnd.ScrollY = this.spriteScrollY;
			_spriteWnd.InitialMenu = initialSpriteMenu;
			_spriteWnd.Menu = this.spriteMenu;

			_toolBox = new ToolBox(this.toolBoxTabs, _spriteWnd, this.toolBoxSwitchWindow, this.toolBoxMinimise);

			this.toolBoxMinimise.Click += (s, e) => {_toolBox.Minimise(); this.PerformLayout();};
			this.toolBoxSwitchWindow.Click += new EventHandler(this.switchToolBoxWindow);
		}

		private void layoutHandler(object sender, LayoutEventArgs e)
		{
			int totalH = this.ClientSize.Height;
			int menuH = this.menuStrip1.Size.Height;

			int availX = this.ClientSize.Width - (this.inputScroll.Size.Width + this.spriteScrollY.Size.Width);
			int tileBoxW = availX / 2;

			PictureBox tbWndBox = null;
			TileWindow tbWnd = _toolBox.ActiveWindow;
			var tbLayout = ToolBoxOrientation.None;
			if (tbWnd == _inputWnd)
			{
				tbWndBox = this.inputBox;
				tbLayout = ToolBoxOrientation.Left;
			}
			else if (tbWnd == _spriteWnd)
			{
				tbWndBox = this.spriteBox;
				tbLayout = ToolBoxOrientation.Right;
			}

			this.SuspendLayout();

			this.inputBox.Location = new Point(0, menuH);
			this.inputBox.Size = new Size(tileBoxW, totalH - menuH);

			this.spriteBox.Location = new Point(this.inputBox.Size.Width + this.inputScroll.Size.Width, menuH);
			this.spriteBox.Size = new Size(tileBoxW, totalH - (menuH + this.spriteScrollX.Size.Height));

			_toolBox.UpdateLayout(tbLayout, this.ClientSize);
			if (tbWndBox != null)
				tbWndBox.Size = new Size(tbWndBox.Size.Width, tbWndBox.Size.Height - this.toolBoxTabs.Size.Height);

			this.inputScroll.Location = new Point(tileBoxW, this.inputScroll.Location.Y);
			this.inputScroll.Size = new Size(this.inputScroll.Size.Width, this.inputBox.Size.Height);

			this.spriteScrollY.Location = new Point(this.spriteBox.Location.X + tileBoxW, menuH);
			this.spriteScrollY.Size = new Size(this.spriteScrollY.Size.Width, this.spriteBox.Size.Height);

			this.spriteScrollX.Location = new Point(this.spriteBox.Location.X, menuH + this.spriteBox.Size.Height);
			this.spriteScrollX.Size = new Size(tileBoxW, this.spriteScrollX.Size.Height);

			this.ResumeLayout();

			this.toolBoxTabs.Refresh();
			_inputWnd.UpdateBars();
			_spriteWnd.UpdateBars();
			Draw();
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
			_pasteTile.Enabled = true;
		}

		private void PasteTile(TileWindow wnd)
		{
			wnd.Selected = true;
			Transfer.Dest = wnd.CurrentSelection();
			Transfer.Paste();
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
			if (_drag != null)
				return;

			var ctrl = sender as Control;
			if (ctrl is PictureBox)
				this.ActiveControl = ctrl;

			TileWindow wnd = null;
			if (ctrl == this.inputBox)
				wnd = _inputWnd;
			else if (ctrl == this.spriteBox)
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
			if (curCtrl == this.inputBox)
			{
				wnd = _inputWnd;
			}
			else if (curCtrl == this.spriteBox)
			{
				wnd = _spriteWnd;
			}
			
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
			
			if (active is TextBox)
			{
				if (e.KeyCode == Keys.Escape)
					this.ActiveControl = this.inputBox;

				Draw();
				return;
			}

			if (e.KeyCode == Keys.Escape)
			{
				ClearSelection();
				_drag = null;
			}

			if (e.KeyCode == Keys.Delete)
			{
				_spriteWnd.Delete();
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
				/*
				if (e.KeyCode == Keys.G)
					this.ActiveControl = this.inputOffset;
				*/

				int zoom = 0;
				if (e.KeyCode == Keys.OemMinus)
					zoom = -1;
				else if (e.KeyCode == Keys.Oemplus)
					zoom = 1;

				if (zoom != 0)
					_spriteWnd.Zoom(zoom, this.spriteBox.Size.Width / 2, this.spriteBox.Size.Height / 2);

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
					_spriteWnd.Selected = true;
					Transfer.Source = _spriteWnd.CurrentSelection();
					Transfer.Start();

					_spriteWnd.MoveSelection(x, y);
					Transfer.Dest = _spriteWnd.CurrentSelection();
					Transfer.Swap();

					Draw();
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

		private void switchToolBoxWindow(object sender, EventArgs e)
		{
			if (_toolBox.ActiveWindow == _inputWnd)
				_toolBox.ActiveWindow = _spriteWnd;
			else if (_toolBox.ActiveWindow == _spriteWnd)
				_toolBox.ActiveWindow = _inputWnd;

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

			_spriteWnd.Close();
			_inputWnd.Load(fmt, data);
			_spriteWnd.FormatToLoad = fmt;
			//_spriteWnd.Prompt = "Drag or send a tile to begin!";

			Draw();
		}
	}
}
