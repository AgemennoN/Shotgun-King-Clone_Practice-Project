using UnityEngine;

public class TurnManager : MonoBehaviour {
    public static TurnManager Instance { get; private set; }

    private enum TurnState {
        PlayerTurn,
        EnemyTurn
    }

    private TurnState currentTurn;
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

    public void StartPlayerTurn() {
        roundNumber++;
        Debug.Log("roundNumber: " + roundNumber);
        currentTurn = TurnState.PlayerTurn;
        PlayerManager.Instance.PlayerTurnStart();
    }

    public void EndPlayerTurn() {
        currentTurn = TurnState.EnemyTurn;
        EnemyManager.Instance.StartEnemyTurn();
    }

    public void EndEnemyTurn() {
        StartPlayerTurn();
    }

    public bool IsPlayerTurn() {
        return currentTurn == TurnState.PlayerTurn;
    }

    public bool IsEnemyTurn() {
        return currentTurn == TurnState.EnemyTurn;
    }
}