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
		IPiece Piece { get; }

		//void Move(int dCol, int dRow);
		//void SetPos(int )
	}
}
