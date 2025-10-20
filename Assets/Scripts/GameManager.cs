using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager Instance { get; private set; }
    private EnemyManager enemyManager;
    private PlayerManager playerManager;
    private BoardManager boardManager;
    private int roundNumber;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        Initialize();

        StartGame();
    }

    public void Initialize() {
        boardManager = BoardManager.Instance;
        boardManager.Initialize();
        enemyManager = EnemyManager.Instance;
        enemyManager.Initialize();
        playerManager = PlayerManager.Instance;
        playerManager.Initialize();

        roundNumber = 0;
    }

    public void StartGame() {
        playerManager.SpawnPlayer();
        enemyManager.SpawnEnemies();
        playerManager.PlayerTurnStart();
    }

}
