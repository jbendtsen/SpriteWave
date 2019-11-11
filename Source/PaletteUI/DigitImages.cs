using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Imaging;

namespace SpriteWave
{
	public static class DigitImages
	{
		public const int digitW = 8;
		public const int digitH = 16;
		public const float fontSize = 12;

		// List of pixel maps of numbers 0-9 (black then white)
		private static byte[][] _numbersPix;
		private const int numBufSize = digitW * digitH * Utils.cLen;
	
		private static void CreateDigitMaps()
		{
			Font font;
			try {
				font = new Font(FontFamily.GenericMonospace, fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
			}
			catch (ArgumentException) {
				font = SystemFonts.DefaultFont;
			}

			Brush[] clrs = {new SolidBrush(Color.Black), new SolidBrush(Color.White)};

			// For positioning digit characters inside each image
			const float numKernX = -2;
			const float numKernY = 1;
			var bounds = new RectangleF(
				numKernX,
				numKernY,
				digitW - numKernX,
				digitH - numKernY
			);
			var sizeRect = new Rectangle(
				0, 0, digitW, digitH
			);

			_numbersPix = new byte[10 * 2][];
			int idx = 0;
			for (int i = 0; i < 2; i++) // 2 colours
			{
				for (int j = 0; j < 10; j++) // 10 digits
				{
					// Render text to a bitmap image
					var bmp = new Bitmap(digitW, digitH, PixelFormat.Format32bppArgb);
					using (var g = Graphics.FromImage(bmp))
					{
						g.TextRenderingHint = TextRenderingHint.AntiAlias;
						g.DrawString(j.ToString(), font, clrs[i], bounds);
					}

					// Extract the pixel data out of the bitmap image
					_numbersPix[idx] = new byte[numBufSize];
					var data = bmp.LockBits(
						sizeRect,
						ImageLockMode.ReadWrite,
						PixelFormat.Format32bppArgb
					);

					// Copy the bitmap data directly to our pixel array.
					// Each pixel is stored as four bytes - B, G, R, then A.
					Marshal.Copy(data.Scan0, _numbersPix[idx], 0, numBufSize);
					bmp.UnlockBits(data);

					idx++;
				}
			}
		}

		public static byte[] Generate(int count)
		{
			if (count < 2)
				return null;

			if (_numbersPix == null)
				CreateDigitMaps();

			// Minus one because maximum value is the last index, not the size
			int nDigits = Utils.DigitCount(count - 1);
			int[] digits = new int[nDigits];

			// x2 is for black and white versions
			var numArrayPix = new byte[numBufSize * nDigits * count * 2];

			int stride = nDigits * digitW * Utils.cLen;
			for (int i = 0; i < count; i++)
			{
				int dc = Utils.DigitCount(i);
				int n = i;
				int j;
				// Format the digits list such that the number starts from the left side of the row, eg.
				// 7 (out of 8) = |7|, 6 (out of 16) = |6 |, 14 (out of 256) = |14 |
				for (j = 0; j < nDigits; j++)
				{
					if (j < dc)
						digits[dc-j-1] = n % 10; // placed in reverse, as digit significance goes left->right
					else
						digits[j] = -1; // -1 means a blank space

					n /= 10;
				}

				for (j = 0; j < digitH; j++)
				{
					int dstX = 0;
					for (int k = 0; k < nDigits; k++)
					{
						int srcOff = j * digitW * Utils.cLen;
						int dstY = i * 2 * digitH + j;

						if (digits[k] >= 0)
						{
							// Copy row from black digit
							Buffer.BlockCopy(
								_numbersPix[digits[k]],
								srcOff,
								numArrayPix,
								dstY * stride + dstX,
								digitW * Utils.cLen
							);
							// Copy row from white digit
							Buffer.BlockCopy(
								_numbersPix[10 + digits[k]],
								srcOff,
								numArrayPix,
								(dstY + digitH) * stride + dstX,
								digitW * Utils.cLen
							);
						}

						dstX += digitW * Utils.cLen;
					}
				}
			}

			return numArrayPix;
		}
	}
}
