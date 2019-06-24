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
		private Dictionary<FormatKind, FileFormat> _formatList;

		private ToolStripMenuItem _pasteTile;
		private DragObject _drag;

		private FormWindowState _prevWndState = FormWindowState.Minimized;

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

			//this.Resize += new EventHandler(this.resizeRefreshHack);
			this.Layout += new LayoutEventHandler(this.layoutHandler);
			this.ActiveControl = this.inputBox;

			this.KeyPreview = true;
			this.KeyDown += new KeyEventHandler(this.keysHandler);

			Utils.ApplyRecursiveControlAction(this, this.SetMouseEventHandler);

			this.inputSend.Click += new EventHandler((s, e) => {CopyTile(_inputWnd); PasteTile(_spriteWnd);});

			inputMenu.Items.Add("Copy Tile", null, (s, e) => CopyTile(_inputWnd));
			spriteMenu.Items.Add("Copy Tile", null, (s, e) => CopyTile(_spriteWnd));
			
			_pasteTile = new ToolStripMenuItem("Paste Tile", null, (s, e) => PasteTile(_spriteWnd));
			_pasteTile.Enabled = false;
			spriteMenu.Items.Add(_pasteTile);

			var _initialSpriteMenu = new ContextMenuStrip();
			_initialSpriteMenu.Items.Add(new ToolStripMenuItem("Paste Tile", null, (s, e) => PasteTile(_spriteWnd)));

			_inputWnd = new InputWindow();
			_inputWnd.Canvas = this.inputBox;
			_inputWnd.ScrollY = this.inputScroll;
			_inputWnd.Menu = this.inputMenu;
			_inputWnd.Panel = this.inputPanel;

			_spriteWnd = new SpriteWindow();
			_spriteWnd.Canvas = this.spriteBox;
			_spriteWnd.ScrollX = this.spriteScrollX;
			_spriteWnd.ScrollY = this.spriteScrollY;
			_spriteWnd.InitialMenu = _initialSpriteMenu;
			_spriteWnd.Menu = this.spriteMenu;
			_spriteWnd.Panel = this.spritePanel;
		}

		private int Centre(int cont, int obj)
		{
			return (cont - obj) / 2;
		}

		private void layoutHandler(object sender, LayoutEventArgs e)
		{
			int totalH = this.ClientSize.Height;
			int menuH = this.menuStrip1.Size.Height;

			int availX = this.ClientSize.Width - (this.inputScroll.Size.Width + this.spriteScrollY.Size.Width);

			int availInputY = totalH - (menuH + this.inputPanel.Size.Height);
			int availSpriteY = totalH - (menuH + this.spritePanel.Size.Height + this.spriteScrollX.Size.Height);

			int inputBoxW = availX / 2;
			int spriteBoxW = availX / 2;

			Func<int, int, int> centre = (cont, obj) => (cont - obj) / 2;

			this.SuspendLayout();

			this.inputBox.Size = new Size(inputBoxW, availInputY);
			this.spriteBox.Size = new Size(spriteBoxW, availSpriteY);

			this.inputScroll.Location = new Point(inputBoxW, this.inputScroll.Location.Y);
			this.inputScroll.Size = new Size(this.inputScroll.Size.Width, availInputY);

			this.inputPanel.Location = new Point(0, menuH + availInputY);
			this.inputPanel.Size = new Size(inputBoxW + this.inputScroll.Size.Width, this.inputPanel.Size.Height);

			this.spriteBox.Location = new Point(inputBoxW + this.inputScroll.Size.Width, menuH);

			this.spriteScrollY.Location = new Point(this.spriteBox.Location.X + spriteBoxW, menuH);
			this.spriteScrollY.Size = new Size(this.spriteScrollY.Size.Width, availSpriteY);

			this.spriteScrollX.Location = new Point(this.spriteBox.Location.X, menuH + availSpriteY);
			this.spriteScrollX.Size = new Size(spriteBoxW, this.spriteScrollX.Size.Height);
			
			this.spritePanel.Location = new Point(this.spriteBox.Location.X, this.spriteScrollX.Location.Y + this.spriteScrollX.Size.Height);
			this.spritePanel.Size = new Size(spriteBoxW + this.spriteScrollY.Size.Width, this.spritePanel.Size.Height);

			this.inputOffsetLabel.Location = new Point(this.inputOffsetLabel.Location.X, centre(this.inputPanel.Size.Height, this.inputOffsetLabel.Size.Height));
			this.inputOffset.Location = new Point(this.inputOffset.Location.X, centre(this.inputPanel.Size.Height, this.inputOffset.Size.Height));
			this.inputSizeLabel.Location = new Point(this.inputSizeLabel.Location.X, centre(this.inputPanel.Size.Height, this.inputSizeLabel.Size.Height));

			this.inputSend.Location = new Point(this.inputPanel.Size.Width - 160, centre(this.inputPanel.Size.Height, this.inputSend.Size.Height));
			this.inputSample.Location = new Point(this.inputPanel.Size.Width - 60, centre(this.inputPanel.Size.Height, this.inputSample.Size.Height));
			
			this.ResumeLayout();

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
			wnd.Selection = wnd;
			Transfer.Source = wnd;
			Transfer.Start();
			_pasteTile.Enabled = true;
		}

		private void PasteTile(TileWindow wnd)
		{
			wnd.Selection = wnd;
			Transfer.Dest = wnd;
			Transfer.Paste();
			Draw();
		}

		private void ClearSelection()
		{
			_inputWnd.Selection = null;
			_spriteWnd.Selection = null;
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
				wnd.Location = wnd.GetPosition(x, y);
				Transfer.Source = wnd;
				if (wnd.EdgeOf(wnd.Location) == EdgeKind.None)
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
				Transfer.Start();
				_spriteWnd.Selection = _spriteWnd;
				Transfer.Dest = _spriteWnd;
				Transfer.Paste();
				_spriteWnd.MoveSelection(1, 0);
			}

			Keys mod = Control.ModifierKeys;

			if ((mod & Keys.Control) != 0)
			{
				if (e.KeyCode == Keys.G)
					this.ActiveControl = this.inputOffset;

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
					_spriteWnd.Selection = _spriteWnd;
					Transfer.Source = _spriteWnd;
					Transfer.Start();

					_spriteWnd.MoveSelection(x, y);
					Transfer.Dest = _spriteWnd;
					Transfer.Paste();

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
				Transfer.Source = _inputWnd;
				Transfer.Dest = _spriteWnd;
			}

			Draw();
		}

		/*
			It seems like if you go too many levels deep into SplitContainer-ception,
			the resize/paint chain doesn't complete when maximising, minimising, etc.
			Thanks StackOverflow!
			- Problem: https://stackoverflow.com/questions/6644213/winform-splitcontainers-redraw-issue
			- Solution: https://stackoverflow.com/a/16626928
			And yes, technically checking for form state changes is not required,
			but if I don't include it then I end up with unnecessary redrawing (decent performance penalty)
		*/
		private void resizeRefreshHack(object sender, EventArgs e)
		{
			FormWindowState state = this.WindowState;
			if (state != _prevWndState)
			{
				_prevWndState = state;
				Draw();
			}
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

		private void exportSprite(object sender, EventArgs e)
		{

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
			_spriteWnd.Prompt = "Drag or send a tile to begin!";

			Draw();
		}
	}
}
