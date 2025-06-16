using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager Instance { get; private set; }
    private EnemyManager enemyManager;
    private PlayerManager playerManager;
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
        enemyManager = EnemyManager.Instance;
        playerManager = PlayerManager.Instance;
        roundNumber = 0;
    }

    public void StartGame() {
        playerManager.SpawnPlayer();
        enemyManager.SpawnEnemies();
        playerManager.PlayerTurnStart();
    }

}
