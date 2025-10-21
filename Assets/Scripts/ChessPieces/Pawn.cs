using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pawn : EnemyPiece {

    private void Awake() {
        maxHealth = 3;
        currenHealth = maxHealth;
        speed = 5;
        cooldownToMove = Random.Range(2, speed+1);
        readyToMove = false;
    }

    private void Start() {
        board = BoardManager.Instance.GetBoard();
    }

    public override List<BoardTile> GetAvailableMoves(BoardTile[,] board) {
        if (board == null) {
            board = BoardManager.Instance.GetBoard();
        }
        // TO DO: GetPosition is not logical to be in local methods and called in every use FIX
        Vector2Int position = currentTile.GetPosition();
        List<BoardTile> availableTiles = new List<BoardTile>();

        if (position.y > 0 && (board[position.x, position.y - 1].pieceOnIt == null)) {
            // TO DO: PERK ~ Check if it is in Thereataned Tiles of the PlayerPiece
            availableTiles.Add(board[position.x, position.y - 1]);

            // TO DO: PERK
            //if (position.x == 6 && (board[position.x - 2, position.y].pieceOnIt == null)) {
            //    availableTiles.Add(board[position.x - 2, position.y]);
            //}
        }
        return availableTiles;
    }

    public override List<BoardTile> GetThreatenedTiles(BoardTile[,] board) {
        Vector2Int position = currentTile.GetPosition();
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
