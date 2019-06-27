using System;
using System.Drawing;

using System.Windows.Forms;

namespace SpriteWave
{
	public enum Translation
	{
		Left, Right, Horizontal, Vertical
	}

	public abstract class Tile : IPiece
	{
		// Implements IPiece.EdgeKind
		public virtual EdgeKind EdgeKind { get { return EdgeKind.None; } }

		protected byte[] _data = {};
		public byte[] Data { get { return _data; } }
		
		public abstract int Width { get; }
		public abstract int Height { get; }
		public abstract int BytesPP { get; }

		// If the return value is less than or equal to the offset value, it means there was an error
		public abstract int Import(byte[] input, int offset);

		public abstract void ExtractRow(byte[] line, int offset, int y, byte[] palRGBA);

		public Tile Clone()
		{
			Tile t = Activator.CreateInstance(this.GetType()) as Tile;
			Buffer.BlockCopy(_data, 0, t.Data, 0, Width * Height * BytesPP);
			return t;
		}

		public void ApplyTo(byte[] canvas, int offset, int width, Palette p)
		{
			byte[] palBGRA = p != null ? p.Data : null;

			// Iterates backwards as BMPs are backwards
			for (int i = Height-1; i >= 0; i--)
			{
				ExtractRow(canvas, offset, i, palBGRA);
				offset += width * Utils.cLen;
			}
		}

		public Bitmap ToBitmap(Palette p)
		{
			byte[] pixels = new byte[Width * Height * Utils.cLen];

			ApplyTo(pixels, 0, Width, p);
			return Utils.BitmapFrom(pixels, Width, Height);
		}

		public void Translate(Translation t)
		{
			var image = new byte[Width * Height * BytesPP];
			int idx = 0;
			for (int i = 0; i < Height; i++)
			{
				for (int j = 0; j < Width; j++)
				{
					int y, x;
					switch (t)
					{
						case Translation.Left:
							y = Width-j-1;
							x = i;
							break;
						case Translation.Right:
							y = j;
							x = Height-i-1;
							break;
						case Translation.Horizontal:
							y = i;
							x = Width-j-1;
							break;
						case Translation.Vertical:
							y = Height-i-1;
							x = j;
							break;
						default:
							throw new ArgumentException();
					}

					int off = (y * Width + x) * BytesPP;
					for (int k = 0; k < BytesPP; k++)
					{
						image[off + k] = _data[idx + k];
					}

					idx += BytesPP;
				}
			}

			_data = image;
		}
		
		public bool IsBlank()
		{
			int idx = 0;
			for (int i = 0; i < Height; i++)
			{
				for (int j = 0; j < Width; j++)
				{
					for (int k = 0; k < BytesPP; k++)
					{
						if (_data[idx] != 0)
							return false;

						idx++;
					}
				}
			}

			return true;
		}
	}
}
