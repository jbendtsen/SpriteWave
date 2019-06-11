using System;
using System.Drawing;

namespace SpriteWave
{
	public interface ISelection
	{
		void Receive(ISelection isel);
		void DrawSelection(TileWindow wnd, Graphics g);

		ISelection Selection { get; set; }
		Position Location { get; set; }
		Tile Tile { get; }

		//void Move(int dCol, int dRow);
		//void SetPos(int )
	}
}
