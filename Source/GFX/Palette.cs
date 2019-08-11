using System;
using System.Drawing;

namespace SpriteWave
{
	public class Palette
	{
		// Each pixel is stored as four bytes, ordered B, G, R, then A
		private byte[] _activeColors;
		public byte[] ActiveColors { get { return _activeColors; } }

		private uint _mean;
		public uint Mean { get { return _mean; } }

		private readonly ColorTable _tbl;

		public Palette(ColorTable tbl)
		{
			_tbl = tbl;
			uint[] defs = _tbl.Defaults;
			_activeColors = new byte[defs.Length * Utils.cLen];

			for (int i = 0; i < defs.Length; i++)
				SetColor(i, defs[i], recalcMean: false);

			_mean = Utils.MeanColor(_activeColors);
		}

		public void SetColor(int idx, uint nativeClr, bool recalcMean = true)
		{
			int which = idx * Utils.cLen;
			if (which < 0 || which > _activeColors.Length - 4)
				return;

			uint rgba = _tbl.NativeToRGBA(nativeClr);
			Utils.EmbedPixel(_activeColors, rgba, which);

			if (recalcMean)
				_mean = Utils.MeanColor(_activeColors);
		}
	}
}
