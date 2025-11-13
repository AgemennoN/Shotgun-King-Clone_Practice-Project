using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public static GameManager Instance { get; private set; }
    public static int Floor = 1;

    private PerkManager perkManager;
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

        StartCoroutine(StartFloor());
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

        perkManager = PerkManager.Instance;
        perkManager.Initialize();

        EnemyManager.onEnemyCheckMatesThePlayer += HandleEnemyWins;
        EnemyManager.onEnemyKingsDie += HandlePlayerWins;
    }

    public IEnumerator StartFloor() {
        yield return StartCoroutine(PrepareNewFloor());
        turnManager.GameStart();
    }

    public IEnumerator PrepareNewFloor() {
        yield return StartCoroutine(boardManager.NewFloorPreparation());        //Generates Board
        yield return StartCoroutine(enemyManager.NewFloorPreparation());        //Generates Enemies
        yield return StartCoroutine(playerManager.NewFloorPreparation());       //Generates PlayerPiece
    }

    private void HandleEnemyWins() {

        // GameManager.OnDefeat_ResetStaticsAllManagers();

        gameOverPanel.SetActive(true);
    }


    public void OnPlayAgainButton() {
        GameManager.OnDefeat_ResetStaticsAllManagers();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void HandlePlayerWins() {
        StartCoroutine(HandlePlayerWinsRoutine());
    }

    private IEnumerator HandlePlayerWinsRoutine() {

        yield return StartCoroutine(enemyManager.onPlayerWin_EnemyManager());
        yield return StartCoroutine(playerManager.onPlayerWin_PlayerManager());
        yield return StartCoroutine(boardManager.onPlayerWin_BoardManager());

        perkManager.onPlayerWin_PerkManager();
    }

    public void LoadNextFloor() { // Called from Perk select panel buttons
        Floor++;
        //When the Perk Manager ready Restart Scene
        StartCoroutine(StartFloor());

    }

    private void OnDisable() {
        EnemyManager.onEnemyCheckMatesThePlayer -= HandleEnemyWins;
        EnemyManager.onEnemyKingsDie -= HandlePlayerWins;
    }

    public static void ResetStaticVariablesOnDefeat() {
        // TO DO: Delete if not needed
    }

    public static void OnDefeat_ResetStaticsAllManagers() {       //TO DO: use Event Maybe
        // Resets statics because the scene will be reloaded
        GameManager.ResetStaticVariablesOnDefeat();
        EnemyManager.ResetStaticVariablesOnDefeat();
        PlayerManager.ResetStaticVariablesOnDefeat();
    }

}
