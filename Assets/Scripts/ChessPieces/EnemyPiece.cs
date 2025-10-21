using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

public class EnemyPiece : ChessPiece {
    [SerializeField] protected int maxHealth;
    [SerializeField] protected int currenHealth;
    [SerializeField] protected int speed; // Max cooldownToMove
    [SerializeField] protected int cooldownToMove;
    [SerializeField] protected bool readyToMove;

    protected List<BoardTile> availableTiles;
    protected BoardTile[,] board;

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
                } else {
                    StopReadyingToMove();
                    MoveToPosition(DecideMovementTile(availableTiles));
                    cooldownToMove = speed;
                    Debug.Log($"GameObject {gameObject.name} Taking Action");
                    //MOVE and Stop Shaking
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