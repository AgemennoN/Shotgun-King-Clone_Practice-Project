using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class EnemyManager : MonoBehaviour {
    public static EnemyManager Instance { get; private set; }
    public static Dictionary<EnemyType, int> enemyDictToCreate;

    public static Action onEnemyCheckMatesThePlayer;
    public static Action onEnemyKingsDie;

    private Dictionary<EnemyType, List<EnemyPiece>> enemyDict;

    private PieceFactory pieceFactory; 
    private BoardManager boardManager;
    private TurnManager turnManager;

    private readonly EnemyType[] executionOrder =
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

        InitEnemyDictToCreate();
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
        List<EnemyPiece> allEnemies = enemyDict.Values.SelectMany(list => list).ToList();
        List<EnemyPiece> checkingEnemies = new List<EnemyPiece>();

        foreach (EnemyPiece enemy in allEnemies) {
            if (enemy.CheckControl(true)) checkingEnemies.Add(enemy);
        }

        if (checkingEnemies.Count > 0) {
            // Randomly one of the checking pieces captures the king
            checkingEnemies[UnityEngine.Random.Range(0, checkingEnemies.Count)].CapturePlayer();
            return true;
        }
        return false;
    }

    public bool IsTileInThreatened(BoardTile tile, bool showThreat=false) {
        bool inThreat = false;

        List<EnemyPiece> allEnemies = enemyDict.Values.SelectMany(list => list).ToList();
        foreach (EnemyPiece enemy in allEnemies) {
            if (enemy.IsTileIsInThreatened(tile, showThreat))
                inThreat = true;
        }

        return inThreat;
    }

    private bool IsEnemyKingAliveControl() {
        if (enemyDict[EnemyType.King].Count > 0) return true;
        KillAllEnemies();
        return false;
    }

    private void AllTakeAction() {
        // Iterate through the order and call TakeAction() on each
        foreach (EnemyType type in executionOrder) {
            foreach (EnemyPiece enemy in enemyDict[type]) {
                enemy.TakeAction();
            }
        }
    }

    private void KillAllEnemies() {
        List<EnemyPiece> allEnemies = enemyDict.Values.SelectMany(list => list).ToList();
        foreach (EnemyPiece enemy in allEnemies) {
            enemy.Die();
        }
        onEnemyKingsDie?.Invoke();
    }

    private void InitEnemyDictToCreate() {
        if (enemyDictToCreate != null)
            return;

        enemyDictToCreate = new Dictionary<EnemyType, int>();

        enemyDictToCreate.Add(EnemyType.King, 1);
        enemyDictToCreate.Add(EnemyType.Queen, 0);
        enemyDictToCreate.Add(EnemyType.Rook, 1);
        enemyDictToCreate.Add(EnemyType.Bishop, 1);
        enemyDictToCreate.Add(EnemyType.Knight, 1);
        enemyDictToCreate.Add(EnemyType.Pawn, 4);
    }

    private void InitEnemyDict() {
        enemyDict = new Dictionary<EnemyType, List<EnemyPiece>>();
        foreach (EnemyType type in System.Enum.GetValues(typeof(EnemyType))) {
            enemyDict[type] = new List<EnemyPiece>();
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
                            return;
                        }
                    }
                }

                offset += 1f;
            }
        }
    }

    private void RegisterToEnemyDict(EnemyType enemyType, GameObject newPieceObj) {
        EnemyPiece enemyPiece = newPieceObj.GetComponent<EnemyPiece>();

        enemyDict[enemyType].Add(enemyPiece);
        enemyPiece.OnDeath += HandleEnemyDeath;

    }

    private void HandleEnemyDeath(EnemyPiece enemyPiece) {
        enemyPiece.OnDeath -= HandleEnemyDeath;

        if (enemyDict.TryGetValue(enemyPiece.GetEnemyTypeSO().enemyType, out var list)) {
            list.Remove(enemyPiece);
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
            list.Remove(pawn);
        }

        GameObject newPiece = pieceFactory.CreatePieceOnBoard(BoardManager.Board, newType, pawnTile.GridPosition.x, pawnTile.GridPosition.y, transform);
        RegisterToEnemyDict(newType, newPiece);

        VisualEffects visualEffects = newPiece.GetComponent<VisualEffects>();
        visualEffects.SpriteFadeInAnimation(Pawn.PromotionDuration, true);
    }

    private void OnDisable() {
        Pawn.OnPawnPromoted -= HandlePawnPromoted;
    }

}
