using System;

namespace SpriteWave
{
	public class ColorList : ColorTable, IPalette
	{
		// Stored as red, green, blue, alpha
		private uint[] _clrs;

		public uint this[int idx]
		{
			get { return Utils.RedBlueSwap(_clrs[idx]); }
			set { _clrs[idx] = Utils.RedBlueSwap(value); }
		}

		public int ColorCount { get { return _clrs.Length; } }

		public uint[] GetList()
		{
			var list = new uint[_clrs.Length];
			for (int i = 0; i < _clrs.Length; i++)
				list[i] = this[i];

			return list;
		}

		public void Render() {}

		public ColorList(uint[] clrList, uint[] defSel)
		{
			_clrs = clrList;
			_defSel = defSel;
		}

		public override uint NativeToRGBA(uint idx)
		{
			uint c = 0;
			if (idx >= 0 && idx < _clrs.Length)
				c = _clrs[idx];

			return c;
		}

		// Only checks red, green and blue
		public override uint RGBAToNative(uint rgba)
		{
			double inRed = Utils.ColorAtF(rgba, 24);
			double inGreen = Utils.ColorAtF(rgba, 16);
			double inBlue = Utils.ColorAtF(rgba, 8);

			double lowest = Double.PositiveInfinity;
			int idx = 0;
			for (int i = 0; i < _clrs.Length; i++)
			{
				uint c = _clrs[idx];
				double clrRed = Utils.ColorAtF(c, 24);
				double clrGreen = Utils.ColorAtF(c, 16);
				double clrBlue = Utils.ColorAtF(c, 8);

				// Fun fact: this is the formula for working out the distance between two points in 3D space
				double delta = Math.Sqrt(
					Math.Pow(clrRed - inRed, 2) +
					Math.Pow(clrGreen - inGreen, 2) +
					Math.Pow(clrBlue - inBlue, 2)
				);

				if (delta < lowest)
				{
					idx = i;
					lowest = delta;
				}
			}

			return (uint)idx;
		}
	}
}