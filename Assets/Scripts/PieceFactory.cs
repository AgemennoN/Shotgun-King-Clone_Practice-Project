using UnityEngine;

public class PieceFactory : MonoBehaviour {
    public static PieceFactory Instance { get; private set; }

    public GameObject kingPrefab;
    public GameObject queenPrefab;
    public GameObject rookPrefab;
    public GameObject knightPrefab;
    public GameObject bishopPrefab;
    public GameObject pawnPrefab;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public GameObject CreatePieceOnBoard(BoardTile[,] board, EnemyType enemyType, int x, int y, Transform parent) {
        // x is column, y is row
        if (board[x, y].GetPiece() != null) {
            Debug.LogWarning($"board[{x},{y}] already has a piece.");
            return null;
        }

        GameObject prefab = GetPrefabForEnemyType(enemyType);
        if (prefab == null) {
            Debug.LogError($"No prefab found for enemy type: {enemyType}");
            return null;
        }

        GameObject obj = Instantiate(prefab,parent.transform);
        ChessPiece piece = obj.GetComponent<ChessPiece>();
        if (piece == null) {
            Debug.LogError("Prefab does not contain a ChessPiece component.");
            Destroy(obj);
            return null;
        }
        piece.SetPosition(board[x,y]);
        return obj;
    }

    private GameObject GetPrefabForEnemyType(EnemyType enemyType) {
        switch (enemyType) {
            case EnemyType.King: return kingPrefab;
            case EnemyType.Queen: return queenPrefab;
            case EnemyType.Rook: return rookPrefab;
            case EnemyType.Bishop: return bishopPrefab;
            case EnemyType.Knight: return knightPrefab;
            case EnemyType.Pawn: return pawnPrefab;
            default: return null;
        }
    }
}
