using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPiece : ChessPiece {
    [SerializeField] protected EnemyTypeSO enemyTypeSO;
    [SerializeField] protected int currentHealth;
    [SerializeField] protected int cooldownToMove;
    [SerializeField] protected bool readyToMove = false;


    private Coroutine damageCoroutine;
    private int pendingDamage;

    public bool IsDead { get; private set; }
    //TO DO: Refactor VisualEffects to be an observer.
    public System.Action<EnemyPiece> OnDeath;
    //public System.Action<EnemyPiece,int> OnTakeDamage;


    protected override void Awake() {
        base.Awake();
        if (enemyTypeSO != null) {
            currentHealth = enemyTypeSO.maxHealth;
            cooldownToMove = UnityEngine.Random.Range(2, enemyTypeSO.speed + 1);
        }

        IsDead = false;
    }

    public EnemyPiece CheckControl(bool showThreat=false) {
        UpdateThreatenedTiles(BoardManager.Board);

        if (IsTileIsInThreatened(PlayerManager.Instance.GetPlayersTile(), showThreat))
            return this;
        return null;
    }

    public bool IsTileIsInThreatened(BoardTile tile, bool showThreat=false) {
        bool inThreat = false;

        if (threatenedTiles.Contains(tile)) {
            inThreat = true;
            if (showThreat) {
                visualEffects.Flicker(Color.red, 0.1f, 3);
            }
        }

        return inThreat;
    }

    public virtual void TakeAction() {
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
                }
            } else { // No Available Movement
                if (readyToMove == true) {
                    StopReadyingToMove();
                }
            }
        }

        StartCoroutine(AfterActionUpdateThreatenedTilesRoutine()); // All pieces should have finished by the time this coroutine starts
    }

    private IEnumerator AfterActionUpdateThreatenedTilesRoutine(float waitDuration = 0.3f) {
        yield return new WaitForSeconds(waitDuration);
        UpdateThreatenedTiles(BoardManager.Board);
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

    public void CheckMateTheKing() {
        PlayerManager.Instance.CheckMatedAnimation();
        StartCoroutine(CheckmateMoveToPosition());
    }

    private IEnumerator CheckmateMoveToPosition() {
        Vector3 targetPos = PlayerManager.Instance.GetPlayersTile().transform.position;
        Vector3 startPos = transform.position;
        float liftHeight = 0.5f;
        float time = 0f;

        while (time < checkMateMovementDuration) {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / checkMateMovementDuration);

            // Interpolate horizontal position
            Vector3 horizontalPos = Vector3.Lerp(startPos, targetPos, t);

            // Create lift curve: rises early, falls near the end
            // Shape: ease-out up, ease-in down
            float lift;
            if (t < 0.3f)
                lift = Mathf.SmoothStep(0f, liftHeight, t / 0.3f);  // rise quickly
            else if (t > 0.7f)
                lift = Mathf.SmoothStep(liftHeight, 0f, (t - 0.7f) / 0.3f);  // lower smoothly
            else
                lift = liftHeight;  // stay up briefly in the middle

            // Apply vertical lift
            transform.position = horizontalPos + Vector3.up * lift;

            yield return null;
        }

        transform.position = targetPos;

        EnemyManager.onEnemyCheckMatesThePlayer?.Invoke();
    }

    public void TakeDamage(int amount) {
        pendingDamage += amount;
        if (damageCoroutine == null) {
            damageCoroutine = StartCoroutine(TakePendingDamage(0.1f));
            visualEffects.Flicker(Color.red, 0.1f, 2, true);
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
    protected void Die() {
        if (IsDead) return;
        IsDead = true;

        BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();
        boxCollider2D.enabled = false;

        currentTile.SetPiece(null);
        visualEffects.SpriteFadeOutAnimation(1f);
        StartCoroutine(DestroyedIn(1f));
        OnDeath?.Invoke(this);
    }

    protected IEnumerator DestroyedIn(float time) {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

    public EnemyTypeSO GetEnemyTypeSO() {
        return enemyTypeSO;
    }

}