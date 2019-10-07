using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SpriteWave
{
	public partial class InputWindow
	{
		public override void Activate()
		{
			_vis.col = _cl.Columns;
			AdjustWindow();
			base.Activate();
		}

		public override void Clear()
		{
			base.Clear();
			this.Prompt = "Open a file containing tiles to begin!";
		}

		protected override void SetupWindowUI()
		{
			_window.Name = "inputBox";
			_scrollX.Name = "inputScrollX";
			_scrollY.Name = "inputScrollY";
			_menu.Name = "inputMenu";

			_window.MouseLeave += (s, e) => Draw();
			_scrollX.Enabled = false;
		}

		protected override void InitializeTabs()
		{
			_tabs = new List<ITab>();
			_tabs.Add(new InputControlsTab(this));
		}

		protected override void InitializeRightClickMenu(MainForm.TileAction copyTile, MainForm.TileAction pasteTile = null)
		{
			_menu.Items.Add(new ToolStripMenuItem("Copy Tile", null, (s, e) => copyTile(this), "copyTile"));
		}
	}
}
