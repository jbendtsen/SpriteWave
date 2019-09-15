using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public class PalettePanel : Panel
	{
		private readonly int scrollW = SystemInformation.VerticalScrollBarWidth;
		private const int pixPerScroll = 20;

		private PaletteTab _uiTab;

		private ColorBox _box;
		private VScrollBar _scroll;

		private int _nCols;
		private int _nRows;
		private int _maxVisRows;

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

		public PalettePanel(PaletteTab uiTab, IPalette pal, int maxVisRows = 2)
		{
			_uiTab = uiTab;
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
				_box.Image = new Bitmap(s.Width, s.Height);
				_box.Size = s;
			}
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
				double r = (double)((clr >> 8) & 0xff);
				double g = (double)((clr >> 16) & 0xff);
				double b = (double)(clr >> 24);

				double lum = 0.2126*r + 0.7152*g + 0.0722*b;
				_palPolarLum[i] = lum >= 127.5f ? black : white;
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

					uint clr = colors[cell];
					buf[idx] = (byte)(clr >> 24);
					buf[idx + 1] = (byte)((clr >> 16) & 0xff);
					buf[idx + 2] = (byte)((clr >> 8) & 0xff);
					buf[idx + 3] = (byte)(clr & 0xff);

					if (subY < numH && subX < numW)
					{
						// XOR 1 because we want the text colour to oppose the brightness level of the cell
						int clrVersion = _palPolarLum[cell] ^ 1;

						int numLine = (cell * 2 + clrVersion) * (int)numH;
						int numPixIdx = ((numLine + (int)subY) * (int)numW + (int)subX) * Utils.cLen;

						float alpha = (float)_palNumbers[numPixIdx + 3] / 255f;
						buf[idx] = (byte)((float)buf[idx] * (1f - alpha) + _palNumbers[numPixIdx] * alpha);
						buf[idx + 1] = (byte)((float)buf[idx + 1] * (1f - alpha) + _palNumbers[numPixIdx + 1] * alpha);
						buf[idx + 2] = (byte)((float)buf[idx + 2] * (1f - alpha) + _palNumbers[numPixIdx + 2] * alpha);
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

			_uiTab.SelectFromTable(this, idx);
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
