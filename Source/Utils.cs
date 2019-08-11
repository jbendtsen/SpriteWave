using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace SpriteWave
{
	public struct Position
	{
		public int row, col;

		public Position(int c, int r)
		{
			col = c;
			row = r;
		}
	}

	public struct EventPair
	{
		public string name;
		public EventHandler handler;

		public EventPair(string n, EventHandler h)
		{
			name = n;
			handler = h;
		}
	}

	public static class Utils
	{
		public const int cLen = 4;

		public static Type TileType(string name)
		{
			return Type.GetType("SpriteWave." + name);
		}
		
		public static Control FindControl(Control container, string name)
		{
			Control[] list = container.Controls.Find(name, false);
			if (list == null || list.Length != 1)
				return null;

			return list[0];
		}

		/*
			Thanks again, StackOverflow!
			https://stackoverflow.com/a/439606
		*/
		public static Control FindActiveControl(Control c)
		{
			var container = c as IContainerControl;
			while (container != null)
			{
				c = container.ActiveControl;
				container = c as IContainerControl;
			}

			return c;
		}
		
		public delegate object ControlFunc(Control ctrl, object result);

		// Sounds (at least) 10x scarier than it really is
		public static object ApplyRecursiveControlFunc(Control ctrl, ControlFunc ctrlFunc, object result = null)
		{
			result = ctrlFunc(ctrl, result);

			foreach (Control c in ctrl.Controls)
				result = ApplyRecursiveControlFunc(c, ctrlFunc, result);

			return result;
		}

		public static void ApplyEvents(Control ctrl, EventPair[] events)
		{
			var list = ctrl.GetType().GetEvents();
			int count = 0;
			foreach (EventInfo ev in list)
			{
				foreach (EventPair pair in events)
				{
					if (ev.Name == pair.name)
					{
						ev.AddEventHandler(ctrl, pair.handler);
						count++;
						break;
					}
				}
				if (count == events.Length)
					return;
			}
		}

		/*
			https://stackoverflow.com/a/26808856
			Honestly, where would I be without this site
		*/
		public static bool HasMouse(this Control c)
		{
			return c.ClientRectangle.Contains(c.PointToClient(Cursor.Position));
		}

		/*
			Check to see if a control needs to be initialised.
			If the control has already been set, set its visibility depending on whether the new value is null.
			Note that in the case that both the given control and its intended new value are null,
			 this will return false, as there is no initialisation that can be done.
		*/
		public static bool NeedsInit(this Control c, Control val)
		{
			if (c != null)
				c.Visible = val != null;

			return c == null && val != null;
		}

		public static void Reset(this ScrollBar bar)
		{
			bar.Minimum = 0;
			bar.Value = 0;
		}

		public static void Inform(this ScrollBar bar, int value, int large, int min, int max)
		{
			bar.LargeChange = large;
			bar.Minimum = min;
			bar.Maximum = max;
			if (bar.Visible && value >= bar.Minimum && value <= bar.Maximum)
				bar.Value = value;
		}

		public static void ToggleSmoothing(this Graphics g, bool smooth)
		{
			if (smooth)
			{
				g.InterpolationMode = InterpolationMode.Bilinear;
				g.PixelOffsetMode = PixelOffsetMode.Default;
			}
			else
			{
				g.InterpolationMode = InterpolationMode.NearestNeighbor;
				g.PixelOffsetMode = PixelOffsetMode.Half;
			}
		}

		public static Bitmap Scale(this Bitmap bmp, float scX, float scY = default(float), bool smooth = false)
		{
			if (object.Equals(scY, default(float)))
				scY = scX;

			int width = (int)((float)bmp.Width * scX);
			int height = (int)((float)bmp.Height * scY);

			Bitmap scaled = new Bitmap(width, height);
			using (var g = Graphics.FromImage(scaled))
			{
				g.ToggleSmoothing(smooth);
				g.DrawImage(bmp, 0, 0, width, height);
			}

			return scaled;
		}

		/*
			Combination of:
				- https://www.c-sharpcorner.com/article/drawing-transparent-images-and-shapes-using-alpha-blending/
				- https://stackoverflow.com/a/38852476
				- the function above
		*/
		public static Bitmap SetAlpha(this Bitmap bmp, float alpha)
		{
			float[][] ptsArray = {
				new[] {1f, 0, 0, 0, 0},
				new[] {0f, 1, 0, 0, 0},
				new[] {0f, 0, 1, 0, 0},
				new[] {0f, 0, 0, alpha, 0},
				new[] {0f, 0, 0, 0, 1}
			};

			Bitmap faded = new Bitmap(bmp.Width, bmp.Height);
			using (var imgAttrs = new ImageAttributes())
			{
				imgAttrs.SetColorMatrix(new ColorMatrix(ptsArray));
				using (var g = Graphics.FromImage(faded))
				{
					g.DrawImage(
						bmp,
						new Rectangle(0, 0, bmp.Width, bmp.Height),
						0, 0, bmp.Width, bmp.Height,
						GraphicsUnit.Pixel, imgAttrs
					);
				}
			}

			return faded;
		}

		public unsafe static void ClearBitmap(Bitmap img, Color clr, Rectangle area)
		{
			uint pix = 0xff000000 | (uint)clr.R << 16 | (uint)clr.G << 8 | (uint)clr.B;

			var data = img.LockBits(
				area,
				ImageLockMode.ReadWrite,
				PixelFormat.Format32bppArgb
			);

			uint *fb = (uint*)data.Scan0.ToPointer();
			int size = area.Width * area.Height;

			for (int i = 0; i < size; i++)
				*fb++ = pix;

			img.UnlockBits(data);
		}

		public static void Clear(this PictureBox box)
		{
			ClearBitmap(box.Image as Bitmap, box.BackColor, box.DisplayRectangle);
			box.Invalidate();
		}

		public static int Between(this int n, int min, int max)
		{
			return Math.Max(Math.Min(n, max), min);
		}

		public static uint GetBits(uint val, int len, int shift)
		{
			uint mask = (1u << len) - 1;
			return (val & (mask << shift)) >> shift;
		}

		public static uint ColorAt(uint clr, int rshift)
		{
			return (clr >> rshift) & 0xff;
		}
		public static double ColorAtF(uint clr, int rshift)
		{
			return (double)((clr >> rshift) & 0xff) / 255.0;
		}

		// Ignores alpha
		public static Color FromRGB(uint clr)
		{
			int rgb = (int)(clr >> 8);
			return Color.FromArgb(255, Color.FromArgb(rgb));
		}

		private static uint ReduceAggregateRGBA(int nComp, double red, double green, double blue, double alpha)
		{
			double n = (double)nComp;

			uint rgba = ((uint)(red / n) & 0xff) << 24;
			rgba |= ((uint)(green / n) & 0xff) << 16;
			rgba |= ((uint)(blue / n) & 0xff) << 8;
			rgba |= (uint)(alpha / n) & 0xff;

			return rgba;
		}

		// This is one MEAN method hey?
		// omg am i the funniest programmer alive or what
		public static uint MeanColor(uint[] list)
		{
			double red = 0, green = 0, blue = 0, alpha = 0;
			foreach (uint clr in list)
			{
				red += (double)((clr >> 24) & 0xff);
				green += (double)((clr >> 16) & 0xff);
				blue += (double)((clr >> 8) & 0xff);
				alpha += (double)(clr & 0xff);
			}

			return ReduceAggregateRGBA(list.Length, red, green, blue, alpha);
		}

		// bet you were wondering if i MEANt to repeat this method, eh?
		// Expected format for list: every 4 bytes makes a pixel,
		//  with the 4 bytes being stored as blue, green, red, alpha.
		public static uint MeanColor(byte[] list)
		{
			double red = 0, green = 0, blue = 0, alpha = 0;
			int len = list.Length / cLen;
			for (int i = 0; i < len; i++)
			{
				red += (double)list[i * cLen + 2];
				green += (double)list[i * cLen + 1];
				blue += (double)list[i * cLen];
				alpha += (double)list[i * cLen + 3];
			}

			return ReduceAggregateRGBA(len, red, green, blue, alpha);
		}

		public static uint InvertRGB(uint clr)
		{
			return (clr ^ 0xFFFFFF00) | 0xFF;
		}
		/*
			This function assumes 'input' is stored as red.green.blue.alpha
			  so that it can be transformed into {blue, green, red, alpha}.
			The reason the order is shuffled is so that the pixel
			  can be easily understood by the Windows graphics API.
		*/
		public static void EmbedPixel(byte[] output, uint input, int offset = 0)
		{
			output[offset] = (byte)((input >> 8) & 0xff); // blue
			output[offset+1] = (byte)((input >> 16) & 0xff); // green
			output[offset+2] = (byte)((input >> 24) & 0xff); // red
			output[offset+3] = (byte)(input & 0xff); // alpha
		}
		/*
		public static uint FromBA(byte[] input, int offset)
		{
			uint output = (uint)input[offset] << 24;
			output |= (uint)input[offset+1] << 16;
			output |= (uint)input[offset+2] << 8;
			return output | (uint)input[offset+3];
		}
		*/
		// An extension method for int.
		// Take an integer and stuff it into an array (using little-endian format)
		public static void EmbedLE(this int i, byte[] array, int offset)
		{
			uint val = (uint)i;
			// not sure how effective loop rolling is in .NET but let's give it a go
			array[offset+3] = (byte)((val >> 24) & 0xff);
			array[offset+2] = (byte)((val >> 16) & 0xff);
			array[offset+1] = (byte)((val >> 8) & 0xff);
			array[offset] = (byte)(val & 0xff);
		}

		/*
			This method takes a byte array of 32-bit pixels and converts it into a Bitmap (which can be used in rendering).
			It assumes that 'pixbuf' stores each pixel as 4 consecutive bytes, in the format B, G, R & A respectively.
			Note: Bitmaps often require padding at the end of each row to the next multiple of 4 bytes,
			  however since this method only deals with 4-byte pixels, no padding is ever necessary.
		*/
		public static Bitmap BitmapFrom(byte[] pixbuf, int width, int height)
		{
			// Create a BMP header, so that the API knows how to arrange our pixels
			byte[] hdr = new byte[54];

			// BMP magic (file type identifier)
			hdr[0] = (byte)'B';
			hdr[1] = (byte)'M';

			// File size
			int fileSize = pixbuf.Length + 54;
			fileSize.EmbedLE(hdr, 2);
			// Header size
			hdr[10] = 54;
			// Pixel data offset
			hdr[14] = 40;

			// Width
			width.EmbedLE(hdr, 18);
			// Height
			height.EmbedLE(hdr, 22);

			// Number of drawing planes (zoooooom)
			hdr[26] = 1;
			// BGRA = 32BPP
			hdr[28] = 32;
			// Pixel data size
			pixbuf.Length.EmbedLE(hdr, 34);

			// Stitch the header and the pixel data together, as if it were a BMP file
			byte[] bmpFile = new byte[hdr.Length + pixbuf.Length];
			Buffer.BlockCopy(hdr, 0, bmpFile, 0, hdr.Length);
			Buffer.BlockCopy(pixbuf, 0, bmpFile, hdr.Length, pixbuf.Length);

			/*
				Some explanation: the most efficient way to create a drawable graphic from RAM in WinForms
				is to make a MemoryStream (like a file but references RAM instead) out of our buffer,
				and then pass that to a Bitmap constructor. This creates a perfectly good Bitmap, but there's a catch.
				When creating a Bitmap in this way, it references the MemoryStream instead of making a copy of the data.
				This is fine, but we need our Bitmap to outlive the MemoryStream. So, we make a copy of the first Bitmap and return that instead.
				Note that a 'using' block means that whichever object you specified to use gets "Dispose()d" of once the block ends.
			*/
			Bitmap bmp;
			using (var ms = new MemoryStream(bmpFile))
			{
				using (var temp = new Bitmap(ms))
					bmp = new Bitmap(temp);
			}

			return bmp;
		}

		// Doesn't handle negative numbers, only for base-10
		public static int DigitCount(int n)
		{
			return n > 1 ? (int)Math.Log10(n) + 1 : 1;
		}

		public const int numberW = 8;
		public const int numberH = 16;
		private const float numbersFontSize = 12;

		// List of pixel maps of numbers 0-9 (black then white)
		private static byte[][] _numbersPix;

		public static byte[] GenerateNumbersPixMap(int count)
		{
			if (count < 2)
				return null;

			const int numBufSize = numberW * numberH * cLen;

			if (_numbersPix == null)
			{
				System.Diagnostics.Debug.WriteLine("Generating digits");

				Font font = new Font(FontFamily.GenericMonospace, numbersFontSize, FontStyle.Bold, GraphicsUnit.Pixel);
				Brush[] clrs = {new SolidBrush(Color.Black), new SolidBrush(Color.White)};

				// For positioning digit characters inside each image
				const float numKernX = -2;
				const float numKernY = 1;
				var bounds = new RectangleF(
					numKernX,
					numKernY,
					numberW - numKernX,
					numberH - numKernY
				);
				var sizeRect = new Rectangle(
					0, 0, numberW, numberH
				);

				_numbersPix = new byte[10 * 2][];
				int idx = 0;
				for (int i = 0; i < 2; i++) // 2 colours
				{
					for (int j = 0; j < 10; j++) // 10 digits
					{
						// Render text to a bitmap image
						var bmp = new Bitmap(numberW, numberH);
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

			// Minus one because maximum value is the last index, not the size
			int nDigits = DigitCount(count - 1);
			int[] digits = new int[nDigits];

			// x2 is for black and white versions
			var numArrayPix = new byte[numBufSize * nDigits * count * 2];

			int stride = nDigits * numberW * cLen;
			for (int i = 0; i < count; i++)
			{
				int dc = DigitCount(i);
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

				for (j = 0; j < numberH; j++)
				{
					int dstX = 0;
					for (int k = 0; k < nDigits; k++)
					{
						int srcOff = j * numberW * cLen;
						int dstY = i * 2 * numberH + j;

						if (digits[k] >= 0)
						{
							// Copy row from black digit
							Buffer.BlockCopy(
								_numbersPix[digits[k]],
								srcOff,
								numArrayPix,
								dstY * stride + dstX,
								numberW * cLen
							);
							// Copy row from white digit
							Buffer.BlockCopy(
								_numbersPix[10 + digits[k]],
								srcOff,
								numArrayPix,
								(dstY + numberH) * stride + dstX,
								numberW * cLen
							);
						}

						dstX += numberW * cLen;
					}
				}
			}

			return numArrayPix;
		}

		public static readonly string[] RGBANames =
		{
			"Red", "Green", "Blue", "Alpha"
		};

		// Stored as RGBA
		public static readonly uint[] NESPalette =
		{
			0x7C7C7CFF, 0x0000FCFF, 0x0000BCFF, 0x4428BCFF,
			0x940084FF, 0xA80020FF, 0xA81000FF, 0x881400FF,
			0x503000FF, 0x007800FF, 0x006800FF, 0x005800FF,
			0x004058FF, 0x000000FF, 0x000000FF, 0x000000FF,
			0xBCBCBCFF, 0x0078F8FF, 0x0058F8FF, 0x6844FCFF,
			0xD800CCFF, 0xE40058FF, 0xF83800FF, 0xE45C10FF,
			0xAC7C00FF, 0x00B800FF, 0x00A800FF, 0x00A844FF,
			0x008888FF, 0x000000FF, 0x000000FF, 0x000000FF,
			0xF8F8F8FF, 0x3CBCFCFF, 0x6888FCFF, 0x9878F8FF,
			0xF878F8FF, 0xF85898FF, 0xF87858FF, 0xFCA044FF,
			0xF8B800FF, 0xB8F818FF, 0x58D854FF, 0x58F898FF,
			0x00E8D8FF, 0x787878FF, 0x000000FF, 0x000000FF,
			0xFCFCFCFF, 0xA4E4FCFF, 0xB8B8F8FF, 0xD8B8F8FF,
			0xF8B8F8FF, 0xF8A4C0FF, 0xF0D0B0FF, 0xFCE0A8FF,
			0xF8D878FF, 0xD8F878FF, 0xB8F8B8FF, 0xB8F8D8FF,
			0x00FCFCFF, 0xF8D8F8FF, 0x000000FF, 0x000000FF
		};

		// A hand-picked selection from the table above
		public static readonly uint[] NESDefSel =
		{
			// dark blue, green, bright yellow, white
			0x0C, 0x1A, 0x37, 0x30
		};

		public const uint SNESRGBAOrderAndDepth = 0x12305551;
		
		// A hand-picked selection of SNES-compatible RGB colors
		public static readonly uint[] SNESDefSel =
		{
			0x8000, 0x8008, 0x804C, 0x808E, // black -> dark blue
			0x818C, 0x9188, 0x9208, 0x9284, // dark blue green -> green
			0xB284, 0xD2C4, 0xEB48, 0xFF48, // green -> bright yellow
			0xFFA8, 0xFFD0, 0xFFF8, 0xFFFF	// bright yellow -> white
		};
	}
}
