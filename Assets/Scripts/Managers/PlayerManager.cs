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

    // Soul Mod Settings
    private static int soulSlot = 2;
    private static List<EnemyTypeSO> soulTypeSOList = new List<EnemyTypeSO>();
    [SerializeField] private bool soulModeEnabled = false;
    [SerializeField] private EnemyTypeSO selectedSoul = null;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Initialize() {
        turnManager = TurnManager.Instance;

        boardInputBroadcaster.OnTileHovered += HandleTileHover;
        boardInputBroadcaster.OnTileClicked += HandleTileClick;

        turnManager.OnPlayerTurnStarted += StartPlayerTurn;

        InitializeSouls();
        Debug.Log("soulTypeSOList: " + soulTypeSOList);
    }

    private void InitializeSouls() {
        Debug.Log("soulTypeSOList.Count: " + soulTypeSOList.Count);
        while (soulTypeSOList.Count < soulSlot) {
            Debug.Log("ADD NULL");
            soulTypeSOList.Add(null);
        }
        Debug.Log("soulTypeSOList.Count: " + soulTypeSOList.Count);

    }

    private void HandleTileHover(BoardTile tile) {
        if (!TurnManager.Instance.IsPlayerTurn())
            return;

        if (tile != null)
            if (soulModeEnabled == false) {
                if (playerAvailableMoves.Contains(tile)) {
                    Vector3 from = playerPiece.GetTile().transform.position;
                    Vector3 to = tile.transform.position;
                    arrowIndicator.Show(from, to);
                    weapon.Aim(false);
                } else {
                    arrowIndicator.Hide();
                    weapon.Aim(true);
                }
            } else {
                if (playerAvailableMoves.Contains(tile)) {
                    Vector3 from = playerPiece.GetTile().transform.position;
                    Vector3 to = tile.transform.position;
                    arrowIndicator.Show(from, to);
                    weapon.Aim(false);
                }
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
            if (soulModeEnabled == false) {
                if (playerAvailableMoves.Contains(tile)) {
                    if (IsActionApprovedByShieldProtection(tile)) {
                        arrowIndicator.Hide();
                        MakeKingMovementTo(tile);
                        weapon.Reload(); // Reload logic done inside weapon
                    }
                } else if (tile != playerPiece.GetTile()) {
                    if (IsActionApprovedByShieldProtection(GetPlayersTile())) {
                        if (weapon.Shoot(mouseWorldPos)) {
                            StartCoroutine(TurnManager.Instance.StartActionPhase(true));    // When all the registered coroutines end, End Turn
                        }
                    }
                }
            } else { // SOUL MOVEMENT
                if (playerAvailableMoves.Contains(tile)) {
                    arrowIndicator.Hide();
                    MakeSoulMovementTo(tile);
                    weapon.Reload(); // Reload logic done inside weapon

                }

            }
        }

    }

    private void MakeKingMovementTo(BoardTile tile) {
        playerPiece.MoveToPosition(tile);
        StartCoroutine(TurnManager.Instance.StartActionPhase(true));        // When all the registered coroutines end, End Turn
    }
    private void MakeSoulMovementTo(BoardTile tile) {
        playerPiece.MoveToPosition(tile);

        bool perk_EndTurnWithSoul = false;
        StartCoroutine(TurnManager.Instance.StartActionPhase(perk_EndTurnWithSoul));        // When all the registered coroutines end, End Turn

        SpendSoul();
        ExitSoulMode();
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

    public void CheckMatedAnimation() {
        playerPiece.visualEffects.SpriteFadeOutAnimation(ChessPiece.checkMateMovementDuration);
    }

    public bool CanHarvestSoul() {
        for (int i = 0; i < soulTypeSOList.Count; i++) {
            if (soulTypeSOList[i] == null) {
                return true;
            }
        }
        return false; // No Empty Soul Slot
    }

    public void PrintSoulList() { // TO DO: DELETE
        for (int i = 0; i < soulTypeSOList.Count; i++) {
            if (soulTypeSOList[i] != null) {
                Debug.Log($"Soul{i}: {soulTypeSOList[i]}");
            } else Debug.Log($"Soul{i}: NULL");
        }
    }

    public void HarvestSoul(EnemyTypeSO newSoul) {
        for (int i = 0; i < soulTypeSOList.Count; i++) {
            if (soulTypeSOList[i] == null) {
                soulTypeSOList[i] = newSoul;
                return;
            }
        }
    }

    public void EnterSoulMode(EnemyTypeSO selectedSoul) {
        ExitSoulMode();
        // TO DO: Change SPRITE Animation
        if (selectedSoul == null) return;
        this.selectedSoul = selectedSoul;

        soulModeEnabled = true;
        playerAvailableMoves = ChessPiece.GetTilesFromPatternList(BoardManager.Board, playerPiece.GetTile().GridPosition, selectedSoul.movementPatterns, false);

        foreach(BoardTile tile in playerAvailableMoves) {
            tile.Highlight(true);
        }
    }

    public void ExitSoulMode() {
        // TO DO: Change SPRITE Animation
        if (selectedSoul == null)
            return;

        Debug.Log("ExitSoulMode");

        selectedSoul = null;
        soulModeEnabled = false;
        foreach (BoardTile tile in playerAvailableMoves) {
            tile.Highlight(false);
        }
        playerPiece.UpdateAvailableTiles(BoardManager.Board);
        playerAvailableMoves = playerPiece.GetAvailableTiles();
    }

    private void SpendSoul() {
        if (selectedSoul == null)
            return;

        int index = soulTypeSOList.IndexOf(selectedSoul);
        for (int i = index; i < soulTypeSOList.Count - 1; i++) {
            soulTypeSOList[i] = soulTypeSOList[i + 1];
        }
        soulTypeSOList[soulTypeSOList.Count - 1] = null;
    }

    private void EndTurn() {
        turnManager.EndPlayerTurn();
    }

    public static void ResetStaticVariablesOnDefeat() {
        soulSlot = 2;
        soulTypeSOList = new List<EnemyTypeSO>();
    }

    private void OnGUI() { // To Do: DEBUG Delete Later
        GUI.Label(new Rect(10, 70, 300, 20), $"Shield: {shieldChargesRemaining}/{shieldChargesLimit}");
        GUI.Label(new Rect(10, 100, 300, 20), $"Souls: {soulTypeSOList}");
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.S)) {
            PrintSoulList();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            EnterSoulMode(soulTypeSOList[0]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            EnterSoulMode(soulTypeSOList[1]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            ExitSoulMode();
        }

    }
}
