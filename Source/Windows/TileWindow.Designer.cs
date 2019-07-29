using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	abstract partial class TileWindow
	{
		protected readonly Color _emptyBackColor = Color.FromArgb(200, 200, 200);

		protected PictureBox _window;
		protected HScrollBar _scrollX;
		protected VScrollBar _scrollY;
		protected ContextMenuStrip _menu;

		protected Label _prompt;
		public string Prompt
		{
			set {
				bool state = !String.IsNullOrEmpty(value);

				_prompt.Text = value;
				_prompt.Enabled = state;
				_prompt.Visible = state;
				AdjustPrompt();
			}
		}

		protected List<ITab> _tabs;

		public int TabCount { get { return _tabs.Count; } }

		public ITab this[int idx]
		{
			get {
				if (idx < 0 || idx >= _tabs.Count)
					return null;

				return _tabs[idx];
			}
		}
		public ITab this[string name]
		{
			get {
				foreach (ITab t in _tabs)
				{
					if (t.Name == name)
						return t;
				}

				return null;
			}
		}

		public Point CanvasPos { get { return _window.Location; } }
		public Size CanvasSize { get { return _window.Size; } }

		public int ScrollYWidth { get { return _scrollY.Width; } }
		public int ScrollXHeight { get { return _scrollX.Height; } }

		protected abstract void SetupWindowUI();
		protected abstract void InitialiseRightClickMenu(MainForm.TileAction copyTile, MainForm.TileAction pasteTile = null);
		protected abstract void InitialiseTabs();

		protected virtual void DisposeTabs() {}

		public void Dispose()
		{
			_window.Dispose();
			_scrollX.Dispose();
			_scrollY.Dispose();
			_menu.Dispose();
			DisposeTabs();
		}

		protected void InitialiseUI(MainForm main)
		{
			InitialiseTabs();

			_window = new PictureBox();
			_scrollX = new HScrollBar();
			_scrollY = new VScrollBar();
			_menu = new ContextMenuStrip();
			_prompt = new Label();

			((System.ComponentModel.ISupportInitialize)(_window)).BeginInit();

			_window.Resize += this.adjustWindowSize;
			_window.MouseWheel += this.windowScrollAction;

			_scrollX.Size = new Size(331, 17);
			_scrollX.Scroll += xScrollAction;

			_scrollY.Size = new Size(17, 518);
			_scrollY.Scroll += yScrollAction;

			_menu.Size = new Size(61, 4);
			InitialiseRightClickMenu(main.CopyTile, main.PasteTile);
			ToggleMenu(false);

			_prompt.Parent = _window;
			_prompt.AutoSize = true;
			_prompt.ForeColor = Color.Black;
			_prompt.BackColor = Color.Transparent;
			this.Prompt = "";

			SetupWindowUI();

			Close();

			main.Controls.Add(_window);
			main.Controls.Add(_scrollX);
			main.Controls.Add(_scrollY);

			((System.ComponentModel.ISupportInitialize)(_window)).EndInit();
		}

		public virtual void Activate()
		{
			this.Prompt = "";

			_window.BackColor = SystemColors.ControlDark;

			_scrollX.Visible = true;
			_scrollY.Visible = true;

			ToggleMenu(true);
			ToggleTabsContents(true);
		}

		public virtual void Close()
		{
			ToggleMenu(false);
			ToggleTabsContents(false);

			_cl = null;
			DeleteFrame();

			_scrollX.Visible = false;
			_scrollY.Visible = false;

			_window.BackColor = _emptyBackColor;
		}

		public void UpdateLayout(int x, int w, int totalH, int menuH)
		{
			w -= _scrollY.Width;

			//_window.SuspendLayout();
			_window.Location = new Point(x, menuH);
			_window.Size = new Size(w, totalH - (menuH + _scrollX.Height));
			//_window.ResumeLayout();

			_scrollX.Location = new Point(x, menuH + _window.Height);
			_scrollX.Size = new Size(w, _scrollX.Height);

			_scrollY.Location = new Point(x + w, menuH);
			_scrollY.Size = new Size(_scrollY.Width, _window.Height);

			AdjustPrompt();
		}

		private void AdjustPrompt()
		{
			if (!_prompt.Visible)
				return;

			_prompt.Location = new Point(
				(_window.Width - _prompt.Width) / 2,
				(_window.Height - _prompt.Height) / 2
			);
		}

		public void ReduceWindowTo(int maxH)
		{
			if (_window.Height <= maxH)
				return;

			_window.Size = new Size(
				_window.Width,
				maxH
			);
			_scrollX.Location = new Point(
				_window.Location.X,
				_window.Location.Y + maxH
			);
			_scrollY.Size = new Size(
				_scrollY.Width,
				maxH
			);
		}

		// Implements ITabCollection.ConfigureTabs
		public int TabIndex(ITab t)
		{
			return _tabs.IndexOf(t);
		}

		public void ToggleTabsContents(bool state)
		{
			foreach (ITab t in _tabs)
			{
				foreach (Control c in t.Panel.Controls)
					c.Visible = state;
			}
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
			return c == _window || c == _prompt;
		}

		public void ProvideTabButtons(ToolBox box)
		{
			foreach (ITab t in _tabs)
				box.AddTabButton(t.TabButton);
		}

		public void RescindTabButtons(ToolBox box)
		{
			foreach (ITab t in _tabs)
				box.RemoveTabButton(t.TabButton);
		}
	}
}
