using System;

namespace SpriteWave
{
	public class ColorPattern : ColorTable
	{
		// RGBA pattern
		private byte[] _rgbaOrder, _rgbaOrderInv;
		private byte[] _rgbaDepth;

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
		public ColorPattern(uint rgbaOrderAndDepth, uint[] defSel)
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

			_defSel = defSel;
		}

		public override uint NativeToRGBA(uint idx)
		{
			/*
				We turn our 'index' into an RGBA color using our 'rgbaOrderAndDepth' formula descriptor.
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
					, the latter of which this while loop achieves.
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

		public override uint RGBAToNative(uint rgba)
		{
			uint native = 0;
			for (int i = 0; i < 4; i++)
			{
				int which = _rgbaOrderInv[i];
				int depth = _rgbaDepth[which];

				uint channel = Utils.ColorAt(rgba, _rgbaOrder[3 - i] * 8);
				uint val = channel >> (8 - depth);

				native <<= depth;
				native |= val;
			}

			return native;
		}
	}
}