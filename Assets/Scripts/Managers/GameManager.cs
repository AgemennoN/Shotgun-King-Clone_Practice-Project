using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public static GameManager Instance { get; private set; }
    public static int Floor = 0;

    private PerkManager perkManager;
    private EnemyManager enemyManager;
    private PlayerManager playerManager;
    private PlayerSoulController playerSoulController;
    private BoardManager boardManager;
    private TurnManager turnManager;
    private InformationUI informationUI;

    [SerializeField] private FadingPanel gameOverPanel;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;

    }

    private void Start() {
        Initialize();

        LoadNextFloor();
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
        informationUI = InformationUI.Instance;
        informationUI.Initialize();
        playerSoulController = PlayerSoulController.Instance;
        playerSoulController.Initialize();

        perkManager = PerkManager.Instance;
        perkManager.Initialize();

        perkManager.OnPerkSelectionEnded += LoadNextFloor;

        EnemyManager.onEnemyCheckMatesThePlayer += HandleEnemyWins;
        EnemyManager.onEnemyKingsDie += HandlePlayerWins;
    }

    public void LoadNextFloor() { // Called from Perk select panel buttons
        StartCoroutine(StartFloor());
    }
    private IEnumerator StartFloor() {
        yield return StartCoroutine(PrepareNewFloor());
        turnManager.GameStart();
    }
    private IEnumerator PrepareNewFloor() {
        yield return StartCoroutine(boardManager.NewFloorPreparation());        //Generates Board
        yield return StartCoroutine(enemyManager.NewFloorPreparation());        //Generates Enemies
        yield return StartCoroutine(playerManager.NewFloorPreparation());       //Generates PlayerPiece
        StartCoroutine(playerSoulController.NewFloorPreparation(perkManager.GetSoulModifierData()));
        StartCoroutine(informationUI.NewFloorPreparation());
    }

    private void HandleEnemyWins() {
        // GameManager.OnDefeat_ResetStaticsAllManagers();
        gameOverPanel.FadeIn();
    }

    public void OnPlayAgainButton() {
        GameManager.OnDefeat_ResetStaticsAllManagers();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void HandlePlayerWins() {
        StartCoroutine(HandlePlayerWinsRoutine());
    }

    private IEnumerator HandlePlayerWinsRoutine() {
        Floor++;

        yield return StartCoroutine(enemyManager.onPlayerWin_EnemyManager());
        yield return StartCoroutine(playerManager.onPlayerWin_PlayerManager());

        StartCoroutine(playerSoulController.onPlayerWin_PlayerSoulController());
        StartCoroutine(informationUI.OnPlayerWin());
        yield return StartCoroutine(boardManager.onPlayerWin_BoardManager());
        perkManager.onPlayerWin_PerkManager();
    }



    private void OnDisable() {
        EnemyManager.onEnemyCheckMatesThePlayer -= HandleEnemyWins;
        EnemyManager.onEnemyKingsDie -= HandlePlayerWins;
    }
    private void OnDestroy() {
        perkManager.OnPerkSelectionEnded -= LoadNextFloor;
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
