using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public partial class SpriteWindow
	{
		protected override void SetupWindowUI()
		{
			_window.Name = "spriteBox";
			_scrollX.Name = "spriteScrollX";
			_scrollY.Name = "spriteScrollY";
			_menu.Name = "spriteMenu";
		}

		protected override void InitialiseControlsTab(MainForm.GrowWindowDelegate growForm)
		{
			_controlsTab = new SpriteControlsTab(this, growForm);
		}

		protected override void InitialiseRightClickMenu(MainForm.TileAction copyTile, MainForm.TileAction pasteTile = null)
		{
			_menu.Items.Add(new ToolStripMenuItem("Copy Tile", null, (s, e) => copyTile(this), "copyTile"));
			_menu.Items.Add(new ToolStripMenuItem("Paste Tile", null, (s, e) => pasteTile(this), "pasteTile"));

			_menu.Items.Add(new ToolStripMenuItem("Erase Tile", null, (s, e) => EraseTile(), "eraseTile"));
			_menu.Items.Add(new ToolStripSeparator());

			_menu.Items.Add(
				new ToolStripMenuItem(
					"Rotate Tile", null, new ToolStripMenuItem[] {
						new ToolStripMenuItem("Left", null, (s, e) => FlipTile(Translation.Left), "rotateLeft"),
						new ToolStripMenuItem("Right", null, (s, e) => FlipTile(Translation.Right), "rotateRight")
					}
				)
			);
			_menu.Items.Add(
				new ToolStripMenuItem(
					"Mirror Tile", null, new ToolStripMenuItem[] {
						new ToolStripMenuItem("Horizontally", null, (s, e) => FlipTile(Translation.Horizontal), "mirrorHori"),
						new ToolStripMenuItem("Vertically", null, (s, e) => FlipTile(Translation.Vertical), "mirrorVert")
					}
				)
			);
			_menu.Items.Add(new ToolStripSeparator());

			_menu.Items.Add(
				new ToolStripMenuItem(
					"Insert", null, new ToolStripMenuItem[] {
						new ToolStripMenuItem("Column Left", null, (s, e) => InsertCollageColumn(_selPos.col), "insertLeft"),
						new ToolStripMenuItem("Column Right", null, (s, e) => InsertCollageColumn(_selPos.col+1), "insertRight"),
						new ToolStripMenuItem("Row Above", null, (s, e) => InsertCollageRow(_selPos.row), "insertAbove"),
						new ToolStripMenuItem("Row Below", null, (s, e) => InsertCollageRow(_selPos.row+1), "insertBelow")
					}
				)
			);

			_menu.Items.Add(
				new ToolStripMenuItem(
					"Delete", null, new ToolStripMenuItem[] {
						new ToolStripMenuItem("Column", null, (s, e) => DeleteCollageColumn(_selPos.col), "deleteCol"),
						new ToolStripMenuItem("Row", null, (s, e) => DeleteCollageRow(_selPos.row), "deleteRow")
					}
				)
			);
		}
	}
}
