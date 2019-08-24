using System;

namespace SpriteWave
{
	/*
		ColorTable: A way of keeping track of which colours the system (NES, SNES, etc.) has access to.
		There are two different ways a ColorTable is created: with a pre-defined set of colours, or with an RGBA pattern.
		The RGBA pattern is interpreted as a RGBA Order And Depth formula. More detail on that one is provided below.
	*/
	public class ColorTable
	{
		// Predetermined color list
		private uint[] _clrList;

		// RGBA pattern
		private byte[] _rgbaOrder, _rgbaOrderInv;
		private byte[] _rgbaDepth;

		// Default selection of colours, in the native format. Useful if a palette has not yet been decided.
		private uint[] _defSel;

		public bool IsList { get { return _clrList != null; } }
		public uint[] Defaults { get { return _defSel; } }

		public uint LastColor
		{
			get {
				return _clrList != null
				?
					(uint)(_clrList.Length - 1)
				:
					(uint)(Math.Pow(2f, (double)(_rgbaDepth[0] + _rgbaDepth[1] + _rgbaDepth[2] + _rgbaDepth[3])) - 1);
			}
		}

		public ColorTable(uint[] clrList, uint[] defSel)
		{
			_rgbaOrder = null;
			_rgbaDepth = null;
			_clrList = clrList;
			_defSel = defSel;
		}

		/*
			RGBA Order And Depth: A formula descriptor for how to arrange different formats of RGBA.
			The input parameter as passed as the integer 0xrgbaRGBA where:
				r = position of red channel (0-3)
				g = position of green channel (0-3)
				b = position of blue channel (0-3)
				a = position of alpha channel (0-3)
				R = depth (number of bits) for the red channel (0-8)
				G = depth for the green channel (0-8)
				B = depth for the blue channel (0-8)
				A = depth for the alpha channel (0-8)
			Each digit takes up 4 bits in the input parameter.
		*/
		public ColorTable(uint rgbaOrderAndDepth, uint[] defSel)
		{
			_rgbaOrder = new byte[4];
			_rgbaOrderInv = new byte[4];
			_rgbaDepth = new byte[4];

			uint cCfg = rgbaOrderAndDepth;

			// Here we take each of the 8 hex digits inside 'cCfg' and place them in their appropriate elements
			for (int i = 0; i < 4; i++)
			{
				int shift = (4 + (3 - i)) * 4;
				uint order = Utils.GetBits(cCfg, 4, shift);
				if (order > 3)
					throw new ArgumentException("RGBA Order Error:\nInvalid order index " + order + " for the " + Utils.RGBANames[i] + " channel (" + i + ")");
	
				uint depth = Utils.GetBits(cCfg, 4, shift - 16);
				if (depth > 8)
					throw new ArgumentException("RGBA Depth Error:\nInvalid bit depth " + depth + " for the " + Utils.RGBANames[i] + " channel (" + i + ")");
	
				_rgbaOrder[i] = (byte)order;
				_rgbaDepth[i] = (byte)depth;
			}

			// We make sure that _rgbaOrder is bijective (1:1 correspondence) to the set {0, 1, 2, 3}
			// This means _rgbaOrder will be bijective with its inverted set (_rgbaOrderInv), a fact we will rely upon later
			for (int i = 0; i < 4; i++)
			{
				int chn = -1, freq = 0;
				for (int j = 0; j < 4; j++)
				{
					if (_rgbaOrder[j] == i)
					{
						chn = j;
						freq++;
					}
				}

				if (chn < 0)
					throw new ArgumentException("RGBA Order Error:\nNo channel for position " + i + " was given");
				if (freq != 1)
					throw new ArgumentException("RGBA Order Error:\nMultiple channels were attempted to placed in the same position");

				_rgbaOrderInv[i] = (byte)chn;
			}

			_clrList = null;
			_defSel = defSel;
		}

		public uint NativeToRGBA(uint idx)
		{
			// If there is a pre-determined list of colors, then 'idx' refers to an index in the table. Return that entry.
			if (_clrList != null)
			{
				uint clr = 0;
				if (idx >= 0 && idx < _clrList.Length)
					clr = _clrList[idx];

				return clr;
			}

			/*
				Else, we turn our 'index' into an RGBA color using our 'rgbaOrderAndDepth' formula descriptor.
				This is something I came up with, so a bit of explanation is always nice.
				I've noticed there are two common attributes of RGBA color that are
				often tailored to particular pixel formats: order of channels and bits per channel.
				'_rgbaOrder' tells us the order of channels as they appear in 'idx', and
				'_rgbaDepth' tells us how many bits are used for the R, G, B and A channels respectively.
			*/

			// This for loop takes the channel values out of 'idx' and places them in RGBA order in the process
			int[] chVals = new int[4];
			for (int i = 3; i >= 0; i--)
			{
				int which = _rgbaOrderInv[i];
				int depth = _rgbaDepth[which];
				chVals[which] = (int)idx & ((1 << depth) - 1);
				idx >>= depth;
			}

			/*
				This loop takes each channel value from above and pads it out as evenly as possible,
				while placing it in the final RGBA value we return.
			*/
			uint rgba = 0;
			for (int i = 0; i < 4; i++)
			{
				int depth = _rgbaDepth[i];
				if (depth == 0)
				{
					if (i != 3)
						rgba <<= 8;

					continue;
				}

				/*
					If we continuously pad the remaining bits with the existing bits, it produces the smoothest pattern.
					Eg.
						00 -> 00000000, 01 -> 01000000, 10 -> 10000000, 11 -> 11000000
					is not as smooth/even as
						00 -> 00000000, 01 -> 01010101, 10 -> 10101010, 11 -> 11111111
					, the latter of which is what this while loop achieves.
				*/
				int val = 0;
				int left = 8;
				while (left > 0)
				{
					int pad = chVals[i];
					if (left < depth)
					{
						pad >>= (depth - left);
						depth = left;
					}

					val <<= depth;
					val |= pad;

					left -= depth;
				}
				val &= 0xff;

				// Place the new channel value in the final output
				rgba |= (uint)val;
				if (i != 3)
					rgba <<= 8;
			}

			return rgba;
		}

		// Only checks red, green and blue
		public static int ClosestColorIndex(uint[] list, uint rgba)
		{
			if (list == null || list.Length < 1)
				return 0;

			double inRed = Utils.ColorAtF(rgba, 24);
			double inGreen = Utils.ColorAtF(rgba, 16);
			double inBlue = Utils.ColorAtF(rgba, 8);

			double lowest = Double.PositiveInfinity;
			int idx = 0;
			for (int i = 0; i < list.Length; i++)
			{
				uint clr = list[idx];
				double clrRed = Utils.ColorAtF(clr, 24);
				double clrGreen = Utils.ColorAtF(clr, 16);
				double clrBlue = Utils.ColorAtF(clr, 8);

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

			return idx;
		}

		public uint RGBAToNative(uint rgba)
		{
			if (_clrList != null)
				return (uint)ClosestColorIndex(_clrList, rgba);

			uint native = 0;
			for (int i = 0; i < 4; i++)
			{
				int which = _rgbaOrderInv[i];
				int depth = _rgbaDepth[which];
				//Console.WriteLine("i = " + i + ", which = " + which + ", depth = " + depth);

				uint channel = Utils.ColorAt(rgba, _rgbaOrder[3 - i] * 8);
				uint val = channel >> (8 - depth);

				native <<= depth;
				native |= val;
			}

			return native;
		}
	}
}
