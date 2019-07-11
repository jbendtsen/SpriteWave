using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public partial class InputWindow
	{
		protected override void SetupWindowUI()
		{
			_window.Name = "inputBox";
			_scrollX.Name = "inputScrollX";
			_scrollY.Name = "inputScrollY";
			_menu.Name = "inputMenu";

			_scrollX.Enabled = false;
		}

		protected override void InitialiseControlsTab(MainForm.GrowWindowDelegate growForm)
		{
			_controlsTab = new InputControlsTab(this, growForm);
		}

		protected override void InitialiseRightClickMenu(MainForm.TileAction copyTile, MainForm.TileAction pasteTile = null)
		{
			_menu.Items.Add(new ToolStripMenuItem("Copy Tile", null, (s, e) => copyTile(this), "copyTile"));
		}
	}
}
