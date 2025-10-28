using System;
using UnityEngine;

public class TurnManager : MonoBehaviour {
    public static TurnManager Instance { get; private set; }

    public event Action OnPlayerTurnStarted;
    public event Action OnEnemyTurnStarted;

    private enum TurnState { None, PlayerTurn, EnemyTurn }
    private TurnState currentTurn = TurnState.None;
    private int roundNumber;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize() {
        roundNumber = 0;
    }

    public void GameStart() {
        Debug.Log("Game started, beginning enemy turn...");
        StartEnemyTurn();
    }

    private void StartPlayerTurn() {
        roundNumber++;
        currentTurn = TurnState.PlayerTurn;
        Debug.Log($"--- Round {roundNumber}: Player Turn ---");
        OnPlayerTurnStarted?.Invoke();
    }

    private void StartEnemyTurn() {
        currentTurn = TurnState.EnemyTurn;
        Debug.Log($"--- Round {roundNumber}: Enemy Turn ---");
        OnEnemyTurnStarted?.Invoke();
    }

    public void EndTurn() {
        if (currentTurn == TurnState.PlayerTurn)
            StartEnemyTurn();
        else
            StartPlayerTurn();
    }

    public bool IsPlayerTurn() => currentTurn == TurnState.PlayerTurn;
    public bool IsEnemyTurn() => currentTurn == TurnState.EnemyTurn;
}
