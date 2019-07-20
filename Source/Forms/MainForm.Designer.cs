/*
 * Created by SharpDevelop.
 * User: 101111482
 * Date: 28/04/2019
 * Time: 9:48 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace SpriteWave
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private IContainer components = null;

		private TabControl toolBoxTabs;
		private Button toolBoxMinimise;
		private Button toolBoxSwitchWindow;

		private MenuStrip menuStrip1;
		private ToolStripMenuItem fileToolStripMenuItem;
		private ToolStripMenuItem openBinaryToolStripMenuItem;
		private ToolStripMenuItem openNESFileStripMenuItem;
		private ToolStripMenuItem openSNESFileStripMenuItem;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripMenuItem closeToolStripMenuItem;
		private ToolStripMenuItem quitToolStripMenuItem;
		private OpenFileDialog openFileDialog1;
		private ToolStripMenuItem copyTileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem colourTableToolStripMenuItem;

		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null)
					components.Dispose();

				_inputWnd.Dispose();
				_spriteWnd.Dispose();
			}

			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.toolBoxTabs = new System.Windows.Forms.TabControl();
			this.toolBoxMinimise = new System.Windows.Forms.Button();
			this.toolBoxSwitchWindow = new System.Windows.Forms.Button();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openBinaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openNESFileStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openSNESFileStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.copyTileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.colourTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolBoxTabs
			// 
			this.toolBoxTabs.Alignment = System.Windows.Forms.TabAlignment.Bottom;
			this.toolBoxTabs.Location = new System.Drawing.Point(376, 313);
			this.toolBoxTabs.Multiline = true;
			this.toolBoxTabs.Name = "toolBoxTabs";
			this.toolBoxTabs.SelectedIndex = 0;
			this.toolBoxTabs.Size = new System.Drawing.Size(324, 229);
			this.toolBoxTabs.TabIndex = 5;
			// 
			// toolBoxMinimise
			// 
			this.toolBoxMinimise.BackColor = System.Drawing.SystemColors.ControlLight;
			this.toolBoxMinimise.FlatAppearance.BorderSize = 0;
			this.toolBoxMinimise.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.toolBoxMinimise.Location = new System.Drawing.Point(660, 526);
			this.toolBoxMinimise.Name = "toolBoxMinimise";
			this.toolBoxMinimise.Size = new System.Drawing.Size(40, 21);
			this.toolBoxMinimise.TabIndex = 6;
			this.toolBoxMinimise.UseVisualStyleBackColor = false;
			// 
			// toolBoxSwitchWindow
			// 
			this.toolBoxSwitchWindow.BackColor = System.Drawing.SystemColors.ControlLight;
			this.toolBoxSwitchWindow.FlatAppearance.BorderSize = 0;
			this.toolBoxSwitchWindow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.toolBoxSwitchWindow.Location = new System.Drawing.Point(352, 334);
			this.toolBoxSwitchWindow.Name = "toolBoxSwitchWindow";
			this.toolBoxSwitchWindow.Size = new System.Drawing.Size(20, 208);
			this.toolBoxSwitchWindow.TabIndex = 7;
			this.toolBoxSwitchWindow.Text = "<";
			this.toolBoxSwitchWindow.UseVisualStyleBackColor = false;
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.fileToolStripMenuItem,
			this.editToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(704, 24);
			this.menuStrip1.TabIndex = 4;
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.openBinaryToolStripMenuItem,
			this.openNESFileStripMenuItem,
			this.openSNESFileStripMenuItem,
			this.toolStripSeparator1,
			this.closeToolStripMenuItem,
			this.quitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// openBinaryToolStripMenuItem
			// 
			this.openBinaryToolStripMenuItem.Name = "openBinaryToolStripMenuItem";
			this.openBinaryToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+O";
			this.openBinaryToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.openBinaryToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
			this.openBinaryToolStripMenuItem.Text = "Open Binary";
			this.openBinaryToolStripMenuItem.Click += new System.EventHandler(this.openBinary);
			// 
			// openNESFileStripMenuItem
			// 
			this.openNESFileStripMenuItem.Name = "openNESFileStripMenuItem";
			this.openNESFileStripMenuItem.Size = new System.Drawing.Size(209, 22);
			this.openNESFileStripMenuItem.Text = "Open NES File";
			this.openNESFileStripMenuItem.Click += new System.EventHandler(this.openNES);
			// 
			// openSNESFileStripMenuItem
			// 
			this.openSNESFileStripMenuItem.Name = "openSNESFileStripMenuItem";
			this.openSNESFileStripMenuItem.Size = new System.Drawing.Size(209, 22);
			this.openSNESFileStripMenuItem.Text = "Open SNES File";
			this.openSNESFileStripMenuItem.Click += new System.EventHandler(this.openSNES);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(206, 6);
			// 
			// closeToolStripMenuItem
			// 
			this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
			this.closeToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+W";
			this.closeToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
			this.closeToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
			this.closeToolStripMenuItem.Text = "Close Workspace";
			this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeWorkspace);
			// 
			// quitToolStripMenuItem
			// 
			this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
			this.quitToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
			this.quitToolStripMenuItem.Text = "Quit";
			this.quitToolStripMenuItem.Click += new System.EventHandler(this.quit);
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.Title = "Open tiles file";
			this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog1FileOk);
			// 
			// copyTileToolStripMenuItem
			// 
			this.copyTileToolStripMenuItem.Name = "copyTileToolStripMenuItem";
			this.copyTileToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
			// 
			// editToolStripMenuItem
			// 
			this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.colourTableToolStripMenuItem});
			this.editToolStripMenuItem.Name = "editToolStripMenuItem";
			this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
			this.editToolStripMenuItem.Text = "Edit";
			// 
			// colourTableToolStripMenuItem
			// 
			this.colourTableToolStripMenuItem.Name = "colourTableToolStripMenuItem";
			this.colourTableToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.colourTableToolStripMenuItem.Text = "Colour Table";
			this.colourTableToolStripMenuItem.Click += new System.EventHandler(this.editColourTable);
			this.colourTableToolStripMenuItem.Enabled = false;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(704, 601);
			this.Controls.Add(this.toolBoxSwitchWindow);
			this.Controls.Add(this.toolBoxMinimise);
			this.Controls.Add(this.toolBoxTabs);
			this.Controls.Add(this.menuStrip1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStrip1;
			this.MinimumSize = new System.Drawing.Size(200, 400);
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "SpriteWave";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
