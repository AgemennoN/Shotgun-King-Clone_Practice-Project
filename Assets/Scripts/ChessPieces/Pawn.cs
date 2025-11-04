using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pawn : EnemyPiece {

    public override List<BoardTile> GetThreatenedTiles(BoardTile[,] board) {
        Vector2Int position = currentTile.GridPosition;
        List<BoardTile> threatenedTiles = new List<BoardTile>();
        if (position.x > 0) {
            if (position.y > 0) {
                threatenedTiles.Add(board[position.x - 1, position.y - 1]);
            }
            if (position.y < board.Length - 1) {
                threatenedTiles.Add(board[position.x - 1, position.y + 1]);
            }
        }
        return threatenedTiles;
    }


}
