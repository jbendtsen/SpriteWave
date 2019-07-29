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

		private ColorTable _tbl;

		public Palette(ColorTable tbl)
		{
			_tbl = tbl;
			uint[] defs = _tbl.Defaults;
			_data = new byte[defs.Length * Utils.cLen];

			for (int i = 0; i < defs.Length; i++)
				SetColor(i, defs[i], false);

			_mean = Utils.MeanColor(_data);
		}

		public void SetColor(int idx, uint tblClr, bool recalcMean = true)
		{
			int which = idx * Utils.cLen;
			if (which < 0 || which > _data.Length - 4)
				return;

			uint rgba = _tbl.NativeToRGBA(tblClr);
			Utils.EmbedPixel(_data, rgba, which);

			if (recalcMean)
				_mean = Utils.MeanColor(_data);
		}
	}
}
