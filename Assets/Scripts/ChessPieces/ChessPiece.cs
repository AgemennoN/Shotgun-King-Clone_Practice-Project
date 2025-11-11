using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPiece : MonoBehaviour {

    public VisualEffects visualEffects;

    protected BoardTile currentTile;
    [SerializeField] protected List<BoardTile> availableTiles;
    [SerializeField] protected List<BoardTile> threatenedTiles;
    public static float movementDuration = 0.5f;
    public static float captureMovementDuration = 2f;

    protected virtual void Awake() {
        visualEffects = GetComponent<VisualEffects>();
    }

    public void SetPosition(BoardTile tile) {
        if (currentTile != null) {
            currentTile.SetPiece(null); // Remove from old tile
        }
        transform.position = tile.transform.position;
        currentTile = tile;
        tile.SetPiece(this); // Update new tile  
        // TO DO: Might use event that listens when a piece is placed on a Tile to be More OOP maybe?
    }

    public void MoveToPosition(BoardTile tile) {
        if (currentTile != null) {
            currentTile.SetPiece(null); // Remove from old tile
            TurnManager.Instance.RegisterAction(MoveToPositionPhysically(tile.transform.position, movementDuration));
        } else {
            transform.position = tile.transform.position;
        }
        currentTile = tile;
        tile.SetPiece(this); // Update new tile  
    }

    private IEnumerator MoveToPositionPhysically(Vector3 targetPos, float duration, float jumpHeight = 0.5f) {
        Vector3 startPos = transform.position;
        float time = 0f;

        while (time < duration) {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);

            // Lerp position between start and target
            Vector3 horizontalPos = Vector3.Lerp(startPos, targetPos, t);

            // Add a jump arc using a parabola: height = 4h * t * (1 - t)
            float height = 4f * jumpHeight * t * (1f - t);

            // Apply the vertical offset
            transform.position = horizontalPos + Vector3.up * height;

            yield return null;
        }
        transform.position = targetPos;
    }

    public BoardTile GetTile() { return currentTile; }

    public virtual void UpdateAvailableTiles(BoardTile[,] board) {
    }

    public virtual void UpdateThreatenedTiles(BoardTile[,] board) {
    }

    public static List<BoardTile> GetTilesFromPatternList(BoardTile[,] board, Vector2Int currentPos, List<MovementPattern> patterns, bool canCapture) {
        List<BoardTile> tiles = new List<BoardTile>();

        foreach (MovementPattern pattern in patterns) {
            switch (pattern.movementType) {
                case MovementType.Jump:
                    HandleJumpMove(board, currentPos, pattern, tiles, canCapture);
                    break;

                case MovementType.FiniteStep:
                    HandleFiniteStepMove(board, currentPos, pattern, tiles, canCapture);
                    break;

                case MovementType.InfiniteStep:
                    HandleInfiniteStepMove(board, currentPos, pattern, tiles, canCapture);
                    break;
            }
        }
        return tiles;
    }

    public static void HandleJumpMove(BoardTile[,] board, Vector2Int currentPos, MovementPattern pattern, List<BoardTile> availableTiles, bool canCapture) {
        Vector2Int targetPos = currentPos + pattern.direction;

        if (!BoardManager.IsInsideBounds(targetPos))
            return;

        BoardTile targetTile = board[targetPos.x, targetPos.y];
        ChessPiece pieceOnTheTile = targetTile.GetPiece();
        if (pieceOnTheTile == null || (pieceOnTheTile is PlayerPiece && canCapture))
            availableTiles.Add(targetTile);
    }

    public static void HandleFiniteStepMove(BoardTile[,] board, Vector2Int currentPos, MovementPattern pattern, List<BoardTile> availableTiles, bool canCapture) {
        for (int step = 1; step <= pattern.maxDistance; step++) {
            Vector2Int targetPos = currentPos + pattern.direction * step;

            if (!BoardManager.IsInsideBounds(targetPos))
                break;

            BoardTile targetTile = board[targetPos.x, targetPos.y];
            ChessPiece pieceOnTheTile = targetTile.GetPiece();

            if (pieceOnTheTile == null) {
                availableTiles.Add(targetTile);
            } else {
                if (pieceOnTheTile is PlayerPiece && canCapture) {
                    // PlayerPiece can be captured so the tile is available
                    availableTiles.Add(targetTile);
                    continue;
                }
                // Stop if blocked by another piece
                break;
            }
        }
    }

    public static void HandleInfiniteStepMove(BoardTile[,] board, Vector2Int currentPos, MovementPattern pattern, List<BoardTile> availableTiles, bool canCapture) {
        for (int step = 1; ; step++) {
            Vector2Int targetPos = currentPos + pattern.direction * step;

            if (!BoardManager.IsInsideBounds(targetPos))
                break;

            BoardTile targetTile = board[targetPos.x, targetPos.y];
            ChessPiece pieceOnTheTile = targetTile.GetPiece();

            if (pieceOnTheTile == null) {
                availableTiles.Add(targetTile);
            } else {
                if (pieceOnTheTile is PlayerPiece && canCapture) {
                    // PlayerPiece can be captured so the tile is available
                    availableTiles.Add(targetTile);
                    continue;
                }
                // Stop if blocked by another piece
                break;
            }
        }
    }



    public virtual List<BoardTile> GetAvailableTiles() {
        return availableTiles;
    }

    public virtual List<BoardTile> GetThreatenedTiles() {
        return threatenedTiles;
    }
}
