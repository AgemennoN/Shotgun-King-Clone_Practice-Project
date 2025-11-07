using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

    public static PlayerManager Instance { get; private set; }

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private ArrowIndicator arrowIndicator;
    [SerializeField] private BoardInputBroadcaster boardInputBroadcaster;

    private TurnManager turnManager;
    private PlayerPiece playerPiece;
    private List<BoardTile> playerAvailableMoves;
    private Weapon weapon;

    private int shieldChargesLimit = 2;  // Number of times the player is protected from moving onto a threatened tile.
    private int shieldChargesRemaining;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize() {
        turnManager = TurnManager.Instance;

        boardInputBroadcaster.OnTileHovered += HandleTileHover;
        boardInputBroadcaster.OnTileClicked += HandleTileClick;

        turnManager.OnPlayerTurnStarted += StartPlayerTurn;
    }

    private void HandleTileHover(BoardTile tile) {
        if (!TurnManager.Instance.IsPlayerTurn())
            return;

        if (tile != null)
            if (playerAvailableMoves.Contains(tile)) {
                Vector3 from = playerPiece.GetTile().transform.position;
                Vector3 to = tile.transform.position;
                arrowIndicator.Show(from, to);
                weapon.Aim(false);
            } else {
                arrowIndicator.Hide();
                weapon.Aim(true);
            }
        else {
            arrowIndicator.Hide();
            weapon.Aim(false);
        }
    }

    private void HandleTileClick(BoardTile tile, Vector3 mouseWorldPos) {
        if (!TurnManager.Instance.IsPlayerTurn())
            return;

        if (tile != null) {
            if (playerAvailableMoves.Contains(tile)) {
                if (IsActionApprovedByShieldProtection(tile)) {
                    arrowIndicator.Hide();
                    MakeKingMovementTo(tile);
                    weapon.Reload(); // Reload logic done inside weapon
                    StartCoroutine(TurnManager.Instance.StartActionPhase(true));        // When all the registered coroutines end, End Turn
                } else {
                    Debug.Log($"TILE: {tile} IS NOT SAFE to MOVE there");
                }
            } else if (tile != playerPiece.GetTile()) {
                if (IsActionApprovedByShieldProtection(GetPlayersTile())) {
                    if (weapon.Shoot(mouseWorldPos)) {
                        StartCoroutine(TurnManager.Instance.StartActionPhase(true));    // When all the registered coroutines end, End Turn
                    }
                } else {
                    Debug.Log($"TILE: {tile} IS NOT SAFE to STAY and Shoot");
                }

            }
        }

    }

    private void MakeKingMovementTo(BoardTile tile) {
        if (playerAvailableMoves.Contains(tile)) {
            playerPiece.MoveToPosition(tile); // Pass callback
        }
    }

    public void SpawnPlayer() {
        // TO DO: Make it private
        GameObject obj = Instantiate(playerPrefab, transform);
        playerPiece = obj.GetComponent<PlayerPiece>();
        if (playerPiece == null) {
            Debug.LogError("Prefab does not contain a ChessPiece component.");
            Destroy(obj);
            return;
        }
        playerPiece.SetPosition(BoardManager.Board[3, 0]);
        weapon = obj.GetComponentInChildren<Weapon>();
    }

    private void StartPlayerTurn() {
        playerPiece.UpdateAvailableTiles(BoardManager.Board);
        playerAvailableMoves = playerPiece.GetAvailableTiles();
        RestoreShieldCharges();
        // Maybe Also get safeMoves(not threatened)
    }

    private bool IsActionApprovedByShieldProtection(BoardTile tile) {
        if (shieldChargesRemaining > 0) { // Protected by shields
            bool inThreat = EnemyManager.Instance.IsTileInThreatened(tile, true); // Shows threating pieces
            if (inThreat == true) {
                DecreaseShieldCharge();
                return false;
            }
        }

        return true;
    }

    private bool IsTileSafe(BoardTile tile) {
        bool inThreat = EnemyManager.Instance.IsTileInThreatened(tile, true);
        return !inThreat; // If in thread == True then it is NOT Safe
    }

    public BoardTile GetPlayersTile() {
        return playerPiece.GetTile();
    }

    private void RestoreShieldCharges() {
        // TO DO: There should be an UI to track this
        shieldChargesRemaining = shieldChargesLimit;
    }

    private void DecreaseShieldCharge() {
        // TO DO: There should be an UI to track this
        shieldChargesRemaining--;
    }

    private void EndTurn() {
        turnManager.EndPlayerTurn();
    }

    private void OnGUI() { // To Do: DEBUG Delete Later
        GUI.Label(new Rect(10, 70, 300, 20), $"Shield: {shieldChargesRemaining}/{shieldChargesLimit}");
    }
}
