using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour {
    public static TurnManager Instance { get; private set; }

    public event Action OnPlayerTurnStarted;
    public event Action OnEnemyTurnStarted;
    private enum TurnState { None, PlayerTurn, EnemyTurn, ActionPhase}
    private TurnState currentTurn = TurnState.None;
    private int roundNumber;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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


    // Actions the ActionPhase should wait for
    private readonly List<IEnumerator> runningActions = new();

    public void RegisterAction(IEnumerator actionCoroutine) {
        StartCoroutine(ActionWrapper(actionCoroutine));
    }

    private IEnumerator ActionWrapper(IEnumerator coroutine) {
        runningActions.Add(coroutine);
        yield return coroutine;
        runningActions.Remove(coroutine);
    }

    private IEnumerator WaitForActionsToComplete() {
        yield return new WaitUntil(() => runningActions.Count == 0);
    }

    public IEnumerator StartActionPhase(bool EndTurnAfterActionPhase) {
        TurnState oldState = currentTurn;
        currentTurn = TurnState.ActionPhase;

        yield return StartCoroutine(WaitForActionsToComplete());

        if (EndTurnAfterActionPhase is true) {
            if (oldState == TurnState.PlayerTurn) {
                EndPlayerTurn();
            } else {
                EndEnemyTurn();
            }
        } else {
            currentTurn = oldState;
        }
    }

    public void EndPlayerTurn() {
        StartEnemyTurn();
    }

    public void EndEnemyTurn() {
        StartPlayerTurn();
    }

    public bool IsPlayerTurn() => currentTurn == TurnState.PlayerTurn;
    public bool IsEnemyTurn() => currentTurn == TurnState.EnemyTurn;
}
