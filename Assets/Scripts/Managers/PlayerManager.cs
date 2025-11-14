using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

    public static PlayerManager Instance { get; private set; }

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject weaponPrefab;
    [SerializeField] private ArrowIndicator arrowIndicator;
    [SerializeField] private BoardInputBroadcaster boardInputBroadcaster;

    private TurnManager turnManager;
    private PlayerPiece playerPiece;
    private PerkManager perkManager;
    private List<BoardTile> playerAvailableMoves;
    private Weapon weapon;

    private int shieldChargesLimit = 2;  // Number of times the player is protected from moving onto a threatened tile.
    private int shieldChargesRemaining;

    // Soul Mod Settings
    private int soulSlotNumber = 1;
    private List<EnemyTypeSO> soulTypeSOList;
    [SerializeField] private bool soulModeEnabled;
    [SerializeField] private EnemyTypeSO selectedSoul;
    [SerializeField] private bool endTurnAfterSoul_perkEffect;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Initialize() {
        turnManager = TurnManager.Instance;
        perkManager = PerkManager.Instance;

        boardInputBroadcaster.OnTileHovered += HandleTileHover;
        boardInputBroadcaster.OnTileClicked += HandleTileClick;

        turnManager.OnPlayerTurnStarted += StartPlayerTurn;

        InitializeSouls();
    }

    private void InitializeSouls() {
        soulModeEnabled = false;
        selectedSoul = null;
        endTurnAfterSoul_perkEffect = true;
        soulTypeSOList = new List<EnemyTypeSO>();
        while (soulTypeSOList.Count < soulSlotNumber) {
            soulTypeSOList.Add(null);
        }
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
                    if(endTurnAfterSoul_perkEffect == false) {
                        MakeSoulMovementTo(tile);
                    }else if (IsActionApprovedByShieldProtection(tile)) {
                        MakeSoulMovementTo(tile);
                        weapon.Reload();
                    }
                }
            }
        }
    }

    private void MakeKingMovementTo(BoardTile tile) {
        arrowIndicator.Hide();
        playerPiece.MoveToPosition(tile);

        StartCoroutine(TurnManager.Instance.StartActionPhase(true));        // When all the registered coroutines end, End Turn
    }

    private void MakeSoulMovementTo(BoardTile tile) {
        arrowIndicator.Hide();
        playerPiece.MoveToPosition(tile);

        SpendSoul();
        ExitSoulMode();
        StartCoroutine(TurnManager.Instance.StartActionPhase(endTurnAfterSoul_perkEffect));
    }

    private Coroutine SpawnPlayer() {
        GameObject playerPieceObj = Instantiate(playerPrefab, transform);
        playerPiece = playerPieceObj.GetComponent<PlayerPiece>();

        playerPiece.SetPosition(BoardManager.Board[4, 0]);
        Coroutine playerSpawnAnimation = playerPiece.SpawnAnimation_Descend(0f);

        GameObject weaponObj = Instantiate(weaponPrefab, playerPieceObj.transform);
        weapon = weaponObj.GetComponent<Weapon>();
        weapon.InitializeWeapon(perkManager.GetWeaponModifierData());

        return playerSpawnAnimation;
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
        if (selectedSoul == null)
            return;
        // TO DO: Change SPRITE Animation

        this.selectedSoul = selectedSoul;

        soulModeEnabled = true;
        playerAvailableMoves = ChessPiece.GetTilesFromPatternList(BoardManager.Board, playerPiece.GetTile().GridPosition, selectedSoul.movementPatterns, false);

        foreach(BoardTile tile in playerAvailableMoves) {
            tile.Highlight(true);
        }
    }

    public void ExitSoulMode() {
        if (selectedSoul == null)
            return;
        // TO DO: Change SPRITE Animation

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

    private void ApplySoulModification(SoulModifierData soulModifierData) {
        if (soulModifierData == null) return;
        if (soulModifierData.moveAfterSoulUsageEnable) endTurnAfterSoul_perkEffect = false;

        int newSoulSlotNumber = SoulModifierData.soulSlotDefault + soulModifierData.soulSlotChange;
        
        soulSlotNumber = Math.Max(0, newSoulSlotNumber);
        if (soulTypeSOList.Count > soulSlotNumber) { // If list has more than new size, trim
            soulTypeSOList.RemoveRange(soulSlotNumber, soulTypeSOList.Count - soulSlotNumber);
        }
        while (soulTypeSOList.Count < soulSlotNumber)
            soulTypeSOList.Add(null);
    }

    private void EndTurn() {
        turnManager.EndPlayerTurn();
    }

    public static void ResetStaticVariablesOnDefeat() {
        // TO DO: Do I have any static left?
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

    public IEnumerator NewFloorPreparation() {
        ApplySoulModification(perkManager.GetSoulModifierData());
        yield return SpawnPlayer();

    }

    public void CapturedByEnemyAnimation() {
        playerPiece.visualEffects.SpriteFadeOutAnimation(ChessPiece.captureMovementDuration);
    }

    public IEnumerator onPlayerWin_PlayerManager() {
        yield return StartCoroutine(DestroyPlayer());
    }

    private IEnumerator DestroyPlayer() {
        yield return playerPiece.visualEffects.DestroyAnimation_Ascend(0.5f, 1.5f);
        playerPiece = null;
        weapon = null;
    }

}
