﻿using System;
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

		protected override void InitialiseControlsTab()
		{
			_controlsTab = new InputControlsTab(this);
		}

		protected override void InitialiseRightClickMenu(Utils.TileAction copyTile, Utils.TileAction pasteTile = null)
		{
			_menu.Items.Add(new ToolStripMenuItem("Copy Tile", null, (s, e) => copyTile(this), "copyTile"));
		}
	}
}