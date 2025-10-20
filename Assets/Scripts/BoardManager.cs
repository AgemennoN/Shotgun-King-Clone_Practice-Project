using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour {

    public static BoardManager Instance { get; private set; }
    private float tileSize = 1f;
    private BoardTile[,] board = new BoardTile[8, 8];
    [SerializeField] private Sprite blackTileSprite;
    [SerializeField] private Sprite whiteTileSprite;


    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() {
    }

    public void Initialize() {
        GenerateBoard(tileSize, 8, 8);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.P)) {
            PrintBoard();
        }
    }

    public Tile GetClosestAvailableTile(Tile tile) {
        Tile closestAvailableTile = new Tile();

        // To DO: Might be used for spawning pieces but for now we will do king's right and left 

        return closestAvailableTile;
    }

    public bool IsTileEmtpy(int tileCountX, int tileCountY) {
        return board[tileCountX, tileCountY].GetPiece() == null;
    }
    private void GenerateBoard(float tileSize, int tileCountX, int tileCountY) {
        for (int x = 0; x < tileCountX; x++) {
            for (int y = 0; y < tileCountY; y++) {
                Sprite sprite = ((x + y) % 2 == 0) ? blackTileSprite : whiteTileSprite;
                board[x, y] = BoardTile.Create(tileSize, x, y, sprite, null, gameObject);
            }
        }
    }

    public BoardTile[,] GetBoard() {
        return board;
    }

    public void PrintBoard() {
        for (int x = 0; x < 8; x++) {
            for (int y = 0; y < 8; y++) {

                Debug.Log($"board[{x},{y}]: {board[x, y].GetPiece()}");
            }
            Debug.LogWarning(" ");
        }
    }
}
