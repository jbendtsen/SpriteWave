using System;
using System.Reflection;
using System.IO;
using System.Drawing;
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

		public static MainForm mainForm;

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

		// Doesn't handle negative numbers, only for base-10
		public static int DigitCount(int n)
		{
			return n > 1 ? (int)Math.Log10(n) + 1 : 1;
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

		// Used to convert RGBA to BGRA and back again
		public static uint RedBlueSwap(uint rgba)
		{
			return
				(rgba & 0x00FF00FF) |
				((rgba & 0xFF000000) >> 16) |
				((rgba & 0x0000FF00) << 16)
			;
		}
		
		public static uint ComposeBGRA(double b, double g, double r, double a)
		{
			return
				(((uint)(b * 255f) & 255) << 24) |
				(((uint)(g * 255f) & 255) << 16) |
				(((uint)(r * 255f) & 255) << 8) |
				((uint)(a * 255f) & 255)
			;
		}

		// Ignores alpha
		public static Color FromRGB(uint clr)
		{
			int rgb = (int)(clr >> 8);
			return Color.FromArgb(255, Color.FromArgb(rgb));
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
		public static void EmbedPixel(uint[] output, uint input, int offset = 0)
		{
			output[offset] =
				(((input >> 8) & 0xff) << 24)  |
				(((input >> 16) & 0xff) << 16) |
				(((input >> 24) & 0xff) << 8)  |
				(input & 0xff);
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

		public static readonly string[] RGBANames =
		{
			"Red", "Green", "Blue", "Alpha"
		};

		// Stored as RGBA
		// Palette created using Bisqwit's NES palette generator (https://bisqwit.iki.fi/utils/nespalette.php) with saturation set to 1.5
		public static readonly uint[] NESPalette =
		{
			0x525252FF, 0x001C72FF, 0x0C0B92FF, 0x2A008FFF,
			0x470068FF, 0x57002EFF, 0x550400FF, 0x411200FF,
			0x232400FF, 0x063400FF, 0x003D00FF, 0x003A04FF,
			0x002E39FF, 0x000000FF, 0x000000FF, 0x000000FF,
			0xA0A0A0FF, 0x0E4DCEFF, 0x3230FFFF, 0x621BF9FF,
			0x8E12BFFF, 0xA71468FF, 0xA42315FF, 0x863C00FF,
			0x575900FF, 0x287300FF, 0x087F00FF, 0x007C24FF,
			0x00697AFF, 0x000000FF, 0x000000FF, 0x000000FF,
			0xFEFFFFFF, 0x53A1FFFF, 0x827FFFFF, 0xBA65FFFF,
			0xEB59FFFF, 0xFF5DC0FF, 0xFF6F5EFF, 0xE18E18FF,
			0xADB000FF, 0x76CC00FF, 0x4BDA23FF, 0x36D670FF,
			0x39C1D5FF, 0x3C3C3CFF, 0x000000FF, 0x000000FF,
			0xFEFFFFFF, 0xB4D7FFFF, 0xC9C8FFFF, 0xE2BCFFFF,
			0xF6B7FFFF, 0xFFB8E5FF, 0xFFC1B9FF, 0xF2CF96FF,
			0xDCDD83FF, 0xC4EA85FF, 0xB0F09CFF, 0xA6EEC1FF,
			0xA7E5EDFF, 0xA9A9A9FF, 0x000000FF, 0x000000FF
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
