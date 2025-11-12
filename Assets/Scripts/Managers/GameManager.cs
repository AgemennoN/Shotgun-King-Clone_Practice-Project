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
    [SerializeField] private GameObject perkSelectPanel;


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
        EnemyManager.onEnemyKingsDie += HandlePlayerWins;
    }
    public void StartGame() {
        playerManager.SpawnPlayer();
        enemyManager.SpawnEnemyDictOfTheMap();
        
        turnManager.GameStart();
    }

    private void HandleEnemyWins() {
        gameOverPanel.SetActive(true);
    }
    private void HandlePlayerWins() {
        enemyManager.onPlayerWin_EnemyManager();
        boardManager.onPlayerWin_BoardManager();
        playerManager.onPlayerWin_PlayerManager();

        // PerkManager might activate PerkSelectionPanel
        perkSelectPanel.SetActive(true);
    }

    public void OnPlayAgainButton() {
        PlayerManager.ResetStaticVariablesOnDefeat(); //TO DO: use Event Maybe
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnDisable() {
        EnemyManager.onEnemyCheckMatesThePlayer -= HandleEnemyWins;
        EnemyManager.onEnemyKingsDie -= HandlePlayerWins;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

}
