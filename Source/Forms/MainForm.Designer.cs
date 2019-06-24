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

		private PictureBox inputBox;
		private VScrollBar inputScroll;
		private ContextMenuStrip inputMenu;
		private Panel inputPanel;
		private Label inputOffsetLabel;
		private TextBox inputOffset;
		private Label inputSizeLabel;
		private PictureBox inputSample;
		private Button inputSend;

		private PictureBox spriteBox;
		private HScrollBar spriteScrollX;
		private VScrollBar spriteScrollY;
		private ContextMenuStrip spriteMenu;
		private Panel spritePanel;
		private Label spritePrompt;

		private MenuStrip menuStrip1;
		private ToolStripMenuItem fileToolStripMenuItem;
		private ToolStripMenuItem openBinaryToolStripMenuItem;
		private ToolStripMenuItem openNESFileStripMenuItem;
		private ToolStripMenuItem openSNESFileStripMenuItem;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripMenuItem closeToolStripMenuItem;
		private ToolStripMenuItem quitToolStripMenuItem;
		private OpenFileDialog openFileDialog1;
		private System.Windows.Forms.TextBox palette1;
		private System.Windows.Forms.Button mirrorVert;
		private System.Windows.Forms.Button mirrorHori;
		private System.Windows.Forms.Button rotateRight;
		private System.Windows.Forms.Button rotateLeft;
		private System.Windows.Forms.TextBox palette4;
		private System.Windows.Forms.TextBox palette3;
		private System.Windows.Forms.TextBox palette2;
		private System.Windows.Forms.Button spriteSave;
		private System.Windows.Forms.TextBox spriteName;
		private System.Windows.Forms.ToolStripMenuItem copyTileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem inputPaletteToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem spritePaletteToolStripMenuItem;

		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.inputBox = new System.Windows.Forms.PictureBox();
			this.inputScroll = new System.Windows.Forms.VScrollBar();
			this.inputPanel = new System.Windows.Forms.Panel();
			this.inputSend = new System.Windows.Forms.Button();
			this.inputSample = new System.Windows.Forms.PictureBox();
			this.inputSizeLabel = new System.Windows.Forms.Label();
			this.inputOffsetLabel = new System.Windows.Forms.Label();
			this.inputOffset = new System.Windows.Forms.TextBox();
			this.spriteBox = new System.Windows.Forms.PictureBox();
			this.spriteScrollX = new System.Windows.Forms.HScrollBar();
			this.spriteScrollY = new System.Windows.Forms.VScrollBar();
			this.spritePanel = new System.Windows.Forms.Panel();
			this.spriteSave = new System.Windows.Forms.Button();
			this.spriteName = new System.Windows.Forms.TextBox();
			this.mirrorVert = new System.Windows.Forms.Button();
			this.mirrorHori = new System.Windows.Forms.Button();
			this.rotateRight = new System.Windows.Forms.Button();
			this.rotateLeft = new System.Windows.Forms.Button();
			this.palette4 = new System.Windows.Forms.TextBox();
			this.palette3 = new System.Windows.Forms.TextBox();
			this.palette2 = new System.Windows.Forms.TextBox();
			this.palette1 = new System.Windows.Forms.TextBox();
			this.spritePrompt = new System.Windows.Forms.Label();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openBinaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openNESFileStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openSNESFileStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.inputPaletteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.spritePaletteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.inputMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.copyTileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.spriteMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.inputBox)).BeginInit();
			this.inputPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.inputSample)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.spriteBox)).BeginInit();
			this.spritePanel.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// inputBox
			// 
			this.inputBox.BackColor = System.Drawing.SystemColors.ControlDark;
			this.inputBox.Location = new System.Drawing.Point(0, 24);
			this.inputBox.Name = "inputBox";
			this.inputBox.Size = new System.Drawing.Size(331, 496);
			this.inputBox.TabIndex = 0;
			this.inputBox.TabStop = false;
			// 
			// inputScroll
			// 
			this.inputScroll.Location = new System.Drawing.Point(331, 24);
			this.inputScroll.Name = "inputScroll";
			this.inputScroll.Size = new System.Drawing.Size(17, 496);
			this.inputScroll.TabIndex = 3;
			this.inputScroll.Visible = false;
			// 
			// inputPanel
			// 
			this.inputPanel.Controls.Add(this.inputSend);
			this.inputPanel.Controls.Add(this.inputSample);
			this.inputPanel.Controls.Add(this.inputSizeLabel);
			this.inputPanel.Controls.Add(this.inputOffsetLabel);
			this.inputPanel.Controls.Add(this.inputOffset);
			this.inputPanel.Location = new System.Drawing.Point(0, 520);
			this.inputPanel.Name = "inputPanel";
			this.inputPanel.Size = new System.Drawing.Size(348, 80);
			this.inputPanel.TabIndex = 4;
			// 
			// inputSend
			// 
			this.inputSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.inputSend.Location = new System.Drawing.Point(200, 12);
			this.inputSend.Name = "inputSend";
			this.inputSend.Size = new System.Drawing.Size(90, 24);
			this.inputSend.TabIndex = 8;
			this.inputSend.Text = "Send To Sprite";
			this.inputSend.UseVisualStyleBackColor = true;
			this.inputSend.Visible = false;
			// 
			// inputSample
			// 
			this.inputSample.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.inputSample.BackColor = System.Drawing.SystemColors.ControlLight;
			this.inputSample.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.inputSample.Location = new System.Drawing.Point(300, 4);
			this.inputSample.Name = "inputSample";
			this.inputSample.Size = new System.Drawing.Size(40, 40);
			this.inputSample.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.inputSample.TabIndex = 7;
			this.inputSample.TabStop = false;
			this.inputSample.Visible = false;
			// 
			// inputSizeLabel
			// 
			this.inputSizeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.inputSizeLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.inputSizeLabel.Location = new System.Drawing.Point(120, 18);
			this.inputSizeLabel.Name = "inputSizeLabel";
			this.inputSizeLabel.Size = new System.Drawing.Size(58, 15);
			this.inputSizeLabel.TabIndex = 6;
			this.inputSizeLabel.Text = "/";
			this.inputSizeLabel.Visible = false;
			// 
			// inputOffsetLabel
			// 
			this.inputOffsetLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.inputOffsetLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.inputOffsetLabel.Location = new System.Drawing.Point(10, 18);
			this.inputOffsetLabel.Name = "inputOffsetLabel";
			this.inputOffsetLabel.Size = new System.Drawing.Size(48, 15);
			this.inputOffsetLabel.TabIndex = 0;
			this.inputOffsetLabel.Text = "Offset: 0x";
			this.inputOffsetLabel.Visible = false;
			// 
			// inputOffset
			// 
			this.inputOffset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.inputOffset.Enabled = false;
			this.inputOffset.Location = new System.Drawing.Point(58, 15);
			this.inputOffset.Name = "inputOffset";
			this.inputOffset.Size = new System.Drawing.Size(60, 20);
			this.inputOffset.TabIndex = 5;
			this.inputOffset.Text = "0";
			this.inputOffset.Visible = false;
			// 
			// spriteBox
			// 
			this.spriteBox.BackColor = System.Drawing.SystemColors.ControlDark;
			this.spriteBox.Location = new System.Drawing.Point(352, 24);
			this.spriteBox.Name = "spriteBox";
			this.spriteBox.Size = new System.Drawing.Size(331, 269);
			this.spriteBox.TabIndex = 0;
			this.spriteBox.TabStop = false;
			// 
			// spriteScrollX
			// 
			this.spriteScrollX.Location = new System.Drawing.Point(352, 293);
			this.spriteScrollX.Name = "spriteScrollX";
			this.spriteScrollX.Size = new System.Drawing.Size(331, 17);
			this.spriteScrollX.TabIndex = 3;
			this.spriteScrollX.Visible = false;
			// 
			// spriteScrollY
			// 
			this.spriteScrollY.Location = new System.Drawing.Point(683, 24);
			this.spriteScrollY.Name = "spriteScrollY";
			this.spriteScrollY.Size = new System.Drawing.Size(17, 269);
			this.spriteScrollY.TabIndex = 4;
			this.spriteScrollY.Visible = false;
			// 
			// spritePanel
			// 
			this.spritePanel.Controls.Add(this.spriteSave);
			this.spritePanel.Controls.Add(this.spriteName);
			this.spritePanel.Controls.Add(this.mirrorVert);
			this.spritePanel.Controls.Add(this.mirrorHori);
			this.spritePanel.Controls.Add(this.rotateRight);
			this.spritePanel.Controls.Add(this.rotateLeft);
			this.spritePanel.Controls.Add(this.palette4);
			this.spritePanel.Controls.Add(this.palette3);
			this.spritePanel.Controls.Add(this.palette2);
			this.spritePanel.Controls.Add(this.palette1);
			this.spritePanel.Controls.Add(this.spritePrompt);
			this.spritePanel.Location = new System.Drawing.Point(352, 310);
			this.spritePanel.Name = "spritePanel";
			this.spritePanel.Size = new System.Drawing.Size(348, 290);
			this.spritePanel.TabIndex = 5;
			// 
			// spriteSave
			// 
			this.spriteSave.Location = new System.Drawing.Point(228, 103);
			this.spriteSave.Name = "spriteSave";
			this.spriteSave.Size = new System.Drawing.Size(75, 23);
			this.spriteSave.TabIndex = 11;
			this.spriteSave.Text = "Save";
			this.spriteSave.UseVisualStyleBackColor = true;
			this.spriteSave.Visible = false;
			// 
			// spriteName
			// 
			this.spriteName.Location = new System.Drawing.Point(216, 77);
			this.spriteName.Name = "spriteName";
			this.spriteName.Size = new System.Drawing.Size(100, 20);
			this.spriteName.TabIndex = 10;
			this.spriteName.Visible = false;
			// 
			// mirrorVert
			// 
			this.mirrorVert.Location = new System.Drawing.Point(102, 114);
			this.mirrorVert.Name = "mirrorVert";
			this.mirrorVert.Size = new System.Drawing.Size(75, 23);
			this.mirrorVert.TabIndex = 9;
			this.mirrorVert.Text = "Mirror Vert";
			this.mirrorVert.UseVisualStyleBackColor = true;
			this.mirrorVert.Visible = false;
			// 
			// mirrorHori
			// 
			this.mirrorHori.Location = new System.Drawing.Point(18, 114);
			this.mirrorHori.Name = "mirrorHori";
			this.mirrorHori.Size = new System.Drawing.Size(75, 23);
			this.mirrorHori.TabIndex = 8;
			this.mirrorHori.Text = "Mirror Hori";
			this.mirrorHori.UseVisualStyleBackColor = true;
			this.mirrorHori.Visible = false;
			// 
			// rotateRight
			// 
			this.rotateRight.Location = new System.Drawing.Point(102, 75);
			this.rotateRight.Name = "rotateRight";
			this.rotateRight.Size = new System.Drawing.Size(75, 23);
			this.rotateRight.TabIndex = 7;
			this.rotateRight.Text = "Rotate Right";
			this.rotateRight.UseVisualStyleBackColor = true;
			this.rotateRight.Visible = false;
			// 
			// rotateLeft
			// 
			this.rotateLeft.Location = new System.Drawing.Point(18, 75);
			this.rotateLeft.Name = "rotateLeft";
			this.rotateLeft.Size = new System.Drawing.Size(75, 23);
			this.rotateLeft.TabIndex = 6;
			this.rotateLeft.Text = "Rotate Left";
			this.rotateLeft.UseVisualStyleBackColor = true;
			this.rotateLeft.Visible = false;
			// 
			// palette4
			// 
			this.palette4.Location = new System.Drawing.Point(262, 24);
			this.palette4.Name = "palette4";
			this.palette4.Size = new System.Drawing.Size(68, 20);
			this.palette4.TabIndex = 4;
			this.palette4.Visible = false;
			// 
			// palette3
			// 
			this.palette3.Location = new System.Drawing.Point(178, 24);
			this.palette3.Name = "palette3";
			this.palette3.Size = new System.Drawing.Size(68, 20);
			this.palette3.TabIndex = 3;
			this.palette3.Visible = false;
			// 
			// palette2
			// 
			this.palette2.Location = new System.Drawing.Point(94, 24);
			this.palette2.Name = "palette2";
			this.palette2.Size = new System.Drawing.Size(68, 20);
			this.palette2.TabIndex = 2;
			this.palette2.Visible = false;
			// 
			// palette1
			// 
			this.palette1.Location = new System.Drawing.Point(10, 24);
			this.palette1.Name = "palette1";
			this.palette1.Size = new System.Drawing.Size(68, 20);
			this.palette1.TabIndex = 1;
			this.palette1.Visible = false;
			// 
			// spritePrompt
			// 
			this.spritePrompt.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.spritePrompt.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.spritePrompt.Location = new System.Drawing.Point(92, 140);
			this.spritePrompt.Name = "spritePrompt";
			this.spritePrompt.Size = new System.Drawing.Size(167, 26);
			this.spritePrompt.TabIndex = 0;
			this.spritePrompt.Visible = false;
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
			this.menuStrip1.Text = "menuStrip1";
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
			// toolStripMenuItem1
			// 
			this.openNESFileStripMenuItem.Name = "openNESFileStripMenuItem";
			this.openNESFileStripMenuItem.Size = new System.Drawing.Size(209, 22);
			this.openNESFileStripMenuItem.Text = "Open NES File";
			this.openNESFileStripMenuItem.Click += new System.EventHandler(this.openNES);
			// 
			// toolStripMenuItem2
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
			// editToolStripMenuItem
			// 
			this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.inputPaletteToolStripMenuItem,
			this.spritePaletteToolStripMenuItem});
			this.editToolStripMenuItem.Name = "editToolStripMenuItem";
			this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
			this.editToolStripMenuItem.Text = "Edit";
			// 
			// inputPaletteToolStripMenuItem
			// 
			this.inputPaletteToolStripMenuItem.Name = "inputPaletteToolStripMenuItem";
			this.inputPaletteToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
			this.inputPaletteToolStripMenuItem.Text = "Input Palette";
			// 
			// spritePaletteToolStripMenuItem
			// 
			this.spritePaletteToolStripMenuItem.Name = "spritePaletteToolStripMenuItem";
			this.spritePaletteToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
			this.spritePaletteToolStripMenuItem.Text = "Sprite Palette";
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.Title = "Open tiles file";
			this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog1FileOk);
			// 
			// inputMenu
			// 
			this.inputMenu.Name = "inputMenu";
			this.inputMenu.Size = new System.Drawing.Size(61, 4);
			// 
			// copyTileToolStripMenuItem
			// 
			this.copyTileToolStripMenuItem.Name = "copyTileToolStripMenuItem";
			this.copyTileToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
			// 
			// spriteMenu
			// 
			this.spriteMenu.Name = "spriteMenu";
			this.spriteMenu.Size = new System.Drawing.Size(61, 4);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(704, 601);
			this.Controls.Add(this.inputBox);
			this.Controls.Add(this.inputScroll);
			this.Controls.Add(this.inputPanel);
			this.Controls.Add(this.spriteBox);
			this.Controls.Add(this.spriteScrollX);
			this.Controls.Add(this.spriteScrollY);
			this.Controls.Add(this.spritePanel);
			this.Controls.Add(this.menuStrip1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStrip1;
			this.MinimumSize = new System.Drawing.Size(200, 400);
			this.Name = "MainForm";
			this.Text = "SpriteWave";
			((System.ComponentModel.ISupportInitialize)(this.inputBox)).EndInit();
			this.inputPanel.ResumeLayout(false);
			this.inputPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.inputSample)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.spriteBox)).EndInit();
			this.spritePanel.ResumeLayout(false);
			this.spritePanel.PerformLayout();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
