﻿/*
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

		private MenuStrip menuStrip1;
		private ToolStripMenuItem fileToolStripMenuItem;
		private ToolStripMenuItem openBinaryToolStripMenuItem;
		private ToolStripMenuItem openNESFileStripMenuItem;
		private ToolStripMenuItem openSNESFileStripMenuItem;
		private ToolStripMenuItem openMDFileStripMenuItem;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripMenuItem closeToolStripMenuItem;
		private ToolStripMenuItem quitToolStripMenuItem;
		private OpenFileDialog openFileDialog1;
		private ToolStripMenuItem copyTileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem colorTableToolStripMenuItem;

		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null)
					components.Dispose();

				inputWnd.Dispose();
				spriteWnd.Dispose();
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
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openBinaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openNESFileStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openSNESFileStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openMDFileStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.colorTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.copyTileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.fileToolStripMenuItem,
			this.editToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(704, 24);
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.openBinaryToolStripMenuItem,
			this.openNESFileStripMenuItem,
			this.openSNESFileStripMenuItem,
			this.openMDFileStripMenuItem,
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
			this.openBinaryToolStripMenuItem.Click += (s, e) => openFileDialog1.ShowDialog();
			// 
			// openNESFileStripMenuItem
			// 
			this.openNESFileStripMenuItem.Name = "openNESFileStripMenuItem";
			this.openNESFileStripMenuItem.Size = new System.Drawing.Size(209, 22);
			this.openNESFileStripMenuItem.Text = "Open NES ROM";
			this.openNESFileStripMenuItem.Click += (s, e) => openRom(FormatKind.NES);
			// 
			// openSNESFileStripMenuItem
			// 
			this.openSNESFileStripMenuItem.Name = "openSNESFileStripMenuItem";
			this.openSNESFileStripMenuItem.Size = new System.Drawing.Size(209, 22);
			this.openSNESFileStripMenuItem.Text = "Open SNES ROM";
			this.openSNESFileStripMenuItem.Click += (s, e) => openRom(FormatKind.SNES);
			// 
			// openMDFileStripMenuItem
			// 
			this.openMDFileStripMenuItem.Name = "openMDFileStripMenuItem";
			this.openMDFileStripMenuItem.Size = new System.Drawing.Size(209, 22);
			this.openMDFileStripMenuItem.Text = "Open Genesis ROM";
			this.openMDFileStripMenuItem.Click += (s, e) => openRom(FormatKind.MD);
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
			// editToolStripMenuItem
			// 
			this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.colorTableToolStripMenuItem});
			this.editToolStripMenuItem.Name = "editToolStripMenuItem";
			this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
			this.editToolStripMenuItem.Text = "Edit";
			// 
			// colorTableToolStripMenuItem
			// 
			this.colorTableToolStripMenuItem.Enabled = false;
			this.colorTableToolStripMenuItem.Name = "colorTableToolStripMenuItem";
			this.colorTableToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
			this.colorTableToolStripMenuItem.Text = "Color Table";
			this.colorTableToolStripMenuItem.Click += new System.EventHandler(this.editColorTable);
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
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(704, 601);
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
