using System;

namespace SpriteWave
{
	public interface IPalette
	{
		uint this[int idx] { get; set; }
		int ColorCount { get; }
		uint[] GetList();
	}
}
