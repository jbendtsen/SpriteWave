using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public class PalettePanel : Panel
	{
		private readonly int scrollW = SystemInformation.VerticalScrollBarWidth;
		private const int pixPerScroll = 20;

		// TODO: Unify this with ColorPicker's alpha rendering
		private readonly float[] _alphaShades = {153, 204};

		private IPalettePicker _uiParent;

		private ColorBox _box;
		private VScrollBar _scroll;

		private int _nCols;
		private int _nRows;
		private int _maxVisRows;

		public int CurrentCell = -1;

		private int _palLen = 0;
		private int[] _palPolarLum = null;
		private byte[] _palNumbers = null;

		private IPalette _pal;
		public IPalette Palette
		{
			set {
				_pal = value;
                if (_pal == null)
                    return;

				int len = _pal.ColorCount;
				if (_palLen != len)
				{
					_palLen = len;
					_palNumbers = DigitImages.Generate(_palLen);
				}

				//CalcBrightness();
			}
			get {
				return _pal;
			}
		}

		private int FirstVisibleCell
		{
			get { return _scroll.Enabled ? _scroll.Value * _nCols : 0; }
		}
		private int VisibleRows
		{
			get { return Math.Min(_nRows, _maxVisRows); }
		}

		public PalettePanel(IPalettePicker uiParent, IPalette pal, int maxVisRows = 2)
		{
			_uiParent = uiParent;
			this.Palette = pal;
			_maxVisRows = maxVisRows;

			this.Name = "paletteBox";
			this.Location = new Point(0, 0);
			Size size = new Size(80, 20);
			this.Size = size;

			_box = new ColorBox();
			_box.MouseDown += this.boxClickHandler;
			_box.MouseWheel += this.boxScrollHandler;

			_scroll = new VScrollBar();
			_scroll.Scroll += (s, e) => Draw();

			int scW = SystemInformation.VerticalScrollBarWidth;
			_scroll.Location = new Point(size.Width - scW, 0);
			_scroll.Size = new Size(scW, size.Height);

			_scroll.Minimum = 0;
			_scroll.Value = 0;
			_scroll.LargeChange = 1;
			_scroll.SmallChange = 1;

			this.Controls.Add(_box);
			this.Controls.Add(_scroll);
		}

		public void AdjustContents(ToolBoxOrientation layout)
		{
			_nCols = 1;
			_nRows = 1;
			if (_pal != null)
			{
				if (this.Width >= 400)
					_nCols = 16;
				else if (this.Width >= 200)
					_nCols = 8;
				else
					_nCols = 4;

				_nCols = Math.Min(_nCols, _palLen);
				_nRows = (int)Math.Ceiling((float)_palLen / (float)_nCols);
			}

			bool scVis = _nRows > _maxVisRows;
			if (scVis)
			{
				if (!_scroll.Enabled)
					_scroll.Enabled = true;

				_scroll.Maximum = _nRows - _maxVisRows;
			}
			else if (_scroll.Enabled)
				_scroll.Enabled = false;

			int scX = layout == ToolBoxOrientation.Left ? 0 : this.Width - scrollW;
			_scroll.Location = new Point(scX, 0);
			_scroll.Size = new Size(scrollW, this.Height);

			int boxX = layout == ToolBoxOrientation.Left ? scrollW : 0;
			_box.Location = new Point(boxX, 0);

			int visRows = VisibleRows;

			if (this.Width > scrollW && this.Height > 0)
			{
				Size s = new Size(this.Width - scrollW, this.Height);
				//_box.Image = new Bitmap(s.Width, s.Height);
				_box.Size = s;
			}
		}

		private static void BlendPixel(byte[] pixbuf, int idx, float b, float g, float r, float alpha)
		{
			pixbuf[idx] = (byte)((float)pixbuf[idx] * (1f - alpha) + b * alpha);
			pixbuf[idx+1] = (byte)((float)pixbuf[idx+1] * (1f - alpha) + g * alpha);
			pixbuf[idx+2] = (byte)((float)pixbuf[idx+2] * (1f - alpha) + r * alpha);
		}

		public void Draw()
		{
			if (_pal == null || this.Height <= 0)
				return;

			// Calculate the brightness of each color to determine its opposite luminance pole (black or white)
			uint[] colors = _pal.GetList();
			_palPolarLum = new int[_palLen];

			const int white = 0, black = 1;
			for (int i = 0; i < _palLen; i++)
			{
				uint clr = colors[i];
				double a = (double)(clr & 0xff) / 255f;
				double r = (double)((clr >> 8) & 0xff) / 255f;
				double g = (double)((clr >> 16) & 0xff) / 255f;
				double b = (double)(clr >> 24) / 255f;

				/*
				// If a cell is selected and it's not this cell, increase the brightness
				if (CurrentCell >= 0 && i != CurrentCell)
				{
					r = 1 - ((1 - r) / 2);
					g = 1 - ((1 - g) / 2);
					b = 1 - ((1 - b) / 2);
				}
				*/

				double lum = 0.2126*r + 0.7152*g + 0.0722*b;
				lum += (1f - lum) * (1f - a);
				_palPolarLum[i] = lum >= 0.5f ? black : white;

				// Reverse pixel channel order
				colors[i] = Utils.ComposeBGRA(a, r, g, b);
			}

			_box.Lock();

			var buf = _box.Buffer;
			int width = _box.Image.Size.Width;
			int height = _box.Image.Size.Height;

			float visRowsF = (float)VisibleRows;
			float cellW = (float)width / (float)_nCols;
			float cellH = (float)height / visRowsF;

			// Minus one because maximum value is the last index, not the size
			int nDigits = Utils.DigitCount(_palLen - 1);

			float numW = (float)(DigitImages.digitW * nDigits);
			float numH = (float)DigitImages.digitH;

			int firstCellIdx = FirstVisibleCell;
			int idx = 0;
			for (int i = 0; i < height; i++)
			{
				int y = (int)((float)i / cellH);
				float subY = (float)i % cellH;
				int cellY = firstCellIdx + y * _nCols;

				for (int j = 0; j < width; j++)
				{
					int x = (int)((float)j / cellW);
					float subX = (float)j % cellW;
					int cell = cellY + x;

					// In most cases, no pixel blending occurs.
					// Therefore, we just use the quickest method (I think) to copy a pixel to a pixel buffer in C#.
					// Note that pixels are laid out as b.g.r.a in memory and x86 is little-endian,
					//  meaning that we need to make sure colors[cell] (the input) is written as 0xaarrggbb before copying.
					Buffer.BlockCopy(colors, cell * 4, buf, idx, 4);

					// If the colour isn't fully opaque
					if (buf[idx+3] != 0xff)
					{
						int tileX = ((j % 16) < 8) ? 1 : 0;
						int tileY = ((i % 16) < 8) ? 1 : 0;
						float shade = _alphaShades[tileX ^ tileY];
						BlendPixel(buf, idx, shade, shade, shade, (float)(255 - buf[idx+3]) / 255f);
						buf[idx+3] = 0xff;
					}

					// If this pixel is in the top-left corner, blend it with its corresponding pixel inside the appropriate digit bitmap.
					// Plain English: this is the part where we draw the numbers.
					if (subY < numH && subX < numW)
					{
						// XOR 1 because we want the text colour to oppose the brightness level of the cell
						int clrVersion = _palPolarLum[cell] ^ 1;

						int numLine = (cell * 2 + clrVersion) * (int)numH;
						int numPixIdx = ((numLine + (int)subY) * (int)numW + (int)subX) * Utils.cLen;

						// If the current pixel inside the digit bitmap isn't fully transparent
						if (_palNumbers[numPixIdx + 3] != 0)
						{
							float dgAlpha = (float)_palNumbers[numPixIdx + 3] / 255f;
							BlendPixel(buf, idx, (float)_palNumbers[numPixIdx], (float)_palNumbers[numPixIdx+1], (float)_palNumbers[numPixIdx+2], dgAlpha);
						}
					}

					idx += 4;
				}
			}

			_box.Unlock();
		}

		private void boxClickHandler(object sender, MouseEventArgs e)
		{
			float cellW = (float)_box.Image.Size.Width / (float)_nCols;
			float cellH = (float)_box.Image.Size.Height / (float)VisibleRows;

			int col = (int)((float)e.X / cellW);
			int row = (int)((float)e.Y / cellH) + (FirstVisibleCell / _nCols);
			int idx = row * _nCols + col;

			_uiParent.SelectFromTable(this, idx);
		}

		private void boxScrollHandler(object sender, MouseEventArgs e)
		{
			if (!_scroll.Enabled)
				return;

			int delta = e.Delta != 0 ? e.Delta / Math.Abs(e.Delta) : 0;
			int oldValue = _scroll.Value;
			int newValue = oldValue - delta;

			if (newValue < _scroll.Minimum)
				newValue = _scroll.Minimum;
			else if (newValue > _scroll.Maximum)
				newValue = _scroll.Maximum;

			_scroll.Value = newValue;

			if (newValue != oldValue)
				Draw();
		}
	}
}
