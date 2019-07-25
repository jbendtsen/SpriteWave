using System;

namespace SpriteWave
{
	public interface ITabCollection
	{
		ITab this[int idx] { get; }
		ITab this[string name] { get; }
		int TabCount { get; }

		int TabIndex(ITab t);
	}
}
