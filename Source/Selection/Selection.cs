using System;
using System.Drawing;

namespace SpriteWave
{
	public interface IPiece
	{
		EdgeKind EdgeKind { get; }
		
		IPiece Clone();
	}

	public interface ISelection
	{
		void Receive(IPiece isel);
		void DrawSelection(Graphics g);
		void Delete();

		Position Location { get; set; }
		IPiece Piece { get; }
		bool IsActive { get; }

		//void Move(int dCol, int dRow);
		//void SetPos(int )
	}
}