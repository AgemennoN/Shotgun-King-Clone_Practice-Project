using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour {
    public static EnemyManager Instance { get; private set; }
    private Dictionary<EnemyType, int> enemyDictToCreate;
    public Dictionary<EnemyPiece, int> enemyDict;
    private PieceFactory pieceFactory; 
    private BoardManager boardManager;
    private BoardTile[,] board;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize() {
        pieceFactory = PieceFactory.Instance;
        boardManager = BoardManager.Instance;
        board = boardManager.GetBoard();
        InitEnemyDict();
    }

    private void InitEnemyDict() {
        enemyDictToCreate = new Dictionary<EnemyType, int>();

        enemyDictToCreate.Add(EnemyType.King, 1);
        enemyDictToCreate.Add(EnemyType.Queen, 0);
        enemyDictToCreate.Add(EnemyType.Rook, 1);
        enemyDictToCreate.Add(EnemyType.Bishop, 1);
        enemyDictToCreate.Add(EnemyType.Knight, 1);
        enemyDictToCreate.Add(EnemyType.Pawn, 4);
    }

    public void SpawnEnemies() {
        if (!enemyDictToCreate.ContainsKey(EnemyType.King)) {
            throw new ArgumentException("The dictionary must contain a King enemy.");
        }

        float startCol = 3.5f;
        Queue<EnemyType> placementQueue = CreateSpawnQueue();
        for (int row = 7; row >= 0; row--) {
            bool isPawnRow = (row == 7);

            float offset = 0.5f;
            while (offset < 4f && placementQueue.Count > 0) {
                foreach (int multiplier in new int[] { -1, +1 }) {
                    int col = Mathf.RoundToInt(startCol + offset * multiplier);
                    if (col < 0 || col >= 8)
                        continue;

                    if (board[col, row].pieceOnIt == null) {
                        EnemyType nextPieceType = placementQueue.Peek();

                        // Pawns can't go on row 7
                        if (isPawnRow && nextPieceType is EnemyType.Pawn)
                            continue;

                        nextPieceType = placementQueue.Dequeue();
                        pieceFactory.CreatePieceOnBoard(board, nextPieceType, col, row);

                        if (placementQueue.Count == 0)
                            return;
                    }
                }

                offset += 1f;
            }
        }
    }
    private Queue<EnemyType> CreateSpawnQueue() {
        var queue = new Queue<EnemyType>();

        EnemyType[] priorityOrder = new EnemyType[]
        {
        EnemyType.King,
        EnemyType.Queen,
        EnemyType.Rook,
        EnemyType.Bishop,
        EnemyType.Knight,
        EnemyType.Pawn
        };

        foreach (var enemyType in priorityOrder) {
            if (enemyDictToCreate.TryGetValue(enemyType, out int count)) {
                for (int i = 0; i < count; i++) {
                    queue.Enqueue(enemyType);
                }
            }
        }
        return queue;
    }

}
