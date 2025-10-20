using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPiece : MonoBehaviour {

    protected BoardTile currentTile;

    public void SetPosition(BoardTile tile) {
        if (currentTile != null) {
            currentTile.SetPiece(null); // Remove from old tile
        }
        transform.position = tile.transform.position;
        currentTile = tile;
        tile.SetPiece(this); // Update new tile  
        // TO DO: Might use event that listens when a piece is placed on a Tile to be More OOP maybe?
    }

    public void MoveToPosition(BoardTile tile, System.Action onComplete = null) {
        if (currentTile != null) {
            currentTile.SetPiece(null); // Remove from old tile
            StartCoroutine(MoveToPositionPhysically(tile.transform.position, 0.3f, onComplete));
        } else {
            transform.position = tile.transform.position;
            onComplete?.Invoke(); // Immediately invoke callback if no movement
        }
        currentTile = tile;
        tile.SetPiece(this); // Update new tile  
    }

    private IEnumerator MoveToPositionPhysically(Vector3 targetPos, float duration, System.Action onComplete) {
        Vector3 startPos = transform.position;
        float time = 0f;

        while (time < duration) {
            transform.position = Vector3.Lerp(startPos, targetPos, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos; // Ensure it lands exactly
        onComplete?.Invoke(); // Invoke callback when movement is complete
    }

    // Virtual methods to be overridden in derived piece classes
    public virtual List<BoardTile> GetAvailableMoves(BoardTile[,] board) {
        return new List<BoardTile>();
    }

    public virtual List<BoardTile> GetThreatenedTiles(BoardTile[,] board) {
        return new List<BoardTile>();
    }
}
