using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public class PalettePanel : Panel
	{
		private readonly int scrollW = SystemInformation.VerticalScrollBarWidth;
		private const int pixPerScroll = 20;

		private ColorBox _box;
		private VScrollBar _scroll;

		private int _nCols;
		private int _nRows;
		private int _palLen = 0;

		private int[] _palPolarLum = null;
		private byte[] _palNumbers = null;

		private Collage _collage;
		public Collage Collage
		{
			set {
				_collage = value;
                if (_collage == null)
                    return;

				int len = _collage.ActiveColors.Length / Utils.cLen;
				if (_palLen != len)
				{
					_palLen = len;
					_palNumbers = DigitImages.Generate(_palLen);
				}

				//CalcBrightness();
			}
		}

		private int FirstVisibleCell
		{
			get { return _scroll.Enabled ? _scroll.Value * _nCols : 0; }
		}
		private int VisibleRows
		{
			get { return _nRows <= 1 ? 1 : 2; }
		}

		public PalettePanel(Collage cl, Point loc, Size size)
		{
			this.Collage = cl;

			this.Name = "paletteBox";
			this.Size = size;
			this.Location = loc;

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
			if (_collage != null)
			{
				int len = _collage.ActiveColors.Length / Utils.cLen;
				_nCols = len;
				if (_nCols >= 8)
					_nCols = this.Width < 200 ? 4 : 8;

				_nRows = (int)Math.Ceiling((float)len / (float)_nCols);
			}

			bool scVis = _nRows > 2;
			if (scVis)
			{
				if (!_scroll.Enabled)
					_scroll.Enabled = true;

				_scroll.Maximum = _nRows - 2;
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
			if (_collage == null || this.Height <= 0)
				return;

			// Calculate the brightness of each color to determine its opposite luminance pole (black or white)
			var colors = _collage.ActiveColors;
			_palPolarLum = new int[_palLen];

			const int white = 0, black = 1;
			for (int i = 0; i < _palLen; i++)
			{
				double r = (double)colors[i*4+2];
				double g = (double)colors[i*4+1];
				double b = (double)colors[i*4];

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
					int cell = 4 * (cellY + x);

					buf[idx] = colors[cell++];
					buf[idx + 1] = colors[cell++];
					buf[idx + 2] = colors[cell++];
					buf[idx + 3] = colors[cell++];

					if (subY < numH && subX < numW)
					{
						// minus 4 because we already incremented the cell channel index to the next cell
						int cellIdx = (cell-4) / 4;
						// XOR 1 because we want the text colour to oppose the brightness level of the cell
						int clrVersion = _palPolarLum[cellIdx] ^ 1;

						int numLine = (cellIdx * 2 + clrVersion) * (int)numH;
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
/*
			double max = _collage.HighestColor;
			uint clr = (uint)(new Random().NextDouble() * max);

			_collage.SetColor(idx, clr);
			CalcBrightness();

			_collage.Render();
			Draw();
*/
			Utils.OpenColorPicker(_collage, idx);
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
