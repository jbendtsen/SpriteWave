using System;

namespace SpriteWave
{
	/*
		ColorTable: A way of keeping track of which colours the system (NES, SNES, etc.) has access to.
		There are two different ways a ColorTable is created: with a pre-defined set of colours, or with an RGBA pattern.
		The RGBA pattern is interpreted as a RGBA Order And Depth formula. More detail on that one is provided below.
	*/
	public abstract class ColorTable
	{
		// Default selection of colours, in the native format. Useful if a palette has not yet been decided.
		protected uint[] _defSel;
		public uint[] Defaults { get { return _defSel; } }

		public abstract uint NativeToRGBA(uint idx);

		public abstract uint RGBAToNative(uint rgba);
	}
}
