using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Multiplayer.Center.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

public class EnemyPiece : ChessPiece {
    [SerializeField] protected EnemyTypeSO enemyTypeSO;
    [SerializeField] protected int currentHealth;
    [SerializeField] protected int cooldownToMove;
    [SerializeField] protected bool readyToMove = false;

    [SerializeField] private VisualEffects visualEffects;

    private Coroutine damageCoroutine;
    private int pendingDamage;

    public bool IsDead { get; private set; }
    //TO DO: Refactor VisualEffects to be an observer.
    public System.Action<EnemyPiece> OnDeath;
    //public System.Action<EnemyPiece,int> OnTakeDamage;


    private void Awake() {
        if (enemyTypeSO != null) {
            currentHealth = enemyTypeSO.maxHealth;
            cooldownToMove = UnityEngine.Random.Range(2, enemyTypeSO.speed + 1);
        }
        
        if (visualEffects == null) {
            visualEffects = GetComponent<VisualEffects>();
        }
        IsDead = false;
    }

    public void CheckControl() {
        UpdateThreatenedTiles(BoardManager.Board);

        IsTileIsInThreatened(PlayerManager.Instance.GetPlayersTile());
    }

    public bool IsTileIsInThreatened(BoardTile tile, bool showThreat = false) {
        bool inThreat = false;

        if (threatenedTiles.Contains(tile)) {
            inThreat = true;
            if (showThreat) {
                visualEffects.Flicker(Color.red, 0.1f, 3);
            }
        }

        return inThreat;
    }

    public void TakeAction() {
        if (cooldownToMove > 1) {
            ReduceCooldown();
        } 
        if (cooldownToMove == 1) {
            UpdateAvailableTiles(BoardManager.Board);
            if (availableTiles.Count != 0) {
                if (cooldownToMove == 1 && readyToMove == false) {
                    GetReadyToMove();
                } else { // Stop Shaking and MOVE
                    StopReadyingToMove();
                    MoveToPosition(DecideMovementTile(availableTiles));
                    cooldownToMove = enemyTypeSO.speed; // Reset Cooldown
                    Debug.Log($"GameObject {gameObject.name} Taking Action maxHealth: {enemyTypeSO.maxHealth}, speed: {enemyTypeSO.speed}");
                    
                }
            } else { // No Available Movement
                if (readyToMove == true) {
                    StopReadyingToMove();
                }
            }
        }
    }

    private void ReduceCooldown() {
        cooldownToMove -= 1;
    }

    private void GetReadyToMove() { // starts shaking
        readyToMove = true;
        visualEffects.StartShake();
    }

    private void StopReadyingToMove() { // Stop shaking
        readyToMove = false;
        visualEffects.StopShake();
    }

    public override void UpdateAvailableTiles(BoardTile[,] board) {
        Vector2Int currentPos = currentTile.GridPosition;
        availableTiles = GetTilesFromPatternList(board, currentPos, enemyTypeSO.movementPatterns, false);
    }

    public override void UpdateThreatenedTiles(BoardTile[,] board) {
        threatenedTiles = new List<BoardTile>();

        List<MovementPattern> patterns = enemyTypeSO.isThreatSameWithMovement
            ? enemyTypeSO.movementPatterns
            : enemyTypeSO.threatPatterns;

        Vector2Int currentPos = currentTile.GridPosition;
        threatenedTiles = GetTilesFromPatternList(board, currentPos, patterns, true);
    }

    private List<BoardTile> GetTilesFromPatternList(BoardTile[,] board, Vector2Int currentPos, List<MovementPattern> patterns, bool canCapture) {
        List<BoardTile> tiles = new List<BoardTile>();

        foreach (MovementPattern pattern in patterns) {
            switch (pattern.movementType) {
                case MovementType.Jump:
                    HandleJumpMove(board, currentPos, pattern, tiles, canCapture);
                    break;

                case MovementType.FiniteStep:
                    HandleFiniteStepMove(board, currentPos, pattern, tiles, canCapture);
                    break;

                case MovementType.InfiniteStep:
                    HandleInfiniteStepMove(board, currentPos, pattern, tiles, canCapture);
                    break;
            }
        }
        return tiles;
    }

    private void HandleJumpMove(BoardTile[,] board, Vector2Int currentPos, MovementPattern pattern, List<BoardTile> availableTiles, bool canCapture) {
        Vector2Int targetPos = currentPos + pattern.direction;

        if (!BoardManager.IsInsideBounds(targetPos))
            return;

        BoardTile targetTile = board[targetPos.x, targetPos.y];
        ChessPiece pieceOnTheTile = targetTile.GetPiece();
        if (pieceOnTheTile == null || (pieceOnTheTile is PlayerPiece && canCapture))
            availableTiles.Add(targetTile);
    }

    private void HandleFiniteStepMove(BoardTile[,] board, Vector2Int currentPos, MovementPattern pattern, List<BoardTile> availableTiles, bool canCapture) {
        for (int step = 1; step <= pattern.maxDistance; step++) {
            Vector2Int targetPos = currentPos + pattern.direction * step;

            if (!BoardManager.IsInsideBounds(targetPos))
                break;

            BoardTile targetTile = board[targetPos.x, targetPos.y];
            ChessPiece pieceOnTheTile = targetTile.GetPiece();

            if (pieceOnTheTile == null) {
                availableTiles.Add(targetTile);
            } else {
                if (pieceOnTheTile is PlayerPiece && canCapture) {
                    // PlayerPiece can be captured so the tile is available
                    availableTiles.Add(targetTile);
                    continue;
                }
                // Stop if blocked by another piece
                break;
            }
        }
    }

    private void HandleInfiniteStepMove(BoardTile[,] board, Vector2Int currentPos, MovementPattern pattern, List<BoardTile> availableTiles, bool canCapture) {
        for (int step = 1; ; step++) {
            Vector2Int targetPos = currentPos + pattern.direction * step;

            if (!BoardManager.IsInsideBounds(targetPos))
                break;

            BoardTile targetTile = board[targetPos.x, targetPos.y];
            ChessPiece pieceOnTheTile = targetTile.GetPiece();

            if (pieceOnTheTile == null) {
                availableTiles.Add(targetTile);
            } else {
                if (pieceOnTheTile is PlayerPiece && canCapture) {
                    // PlayerPiece can be captured so the tile is available
                    availableTiles.Add(targetTile);
                    continue;
                }
                // Stop if blocked by another piece
                break;
            }
        }
    }

    protected virtual BoardTile DecideMovementTile(List<BoardTile> availableTiles) {
        if (availableTiles.Count > 1) {
            List<BoardTile> checkingTiles = new List<BoardTile>();
            BoardTile playerTile = PlayerManager.Instance.GetPlayersTile();
            
            foreach (BoardTile tile in availableTiles) {
                if (WouldThreatenTargetTileFrom(BoardManager.Board, tile, playerTile)){
                    checkingTiles.Add(tile);
                }
            }

            BoardTile targetTileToMove = currentTile;
            if (checkingTiles.Count > 0) {
                targetTileToMove = BoardManager.GetClosestTileToTargetTile(checkingTiles, playerTile);
            } else {
                targetTileToMove = BoardManager.GetClosestTileToTargetTile(availableTiles, playerTile);
            }
            return targetTileToMove;

        } else if (availableTiles.Count == 1) {
            return availableTiles[0];
        } else {
            // TO DO: try expect ile yap burayi düzgün bir log bastir. zaten olduðu kareyi dönsün
            Debug.Log("AvailableTiles are empty");
            return null;
        }
    }

    protected virtual bool WouldThreatenTargetTileFrom(BoardTile[,] board, BoardTile fromTile, BoardTile targetTile) {
        // Temporarily imagine the enemy moved to that tile:
        List<BoardTile> threatenedTiles = new List<BoardTile>();

        List<MovementPattern> patterns = enemyTypeSO.isThreatSameWithMovement
            ? enemyTypeSO.movementPatterns
            : enemyTypeSO.threatPatterns;

        threatenedTiles = GetTilesFromPatternList(board, fromTile.GridPosition, patterns, true);

        if (threatenedTiles.Contains(targetTile))
            return true;
        return false;
    }

    public void TakeDamage(int amount) {
        pendingDamage += amount;
        if (damageCoroutine == null) {
            damageCoroutine = StartCoroutine(TakePendingDamage(0.1f));
            TurnManager.Instance.RegisterAction(TimeToHandleDamage(0.5f));
            visualEffects.Flicker(Color.red, 0.1f, 2);
        }

        if (currentHealth - pendingDamage <= 0) {
            Debug.Log("currentHealth: " + currentHealth + ":pendingDamage :" + pendingDamage);
            Die();
        }
    }

    private IEnumerator TakePendingDamage(float time) {
        yield return new WaitForSeconds(time);
        visualEffects.ShowDamage(pendingDamage);
        currentHealth -= pendingDamage;
        pendingDamage = 0;
        damageCoroutine = null;
    }

    private IEnumerator TimeToHandleDamage(float time) {
        yield return new WaitForSeconds(time);
    }
    
    private void Die() {
        if (IsDead) return;
        IsDead = true;
        // play death animation
        BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();
        boxCollider2D.enabled = false;

        currentTile.SetPiece(null);
        OnDeath?.Invoke(this);
    }

    public EnemyTypeSO GetEnemyTypeSO() {
        return enemyTypeSO;
    }

}