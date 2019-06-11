using System;
using System.Drawing;

namespace SpriteWave
{
	public class Palette
	{
		private byte[] _data;
		public byte[] Data { get { return _data; } }

		private uint _mean;
		public uint Mean { get { return _mean; } }

		private ColourTable _tbl;

		public Palette(ColourTable tbl)
		{
			_tbl = tbl;
			uint[] defs = _tbl.Defaults;
			_data = new byte[defs.Length * Utils.cLen];

			for (int i = 0; i < defs.Length; i++)
				SetColour(i, defs[i], false);

			_mean = Utils.MeanColour(_data);
		}

		public void SetColour(int idx, uint tblClr, bool recalcMean = true)
		{
			int which = idx * Utils.cLen;
			if (which < 0 || which > _data.Length - 4)
				return;

			uint rgba = _tbl.NativeToRGBA(tblClr);
			Utils.EmbedPixel(_data, rgba, which);

			if (recalcMean)
				_mean = Utils.MeanColour(_data);
		}
	}
}
