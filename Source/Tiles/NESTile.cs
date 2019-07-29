using System;

namespace SpriteWave
{
	public class NESTile : Tile
	{
		public const int W = 8;
		public const int H = 8;
		public const int ByPP = 1; // bytes per pixel
		public const int CHRSize = 16;

		public override int Width { get { return W; } }
		public override int Height { get { return H; } }
		public override int BytesPP { get { return ByPP; } }

		public NESTile()
		{
			_data = new byte[W * H];
		}

		public override int Import(byte[] chr, int offset)
		{
			if (chr.Length - offset < CHRSize)
				return offset;

			int idx = 0;
			int i, j, bit;
			for (i = 0; i < H; i++)
			{
				for (j = 0, bit = 7; j < W; j++, bit--)
				{
					int mask = 1 << bit;
					int c = ((int)chr[offset + i] & mask) >> bit;
					c |= (((int)chr[offset + i+8] & mask) >> bit) << 1;
					_data[idx] = (byte)c;
					idx++;
				}
			}
			
			return offset + CHRSize;
		}

		public override void ExtractRow(byte[] line, int offset, int y, byte[] palBGRA)
		{
			const int cLen = 4;
			int row = y % H;
			int idx = row * W;
			for (int i = 0, s = 0; i < W; i++, s += cLen)
			{
				// Copies a color from inside 'palBGRA', indexed by 'palIdx', to 'line' at 'offset + s'
				int palIdx = _data[idx] * cLen;
				Buffer.BlockCopy(palBGRA, palIdx, line, offset + s, cLen);
				idx++;
			}
		}
	}
}
