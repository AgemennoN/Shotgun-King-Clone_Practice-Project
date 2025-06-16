using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

    public static PlayerManager Instance { get; private set; }
    [SerializeField] private GameObject playerPrefab;
    public PlayerPiece playerPiece; // TO DO Should be private to be more OOP
    private BoardTile[,] board;
    public List<BoardTile> playerAvailableMoves;
    public bool playerTurn;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        board = BoardManager.Instance.GetBoard();
    }

    public void SpawnPlayer() {
        GameObject obj = Instantiate(playerPrefab);
        playerPiece = obj.GetComponent<PlayerPiece>();
        if (playerPiece == null) {
            Debug.LogError("Prefab does not contain a ChessPiece component.");
            Destroy(obj);
            return;
        }
        playerPiece.SetPosition(board[3, 0]);
    }

    public void PlayerTurnStart() {
        playerTurn = true;
        playerAvailableMoves = playerPiece.GetAvailableMoves(board);
        // Maybe Also get safeMoves(not threatened)
    }

    private void EndTurn() {
        playerTurn = false;
    }
}
