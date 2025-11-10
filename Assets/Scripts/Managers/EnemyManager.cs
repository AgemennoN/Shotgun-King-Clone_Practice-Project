using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour {
    public static EnemyManager Instance { get; private set; }
    public static Action onEnemyCheckMatesThePlayer;

    private Dictionary<EnemyType, int> enemyDictToCreate;
    private Dictionary<EnemyType, List<GameObject>> enemyDict;

    private PieceFactory pieceFactory; 
    private BoardManager boardManager;
    private TurnManager turnManager;

    private EnemyType[] executionOrder =
        {
            EnemyType.Pawn,
            EnemyType.King,
            EnemyType.Queen,
            EnemyType.Bishop,
            EnemyType.Rook,
            EnemyType.Knight
        };

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Pawn.OnPawnPromoted += HandlePawnPromoted;
    }

    public void Initialize() {
        pieceFactory = GetComponent<PieceFactory>();
        boardManager = BoardManager.Instance;
        turnManager = TurnManager.Instance;

        turnManager.OnEnemyTurnStarted += StartEnemyTurn;

        InitEnemyDict();
    }

    private void StartEnemyTurn() {
        bool gameEnd = !IsEnemyKingAliveControl();

        if (!gameEnd)
            gameEnd = AllCheckControlForMate();

        if (!gameEnd) {
            AllTakeAction();
            StartCoroutine(TurnManager.Instance.StartActionPhase(true));
        }
    }

    private bool AllCheckControlForMate() {
        EnemyPiece checkingPiece = null;

        foreach (EnemyType type in executionOrder) {
            if (!enemyDict.ContainsKey(type))
                continue;

            foreach (GameObject enemy in enemyDict[type]) {
                if (enemy == null) continue;

                EnemyPiece piece = enemy.GetComponent<EnemyPiece>();
                if (piece != null) {
                    if (checkingPiece == null) checkingPiece = piece.CheckControl(true); // Get the first piece that checks to Mate
                    else piece.CheckControl(true); // Show the threat with the rest of the checking pieces but don't need to hold them
                } else
                    Debug.LogWarning($"GameObject {enemy.name} has no EnemyPiece component.");
            }
        }
        if (checkingPiece != null) {
            checkingPiece.CheckMateTheKing();
            return true;
        }
        return false;
    }

    public bool IsTileInThreatened(BoardTile tile, bool showThreat=false) {
        // Iterate through the order and call IsTileIsInThreatened() on each
        bool inThreat = false;

        foreach (EnemyType type in executionOrder) {
            if (!enemyDict.ContainsKey(type))
                continue;

            foreach (GameObject enemy in enemyDict[type]) {
                if (enemy == null) continue;

                EnemyPiece piece = enemy.GetComponent<EnemyPiece>();
                if (piece != null) {
                    if (piece.IsTileIsInThreatened(tile, showThreat)) inThreat = true; ;
                }
                else
                    Debug.LogWarning($"GameObject {enemy.name} has no EnemyPiece component.");
            }
        }
        return inThreat;
    }

    private bool IsEnemyKingAliveControl() {
        // Check if there is any King left in the dictionary if not the stage will end
        return true;
    }

    private void AllTakeAction() {
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

    public void SpawnEnemyDictOfTheMap() {
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

                    if (BoardManager.Board[col, row].GetPiece() == null) {
                        EnemyType nextPieceType = placementQueue.Peek();

                        // Pawns can't go on row 7
                        if (isPawnRow && nextPieceType is EnemyType.Pawn)
                            continue;

                        nextPieceType = placementQueue.Dequeue();
                        GameObject newPiece = pieceFactory.CreatePieceOnBoard(BoardManager.Board, nextPieceType, col, row, transform);

                        RegisterToEnemyDict(nextPieceType, newPiece);

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

    private void RegisterToEnemyDict(EnemyType enemyType, GameObject newPieceObj) {
        enemyDict[enemyType].Add(newPieceObj);

        EnemyPiece newPiece = newPieceObj.GetComponent<EnemyPiece>();
        newPiece.OnDeath += HandleEnemyDeath;
    }

    private void HandleEnemyDeath(EnemyPiece piece) {
        piece.OnDeath -= HandleEnemyDeath;
        if (enemyDict.TryGetValue(piece.GetEnemyTypeSO().enemyType, out var list)) {
            list.Remove(piece.gameObject);
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

    private void HandlePawnPromoted(Pawn pawn, EnemyType newType) {
        pawn.OnDeath -= HandleEnemyDeath;
        BoardTile pawnTile = pawn.GetTile();

        if (enemyDict.TryGetValue(EnemyType.Pawn, out var list)) {
            list.Remove(pawn.gameObject);
        }

        GameObject newPiece = pieceFactory.CreatePieceOnBoard(BoardManager.Board, newType, pawnTile.GridPosition.x, pawnTile.GridPosition.y, transform);
        RegisterToEnemyDict(newType, newPiece);

        VisualEffects visualEffects = newPiece.GetComponent<VisualEffects>();
        visualEffects.SpriteFadeInAnimation(Pawn.PromotionDuration, true);
    }

    private void EndTurn() {
        turnManager.EndEnemyTurn();
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

    private void OnDisable() {
        Pawn.OnPawnPromoted -= HandlePawnPromoted;
    }
}
