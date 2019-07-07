using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	abstract partial class TileWindow
	{
		protected PictureBox _window;
		protected HScrollBar _scrollX;
		protected VScrollBar _scrollY;
		protected ContextMenuStrip _menu;
		protected TabPage _controlsTab;

		public TabPage ControlsTab { get { return _controlsTab; } }

		public Point CanvasPos { get { return _window.Location; } }
		public Size CanvasSize { get { return _window.Size; } }

		public int ScrollYWidth { get { return _scrollY.Size.Width; } }
		public int ScrollXHeight { get { return _scrollX.Size.Height; } }

		protected abstract void SetupWindowUI();
		protected abstract void InitialiseRightClickMenu(Utils.TileAction copyTile, Utils.TileAction pasteTile = null);
		protected abstract void InitialiseControlsTab();

		public void Dispose()
		{
			_window.Dispose();
			_scrollX.Dispose();
			_scrollY.Dispose();
			_menu.Dispose();
			_controlsTab.Dispose();
		}

		protected void InitialiseUI(MainForm main, Utils.TileAction copyTile, Utils.TileAction pasteTile = null)
		{
			InitialiseControlsTab();

			_window = new PictureBox();
			_scrollX = new HScrollBar();
			_scrollY = new VScrollBar();
			_menu = new ContextMenuStrip();

			((System.ComponentModel.ISupportInitialize)(_window)).BeginInit();

			_window.BackColor = System.Drawing.SystemColors.ControlDark;
			_window.Resize += this.adjustWindowSize;
			_window.MouseWheel += this.windowScrollAction;

			_scrollX.Size = new System.Drawing.Size(331, 17);
			_scrollX.Scroll += xScrollAction;

			_scrollY.Size = new System.Drawing.Size(17, 518);
			_scrollY.Scroll += yScrollAction;

			_menu.Size = new System.Drawing.Size(61, 4);
			InitialiseRightClickMenu(copyTile, pasteTile);
			ToggleMenu(false);

			SetupWindowUI();

			Close();

			main.Controls.Add(_window);
			main.Controls.Add(_scrollX);
			main.Controls.Add(_scrollY);

			((System.ComponentModel.ISupportInitialize)(_window)).EndInit();
		}

		public virtual void Activate()
		{
			_scrollX.Visible = true;
			_scrollY.Visible = true;

			ToggleMenu(true);
			ToggleContainer(_controlsTab, true);
		}

		public virtual void Close()
		{
			_scrollX.Visible = false;
			_scrollY.Visible = false;

			ToggleMenu(false);
			ToggleContainer(_controlsTab, false);

			_cl = null;
			DeleteFrame();
		}

		public void UpdateLayout(int x, int w, int totalH, int menuH)
		{
			w -= _scrollY.Size.Width;

			_window.SuspendLayout();
			_window.Location = new Point(x, menuH);
			_window.Size = new Size(w, totalH - (menuH + _scrollX.Size.Height));
			_window.ResumeLayout();

			_scrollX.Location = new Point(x, menuH + _window.Size.Height);
			_scrollX.Size = new Size(w, _scrollX.Size.Height);

			_scrollY.Location = new Point(x + w, menuH);
			_scrollY.Size = new Size(_scrollY.Size.Width, _window.Size.Height);
		}

		public void ReduceWindowTo(int maxH)
		{
			if (_window.Size.Height <= maxH)
				return;

			_window.Size = new Size(
				_window.Size.Width,
				maxH
			);
			_scrollX.Location = new Point(
				_window.Location.X,
				_window.Location.Y + maxH
			);
			_scrollY.Size = new Size(
				_scrollY.Size.Width,
				maxH
			);
		}

		public void ToggleContainer(Control cont, bool state)
		{
			foreach (Control c in cont.Controls)
				c.Visible = state;
		}

		public void ToggleMenu(bool state)
		{
			foreach (ToolStripItem item in _menu.Items)
				item.Visible = state;
		}

		public void EnablePaste()
		{
			int idx = _menu.Items.IndexOfKey("pasteTile");
			if (idx >= 0)
				_menu.Items[idx].Visible = true;
		}

		public void ShowMenu(int x, int y)
		{
			if ( _window == null)
				return;

			// If a piece has been copied to our clipboard, enable the "Paste Tile" option (if there is one)
			int pasteIdx = _menu.Items.IndexOfKey("pasteTile");
			if (pasteIdx >= 0)
				_menu.Items[pasteIdx].Enabled = Transfer.HasPiece;

			_menu.Show(_window, new Point(x, y));
		}

		public void Focus(MainForm main)
		{
			main.ActiveControl = _window;
		}
		public bool WindowIs(Control c)
		{
			return _window == c;
		}
	}
}
