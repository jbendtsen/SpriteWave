﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace SpriteWave
{
	public interface ITab
	{
		string Name { get; }
		string ID { get; }
		Button TabButton { get; }
		Panel Panel { get; }
		TileWindow Window { get; set; }
		Size Minimum { get; }
		int X { set; }

		bool HandleEscapeKey(MainForm main);
		void AdjustContents(Size size, ToolBoxOrientation layout);
		void Destruct();
	}
}
