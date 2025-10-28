using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour {
    public static EnemyManager Instance { get; private set; }

    private Dictionary<EnemyType, int> enemyDictToCreate;
    public Dictionary<EnemyType, List<GameObject>> enemyDict;

    private PieceFactory pieceFactory; 
    private BoardManager boardManager;
    private BoardTile[,] board;
    private TurnManager turnManager;

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
        turnManager = TurnManager.Instance;

        turnManager.OnEnemyTurnStarted += StartEnemyTurn;

        board = boardManager.GetBoard();
        InitEnemyDict();
    }

    public void StartEnemyTurn() {
        //TO DO: Each enemy piece should to An action
        AllTakeAction();
        EndTurn();
    }

    private void AllTakeAction() {
        // Define the order manually
        EnemyType[] executionOrder =
        {
            EnemyType.Pawn,
            EnemyType.Knight,
            EnemyType.Rook,
            EnemyType.Queen,
            EnemyType.Bishop,
            EnemyType.King
        };

        // Iterate through the order and call TakeAction() on each
        foreach (EnemyType type in executionOrder) {
            if (!enemyDict.ContainsKey(type))
                continue;

            foreach (GameObject enemy in enemyDict[type]) {
                if (enemy == null) continue;

                EnemyPiece piece = enemy.GetComponent<EnemyPiece>();
                if (piece != null)
                    piece.TakeAction();
                else
                    Debug.LogWarning($"GameObject {enemy.name} has no EnemyPiece component.");
            }
        }
    }

    private void InitEnemyDict() {
        enemyDictToCreate = new Dictionary<EnemyType, int>();

        enemyDictToCreate.Add(EnemyType.King, 1);
        enemyDictToCreate.Add(EnemyType.Queen, 0);
        enemyDictToCreate.Add(EnemyType.Rook, 1);
        enemyDictToCreate.Add(EnemyType.Bishop, 1);
        enemyDictToCreate.Add(EnemyType.Knight, 1);
        enemyDictToCreate.Add(EnemyType.Pawn, 4);


        enemyDict = new Dictionary<EnemyType, List<GameObject>>();
        foreach (EnemyType type in System.Enum.GetValues(typeof(EnemyType))) {
            enemyDict[type] = new List<GameObject>();
        }
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
                        GameObject newPiece = pieceFactory.CreatePieceOnBoard(board, nextPieceType, col, row, transform);

                        enemyDict[nextPieceType].Add(newPiece);

                        if (placementQueue.Count == 0) {
                            PrintEnemyDict();
                            return;
                        }
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

    private void EndTurn() {
        turnManager.EndTurn();
    }

    public void PrintEnemyDict() {
        foreach (var kvp in enemyDict) {
            string message = $"{kvp.Key}: ";
            foreach (GameObject enemy in kvp.Value) {
                message += enemy != null ? enemy.name + ", " : "null, ";
            }
            Debug.Log(message);
        }
    }

}
