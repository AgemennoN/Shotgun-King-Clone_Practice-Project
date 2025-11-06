using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

public class EnemyPiece : ChessPiece {
    [SerializeField] protected EnemyTypeSO enemyTypeSO;
    [SerializeField] protected int currenHealth;
    [SerializeField] protected int cooldownToMove;
    [SerializeField] protected bool readyToMove = false;

    public System.Action<EnemyPiece> OnDeath;

    private void Awake() {
        if (enemyTypeSO != null) {
            currenHealth = enemyTypeSO.maxHealth;
            cooldownToMove = UnityEngine.Random.Range(2, enemyTypeSO.speed + 1);

        }
    }

    public void CheckControl() {
        UpdateThreatenedTiles(BoardManager.Board);

        IsTileIsInThreatened(PlayerManager.Instance.GetPlayersTile());
    }

    public bool IsTileIsInThreatened(BoardTile tile) {
        return threatenedTiles.Contains(tile);
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
        // Start shaking animation
    }

    private void StopReadyingToMove() { // Stop shaking
        readyToMove = false;
        // Stop shaking animation
    }
    
    public override void UpdateAvailableTiles(BoardTile[,] board) {
        availableTiles = GetTilesFromPatternList(board, enemyTypeSO.movementPatterns, false);
    }

    public override void UpdateThreatenedTiles(BoardTile[,] board) {
        threatenedTiles = new List<BoardTile>();

        var patterns = enemyTypeSO.isThreatSameWithMovement
            ? enemyTypeSO.movementPatterns
            : enemyTypeSO.threatPatterns;

        threatenedTiles = GetTilesFromPatternList(board, patterns, true);
    }

    private List<BoardTile> GetTilesFromPatternList(BoardTile[,] board, List<MovementPattern> patterns, bool canCapture) {
        List<BoardTile> tiles = new List<BoardTile>();

        Vector2Int currentPos = currentTile.GridPosition;

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
        if (availableTiles.Count > 0) {
            int randomIndex = UnityEngine.Random.Range(0, availableTiles.Count);
            BoardTile randomTile = availableTiles[randomIndex];
            return randomTile;
        } else {
            return null;
        }
    }

    public void TakeDamage(int amount) {
        currenHealth -= amount;
        if (currenHealth <= 0) {
            Die();
        }
    }
    
    private void Die() {
        // play death animation
        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }

    public EnemyTypeSO GetEnemyTypeSO() {
        return enemyTypeSO;
    }

}