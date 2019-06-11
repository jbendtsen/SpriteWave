using System;
using System.Drawing;
using System.Windows.Forms;

using System.Diagnostics;

namespace SpriteWave
{
	public class SpriteWindow : TileWindow
	{
		protected FileFormat _fmt;
		public FileFormat FormatToLoad { set { _fmt = value; } }

		protected HScrollBar _scrollX;
		public override HScrollBar ScrollX
		{
			set {
				_scrollX = value;
			}
		}

		public override VScrollBar ScrollY
		{
			set {
				_scrollY = value;
			}
		}
		
		public override PictureBox Window
		{
			set {
				_window = value;
                //_window.Paint += new PaintEventHandler(this.paintBox);
                _window.Resize += new EventHandler(this.adjustWindowSize);
                _window.MouseWheel += new MouseEventHandler(this.windowScrollAction);
			}
		}
		
		Label _panelPrompt;

		TextBox _palette1;
		TextBox _palette2;
		TextBox _palette3;
		TextBox _palette4;

		Button _rotateLeft;
		Button _rotateRight;
		Button _mirrorHori;
		Button _mirrorVert;

		TextBox _nameBox;
		Button _saveButton;

		public override Panel Panel
		{
			set {
				_infoPanel = value;
				_panelPrompt = Utils.FindControl(_infoPanel, "spritePrompt") as Label;

				_palette1 = Utils.FindControl(_infoPanel, "palette1") as TextBox;
				_palette2 = Utils.FindControl(_infoPanel, "palette2") as TextBox;
				_palette3 = Utils.FindControl(_infoPanel, "palette3") as TextBox;
				_palette4 = Utils.FindControl(_infoPanel, "palette4") as TextBox;
				
				_palette1.TextChanged += new EventHandler(this.palette1Handler);
				_palette2.TextChanged += new EventHandler(this.palette2Handler);
				_palette3.TextChanged += new EventHandler(this.palette3Handler);
				_palette4.TextChanged += new EventHandler(this.palette4Handler);

				_rotateLeft = Utils.FindControl(_infoPanel, "rotateLeft") as Button;
				_rotateRight = Utils.FindControl(_infoPanel, "rotateRight") as Button;
				_mirrorHori = Utils.FindControl(_infoPanel, "mirrorHori") as Button;
				_mirrorVert = Utils.FindControl(_infoPanel, "mirrorVert") as Button;

				_rotateLeft.Click += new EventHandler(this.rotateLeftHandler);
				_rotateRight.Click += new EventHandler(this.rotateRightHandler);
				_mirrorHori.Click += new EventHandler(this.mirrorHoriHandler);
				_mirrorVert.Click += new EventHandler(this.mirrorVertHandler);

				_nameBox = Utils.FindControl(_infoPanel, "spriteName") as TextBox;
				_saveButton = Utils.FindControl(_infoPanel, "spriteSave") as Button;
				_saveButton.Click += new EventHandler(this.saveButtonHandler);
			}
		}

		public override void Activate()
		{
			_offset.col = 0;
			_offset.row = 0;
			_vis.col = 5;
			_vis.row = 4;
			_cl.AddBlankTiles(20);

			base.Activate();
			_panelPrompt.Visible = false;

			_palette1.Visible = true;
			_palette2.Visible = true;
			_palette3.Visible = true;
			_palette4.Visible = true;
	
			_rotateLeft.Visible = true;
			_rotateRight.Visible = true;
			_mirrorHori.Visible = true;
			_mirrorVert.Visible = true;

			_nameBox.Visible = true;
			_saveButton.Visible = true;
		}

		public override void Close()
		{
			base.Close();
			_scrollX.Visible = false;
			_panelPrompt.Visible = true;

			_palette1.Visible = false;
			_palette1.Text = "";
			_palette2.Visible = false;
			_palette2.Text = "";
			_palette3.Visible = false;
			_palette3.Text = "";
			_palette4.Visible = false;
			_palette4.Text = "";
	
			_rotateLeft.Visible = false;
			_rotateRight.Visible = false;
			_mirrorHori.Visible = false;
			_mirrorVert.Visible = false;

			_nameBox.Visible = false;
			_saveButton.Visible = false;
		}

		public override void EnableSelection() {}
		public override void DisableSelection() {}
		
		public override void Scroll(int x, int y) {}

		// Implements ISelection.Receive()
		public override void Receive(ISelection isel)
		{
			if (isel.Tile == null)
				return;

			if (_cl == null)
			{
				_cl = new Collage(_fmt, 5, false);
				Activate();
			}
			
			Tile t = _fmt.NewTile();
			t.Copy(isel.Tile);

			_cl.SetTile(_sel.Location, t);
			_cl.Render();
		}

		public override void UpdateBars()
		{
			if (_cl != null)
			{
				_scrollX.Inform(_offset.col, _vis.col, _cl.Columns);
				_scrollY.Inform(_offset.row, _vis.row, _cl.Rows);
			}
		}

		public override void ResetScroll()
		{
			_scrollX.Reset();
			base.ResetScroll();
		}

		private void FlipTile(Translation tr)
		{
			if (_cl == null || _sel != this)
				return;

			Tile t = _cl.TileAt(_selPos);
			if (t != null)
			{
				t.Translate(tr);
				//_cl.SetTile(_selPos, t);
				_cl.Render();
			}
		}

		public void rotateLeftHandler(object sender, EventArgs e)
		{
			FlipTile(Translation.Left);
		}
		public void rotateRightHandler(object sender, EventArgs e)
		{
			FlipTile(Translation.Right);
		}
		public void mirrorHoriHandler(object sender, EventArgs e)
		{
			FlipTile(Translation.Horizontal);
		}
		public void mirrorVertHandler(object sender, EventArgs e)
		{
			FlipTile(Translation.Vertical);
		}

		private void SetPaletteIndex(int idx, string text)
		{
			if (_fmt.TypeString != "NESTile")
				return;

			uint n;
			try {
				n = Convert.ToUInt32(text, 16);
			}
			catch {
				n = 0;
			}

			_cl.SetColour(idx, n);
			_cl.Render();

			_sel = null;
			Draw();
		}

		public void palette1Handler(object sender, EventArgs e)
		{
			SetPaletteIndex(0, _palette1.Text);
		}
		public void palette2Handler(object sender, EventArgs e)
		{
			SetPaletteIndex(1, _palette2.Text);
		}
		public void palette3Handler(object sender, EventArgs e)
		{
			SetPaletteIndex(2, _palette3.Text);
		}
		public void palette4Handler(object sender, EventArgs e)
		{
			SetPaletteIndex(3, _palette4.Text);
		}

		public void saveButtonHandler(object sender, EventArgs e)
		{
			string name = _nameBox.Text;
			if (name == "")
				name = "sprite";

			string fileName = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + name + ".png";
			_cl.Bitmap.Scale(10).Save(fileName);
		}
	}
}
