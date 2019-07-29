using System;

namespace SpriteWave
{
	public class SNESTile : Tile
	{
		public const int W = 8;
		public const int H = 8;
		public const int ByPP = 1; // bytes per pixel
		public const int TileSize = 32;

		public override int Width { get { return W; } }
		public override int Height { get { return H; } }
		public override int BytesPP { get { return ByPP; } }

		public SNESTile()
		{
			_data = new byte[W * H];
		}

		public override int Import(byte[] tile, int offset)
		{
			if (tile.Length - offset < TileSize)
				return offset;

			// byte position for current row, bit to access current column
			int rpos, bit;

			int i, j, idx = 0;
			for (i = 0, rpos = 0; i < H; i++, rpos += 2)
			{
				// First column is most significant bit, last column is least significant bit
				for (j = 0, bit = 7; j < W; j++, bit--)
				{
					int c, mask = 1 << bit;

					c =	  ((int)tile[offset + rpos]        & mask) >> bit;
					c |= (((int)tile[offset + rpos + 1]    & mask) >> bit) << 1;
					c |= (((int)tile[offset + rpos + 16]   & mask) >> bit) << 2;
					c |= (((int)tile[offset + rpos + 16+1] & mask) >> bit) << 3;

					_data[idx] = (byte)c;
					idx++;
				}
			}

			return offset + TileSize;
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
