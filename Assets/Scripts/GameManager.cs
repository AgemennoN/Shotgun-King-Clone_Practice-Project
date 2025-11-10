using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public static GameManager Instance { get; private set; }
    private EnemyManager enemyManager;
    private PlayerManager playerManager;
    private BoardManager boardManager;
    private TurnManager turnManager;

    [SerializeField] private GameObject gameOverPanel;


    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start() {
        Initialize();

        StartGame();
    }

    public void Initialize() {
        turnManager = TurnManager.Instance;
        turnManager.Initialize();
        boardManager = BoardManager.Instance;
        boardManager.Initialize();
        enemyManager = EnemyManager.Instance;
        enemyManager.Initialize();
        playerManager = PlayerManager.Instance;
        playerManager.Initialize();

        EnemyManager.onEnemyCheckMatesThePlayer += HandleEnemyWins;
    }
    public void StartGame() {
        playerManager.SpawnPlayer();
        enemyManager.SpawnEnemyDictOfTheMap();
        
        turnManager.GameStart();
    }

    private void HandleEnemyWins() {
        gameOverPanel.SetActive(true);
    }

    public void OnPlayAgainButton() {

        gameOverPanel.SetActive(false);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnDisable() {
        EnemyManager.onEnemyCheckMatesThePlayer -= HandleEnemyWins;
    }

}
