using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

    public static PlayerManager Instance { get; private set; }

    [SerializeField] private GameObject playerPrefab;
    public PlayerPiece playerPiece; // TO DO Should be private to be more OOP
    public List<BoardTile> playerAvailableMoves;
    private BoardTile[,] board;

    private bool actionAvailable;
    private bool isActionPhaseActive;
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
        board = BoardManager.Instance.GetBoard();
        turnManager = TurnManager.Instance;

        turnManager.OnPlayerTurnStarted += StartPlayerTurn;
    }

    public void SpawnPlayer() {
        GameObject obj = Instantiate(playerPrefab, transform);
        playerPiece = obj.GetComponent<PlayerPiece>();
        if (playerPiece == null) {
            Debug.LogError("Prefab does not contain a ChessPiece component.");
            Destroy(obj);
            return;
        }
        playerPiece.SetPosition(board[3, 0]);
    }

    public void StartPlayerTurn() {
        playerAvailableMoves = playerPiece.GetAvailableMoves(board);
        // Maybe Also get safeMoves(not threatened)
        actionAvailable = true;
        isActionPhaseActive = false;
    }

    public void OnTileClicked(BoardTile tile) {
        if (playerAvailableMoves.Contains(tile) && !isActionPhaseActive) {
            actionAvailable = false;
            isActionPhaseActive = true; // Start action phase
            playerPiece.MoveToPosition(tile, OnActionPhaseComplete); // Pass callback
        }
    }

    private void OnActionPhaseComplete() {
        isActionPhaseActive = false; // End action phase
        EndTurn();
    }


    public bool IsActionAvailable() {
        return actionAvailable;
    }

    private void EndTurn() {
        turnManager.EndTurn();
    }
}
