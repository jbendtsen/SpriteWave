using System;

namespace SpriteWave
{
	public class MDTile : Tile
	{
		public const int W = 8;
		public const int H = 8;
		public const int ByPP = 1; // bytes per pixel
		public const int TileSize = 32;

		public override int Width { get { return W; } }
		public override int Height { get { return H; } }
		public override int BytesPP { get { return ByPP; } }

		public MDTile()
		{
			_data = new byte[W * H];
		}

		public override int Import(byte[] tile, int offset)
		{
			if (tile.Length - offset < TileSize)
				return offset;

			int i, idx = 0;
			for (i = offset; i < offset + TileSize; i++)
			{
				_data[idx++] = (byte)((int)tile[i] >> 4);
				_data[idx++] = (byte)((int)tile[i] & 0xf);
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
