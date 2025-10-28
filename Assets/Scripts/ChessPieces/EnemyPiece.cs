using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

public class EnemyPiece : ChessPiece {
    [SerializeField] protected EnemyTypeSO enemyTypeSO;
    //protected EnemyType enemyType;   TO DO: Not using right now
    protected int maxHealth;
    protected int currenHealth;
    protected int speed; // Max cooldownToMove
    protected int cooldownToMove;
    protected bool readyToMove = false;

    protected List<BoardTile> availableTiles;
    protected BoardTile[,] board;


    private void Awake() {
        if (enemyTypeSO != null) {
            maxHealth = enemyTypeSO.maxHealth;
            currenHealth = maxHealth;
            speed = enemyTypeSO.speed;
            cooldownToMove = UnityEngine.Random.Range(2, speed + 1);

            //enemyType = enemyTypeSO.enemyType; TO DO Not using right now
        }
    }

    private void Start() {
        board = BoardManager.Instance.GetBoard();
    }

    // Virtual methods to be overridden in derived piece classes
    public void TakeAction() {
        if (cooldownToMove > 1) {
            ReduceCooldown();
        } 
        if (cooldownToMove == 1) {
            availableTiles = GetAvailableMoves(board);
            if (availableTiles.Count != 0) {
                if (cooldownToMove == 1 && readyToMove == false) {
                    GetReadyToMove();
                } else { // Stop Shaking and MOVE
                    StopReadyingToMove();
                    MoveToPosition(DecideMovementTile(availableTiles));
                    cooldownToMove = speed; // Reset Cooldown
                    Debug.Log($"GameObject {gameObject.name} Taking Action maxHealth: {maxHealth}, speed: {speed}");
                    
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

    private BoardTile DecideMovementTile(List<BoardTile> availableTiles) {
        if (availableTiles.Count > 0) {
            int randomIndex = UnityEngine.Random.Range(0, availableTiles.Count);
            BoardTile randomTile = availableTiles[randomIndex];
            return randomTile;
        } else {
            return null;
        }
    }

}