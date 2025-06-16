using System.Collections.Generic;
using UnityEngine;

public class PlayerPiece : ChessPiece
{
    public override List<BoardTile> GetAvailableMoves(BoardTile[,] board) {
        List<BoardTile> tiles = new List<BoardTile>();

        Vector2Int currentPosition = currentTile.GetPosition();

        // Check 8 adjacent directions
        for (int dx = -1; dx <= 1; dx++) {
            for (int dy = -1; dy <= 1; dy++) {
                // Skip the current square
                if (dx == 0 && dy == 0)
                    continue;

                int newX = currentPosition.x + dx;
                int newY = currentPosition.y + dy;

                // Bounds check
                if (newX >= 0 && newX < board.GetLength(0) && newY >= 0 && newY < board.GetLength(1)) {
                    if (board[newX,newY].GetPiece() == null)
                        tiles.Add(board[newX, newY]);
                }
            }
        }

        return tiles;
    }


}
