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
    private Weapon weapon = null;

    private int shieldChargesLimit = 2;  // Number of times the player is protected from moving onto a threatened tile.
    private int shieldChargesRemaining;
    public Action<int> onShieldProtection;

    // Soul Mod Settings
    [SerializeField] private bool soulModeEnabled;
    public Action OnSoulMovement;

    [SerializeField] private bool specialPerk_turnContinueAfterSoul;

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

        soulModeEnabled = false;
        SubscribeForSpecialPerks();
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
                        weapon.Reload();
                        MakePlayerMovementTo(tile);
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
                    if(specialPerk_turnContinueAfterSoul == true) {
                        MakePlayerMovementTo(tile);
                    }else if (IsActionApprovedByShieldProtection(tile)) {
                        weapon.Reload();
                        MakePlayerMovementTo(tile);
                    }
                }
            }
        }
    }

    private void MakePlayerMovementTo(BoardTile tile) {
        arrowIndicator.Hide();
        playerPiece.MoveToPosition(tile);

        if (soulModeEnabled) {
            OnSoulMovement?.Invoke();
            StartCoroutine(TurnManager.Instance.StartActionPhase(!specialPerk_turnContinueAfterSoul));
        } else {
            StartCoroutine(TurnManager.Instance.StartActionPhase(true));        // When all the registered coroutines end, End Turn
        }

        
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

    public Weapon GetWeapon() {
        return weapon;
    }

    private void StartPlayerTurn() {
        RestoreShieldCharges();

        playerPiece.UpdateAvailableTiles(BoardManager.Board);
        playerAvailableMoves = playerPiece.GetAvailableTiles();

        boardInputBroadcaster.CheckTileHover(true);

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
        onShieldProtection?.Invoke(shieldChargesLimit);
    }

    private void DecreaseShieldCharge() {
        // TO DO: There should be an UI to track this
        shieldChargesRemaining--;
        onShieldProtection?.Invoke(shieldChargesRemaining);
    }

    public void EnterSoulMode(EnemyTypeSO selectedSoul) {
        if (selectedSoul == null)
            return;
        // TO DO: Change SPRITE Animation

        soulModeEnabled = true;
        playerAvailableMoves = ChessPiece.GetTilesFromPatternList(BoardManager.Board, playerPiece.GetTile().GridPosition, selectedSoul.movementPatterns, false);

        foreach(BoardTile tile in playerAvailableMoves) {
            tile.Highlight(true);
        }
    }

    public void ExitSoulMode() {
        // TO DO: Change SPRITE Animation

        soulModeEnabled = false;
        foreach (BoardTile tile in playerAvailableMoves) {
            tile.Highlight(false);
        }
        playerPiece.UpdateAvailableTiles(BoardManager.Board);
        playerAvailableMoves = playerPiece.GetAvailableTiles();
    }

    private void EndTurn() {
        turnManager.EndPlayerTurn();
    }

    public static void ResetStaticVariablesOnDefeat() {
        // TO DO: Do I have any static left?
    }

    public IEnumerator NewFloorPreparation() {
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

    #region SpecialPerks
    private void SubscribeForSpecialPerks() {
        specialPerk_turnContinueAfterSoul = false;
        perkManager.OnTurnContinueAfterSoul += SpecialPerk_TurnContinueAfterSoulEnabled;

    }

    private void SpecialPerk_TurnContinueAfterSoulEnabled() {
        specialPerk_turnContinueAfterSoul = true;
        perkManager.OnTurnContinueAfterSoul -= SpecialPerk_TurnContinueAfterSoulEnabled;
    }
    #endregion

}
