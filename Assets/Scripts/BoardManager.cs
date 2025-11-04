using Unity.VisualScripting;
using UnityEngine;

public class BoardManager : MonoBehaviour {
    public static BoardManager Instance { get; private set; }

    public static BoardTile[,] Board { get; private set; }

    [SerializeField] private int boardWidth = 8;
    [SerializeField] private int boardHeight = 8;
    [SerializeField] private Sprite blackTileSprite;
    [SerializeField] private Sprite whiteTileSprite;
    [SerializeField] private float spriteScaleFactor = 4.5f;
    public int BoardWidth => boardWidth;
    public int BoardHeight => boardHeight;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize() {
        GenerateBoard();
    }

    private void GenerateBoard() {
        if (Board != null)
            return; // already generated Might destroy the Board Later DestroyBoard()

        Board = new BoardTile[boardWidth, boardHeight];

        float tileWorldSize = blackTileSprite.bounds.size.x * spriteScaleFactor;

        // Calculate offset so board is centered
        float offsetX = (boardWidth - 1) * tileWorldSize / 2f;
        float offsetY = (boardHeight - 1) * tileWorldSize / 2f;

        for (int x = 0; x < boardWidth; x++) {
            for (int y = 0; y < boardHeight; y++) {
                Sprite sprite = ((x + y) % 2 == 0) ? blackTileSprite : whiteTileSprite;
                Board[x, y] = BoardTile.Create(new Vector2Int(x, y), spriteScaleFactor, sprite, transform);
            }
        }
    }

    public bool IsTileEmpty(int x, int y) {
        return Board[x, y].GetPiece() == null;
    }

    private void DestroyBoard() {
        if (Board != null) {
            // destroy old tiles
            foreach (BoardTile tile in Board)
                if (tile != null)
                    Destroy(tile.gameObject);
        }
    }

    public static bool IsInsideBounds(Vector2Int pos) {
        return pos.x >= 0 && pos.x < Board.GetLength(0) &&
               pos.y >= 0 && pos.y < Board.GetLength(1);
    }

    public void PrintBoard() {
        for (int x = 0; x < boardWidth; x++) {
            for (int y = 0; y < boardHeight; y++) {
                Debug.Log(Board[x, y].ToString());
            }
            Debug.LogWarning(" ");
        }
    }
}
