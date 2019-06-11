using System;
using System.Drawing;
using System.Windows.Forms;

using System.Diagnostics;

namespace SpriteWave
{
	public class InputWindow : TileWindow
	{
		private byte[] _contents;

		public override HScrollBar ScrollX
		{
			set
			{
				return;
			}
		}

		public override VScrollBar ScrollY
		{
			set {
				_scrollY = value;
				_scrollY.Scroll += new ScrollEventHandler(this.barScrollAction);
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

		//private SplitContainer _splitInput;
		private Label _offsetLabel;
		private TextBox _offsetBox;
		private Label _sizeLabel;
		private Button _sendTile;
		private PictureBox _tileSample;

		private Bitmap _tileSampleBmp;

		public override Panel Panel
		{
			set {
				_infoPanel = value;
				//_infoPanel.Layout += new LayoutEventHandler(this.PanelLayout);

				//_splitInput = _infoPanel.Parent as SplitContainer;

				_offsetLabel = Utils.FindControl(_infoPanel, "inputOffsetLabel") as Label;

				_offsetBox = Utils.FindControl(_infoPanel, "inputOffset") as TextBox;
				_offsetBox.TextChanged += new EventHandler(this.editOffsetBox);

				_sizeLabel = Utils.FindControl(_infoPanel, "inputSizeLabel") as Label;

				_sendTile = Utils.FindControl(_infoPanel, "inputSend") as Button;

				_tileSample = Utils.FindControl(_infoPanel, "inputSample") as PictureBox;
				_tileSample.Paint += new PaintEventHandler(this.paintSample);
			}
		}

		public override void Activate()
		{
			base.Activate();
			_infoPanel.Visible = true;

			_offsetLabel.Visible = true;

			_offsetBox.Visible = true;
			_offsetBox.Enabled = true;
			_offsetBox.Text = "0";

			_sizeLabel.Visible = true;

			_vis.col = _cl.Columns;
		}
		
		public override void Close()
		{
			base.Close();
			_infoPanel.Visible = false;
		}

		public void Load(FileFormat fmt, byte[] file, int offset = 0)
		{
			_contents = file;
			_cl = new Collage(fmt);
			_cl.LoadTiles(_contents, offset);

			_sizeLabel.Text = "/ 0x" + file.Length.ToString("X");

			Activate();
			ResetScroll();
		}

		public void Load(int offset)
		{
			if (_cl != null && _contents != null)
				_cl.LoadTiles(_contents, offset);

			ResetScroll();
		}
		
		public override void Receive(ISelection isel) {}
		
		public override void UpdateBars()
		{
			if (_cl != null)
				_scrollY.Inform(_offset.row, _vis.row, _cl.Rows);
		}

		public override void EnableSelection()
		{
			if (_cl == null)
				return;

			_tileSampleBmp = TileBitmap(Tile);
			_tileSample.Visible = true;
			_sendTile.Visible = true;
		}

		public override void DisableSelection()
		{
			if (_cl == null)
				return;
			
			_sendTile.Visible = false;
			_tileSample.Visible = false;
			_tileSampleBmp = null;
		}

		public override void SetPosition(Position p)
		{
			base.SetPosition(p);
			_tileSampleBmp = TileBitmap(Tile);
		}

		private void editOffsetBox(object sender, EventArgs e)
		{
			try {
				string text = _offsetBox.Text;
				int offset = 0;
				if (text.Length > 0)
					offset = Convert.ToInt32(text, 16);

				if (offset >= 0 && offset < _contents.Length)
				{
					Selection = null;
					Load(offset);
					Draw();
				}
			}
			catch {}
		}

		private void paintSample(object sender, PaintEventArgs e)
		{
			e.Graphics.ToggleSmoothing(false);
			e.Graphics.DrawImage(_tileSampleBmp, 0, 0, _tileSample.Width - 1, _tileSample.Height - 1);
		}
	}
}
