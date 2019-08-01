using System;

namespace SpriteWave
{
	public struct Vec3
	{
		private float[] _vec;
		public float X { get { return _vec[0]; } set { _vec[0] = value; } }
		public float Y { get { return _vec[1]; } set { _vec[1] = value; } }
		public float Z { get { return _vec[2]; } set { _vec[2] = value; } }

		public float this[int idx] { get { return _vec[idx]; } set { _vec[idx] = value; } }

		public Vec3(float f)
		{
			_vec = new[] { f, f, f };
		}
		public Vec3(float x, float y, float z)
		{
			_vec = new[] { x, y, z };
		}
		public Vec3(byte x, byte y, byte z)
		{
			_vec = new[] {
				(float)x / 255f,
				(float)y / 255f,
				(float)z / 255f
			};
		}

		public int[] LowMidHigh()
		{
			Action<float[], int[], int, int> check = (vec, list, a, b) =>
			{
				if (vec[list[a]] > vec[list[b]])
				{
					int temp = list[a];
					list[a] = list[b];
					list[b] = temp;
				}
			};

			int[] order = new[] {0, 1, 2};
			check(_vec, order, 0, 1);
			check(_vec, order, 1, 2);
			check(_vec, order, 0, 2);
			check(_vec, order, 0, 1);

			return order;
		}

		public byte[] ByteVector()
		{
			return new[] {
				(byte)(_vec[0] * 255f),
				(byte)(_vec[1] * 255f),
				(byte)(_vec[2] * 255f)
			};
		}
	}

	public static class ColorSpace
	{
		private static int[] orderCombs = {21, 15, 7, 5, 11, 19};

		public static Vec3 RGBtoHSL(Vec3 rgb)
		{
			// Find the order of brightnesses for the R, G and B channels
			int[] order = rgb.LowMidHigh();

			// The lightness is always equal to the RGB channel with the largest value, be it red, green or blue.
			float lum = rgb[order[2]];
			if (lum == 0f)
				return new Vec3(0f);

			// The saturation is proportional to lowest RGB channel value over the highest RGB channel value
			float sat = 1f - (rgb[order[0]] / lum);
			if (sat == 0f)
				return new Vec3(0f, 0f, lum);

			/*
				The hue of the input colour, represented as its own RGB colour,
				will have the lowest channel (eg. green) set to 0,
				the highest channel (eg. red) set to 1,
				and the remaining channel set to where it was in proportion to the lowest and highest channels.
			*/
			float hueMid = (rgb[order[1]] - rgb[order[0]]) / (rgb[order[2]] - rgb[order[0]]);

			int[] hueRgb = new int[3];
			hueRgb[order[0]] = 0;
			hueRgb[order[1]] = (int)(hueMid * 255f);
			hueRgb[order[2]] = 255;

			// When combinining the order values in this way, each of the six combinations produces a different number.
			// We take advantage of this fact to look up this number in a hard-coded array.
			int comb = order[0] * 9 + order[1] * 3 + order[2];

			// 'hueSlice' refers to which 6th of the hue wheel the input colour is in.
			// In order, the hue slices/sections are red, yellow, green, cyan, blue, magenta.
			int hueSlice = Array.IndexOf(orderCombs, comb);

			// Every 2nd hue slice, the fractional (mid) part of the colour (as a hue) falls rather than rises.
			float hueFrac = hueSlice % 2 == 1 ? 1f - hueMid : hueMid;

			float hue = ((float)hueSlice + hueFrac) / 6f;
			return new Vec3(hue, sat, lum);
		}

		public static Vec3 HSLtoRGB(Vec3 hsl)
		{
			float hue = hsl[0];
			float invSat = 1f - hsl[1];
			float lum = hsl[2];

			int hueSlice = (int)(hue * 6f);
			float hueFrac = (hue * 6f) - hueSlice;

			int comb = orderCombs[hueSlice];
			int[] order = new int[3];
			for (int i = 2; i >= 0; i--)
			{
				order[i] = comb % 3;
				comb /= 3;
			}

			float hueMid = (hueSlice % 2) == 1 ? 1f - hueFrac : hueFrac;

			Vec3 rgb = new Vec3(0f);
			rgb[order[0]] = lum * invSat;
			rgb[order[1]] = lum * (hueMid + (1f - hueMid) * rgb[order[0]]);
			rgb[order[2]] = lum;

			return rgb;
		}
	}
}
