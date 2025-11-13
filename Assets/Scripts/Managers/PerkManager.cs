using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PerkManager : MonoBehaviour {
    public static PerkManager Instance { get; private set; }
    public static readonly int PerkSlotNumber = 10;

    [SerializeField] private  List<PerkCardSO> playerPerks_Available;
    [SerializeField] private  List<PerkCardSO> enemyPerks_Available;
    [SerializeField] public  List<PerkCardSO> playerPerks_Chosen;
    [SerializeField] public  List<PerkCardSO> enemyPerks_Chosen;


    [SerializeField] private PerkSelectionPanel perkSelectionPanel;


    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Initialize() {
        playerPerks_Chosen = new List<PerkCardSO>();
        enemyPerks_Chosen = new List<PerkCardSO>();
        playerPerks_Available = GetAvailablePlayerPerkListFromResource();
        enemyPerks_Available = GetAvailableEnemyPerkListFromResource();
    }

    private void SendPerksToTheSelectionPanel() {
        PerkCardSO playerPerk_1;
        PerkCardSO playerPerk_2;
        (playerPerk_1, playerPerk_2) = SelectPerksFromPlayerPerkList();
        PerkCardSO enemyPerk_1;
        PerkCardSO enemyPerk_2;
        (enemyPerk_1, enemyPerk_2) = SelectPerksFromEnemyPerkList();

        Debug.Log("PerkManager_HandlePlayerWins");
        Debug.Log("playerPerk_1 " + playerPerk_1 + "enemyPerk_1 " + enemyPerk_1);
        Debug.Log("playerPerk_2 " + playerPerk_2 + "enemyPerk_2 " + enemyPerk_2);

        perkSelectionPanel.UpdateChoiseBoxes(playerPerk_1, enemyPerk_1, playerPerk_2, enemyPerk_2);
    }

    private (PerkCardSO, PerkCardSO) SelectPerksFromPlayerPerkList() {
        int index1 = UnityEngine.Random.Range(0, playerPerks_Available.Count);
        int index2;

        do {
            index2 = UnityEngine.Random.Range(0, playerPerks_Available.Count);
        } while (index2 == index1); // Get different perks

        PerkCardSO perk1 = playerPerks_Available[index1];
        PerkCardSO perk2 = playerPerks_Available[index2];

        return (perk1, perk2);
    }

    private (PerkCardSO, PerkCardSO) SelectPerksFromEnemyPerkList() {
        if (GameManager.Floor == 3) {
            return GetHomecomingsAndAddQueensToListFromResource();
        }

        int index1 = UnityEngine.Random.Range(0, playerPerks_Available.Count);
        int index2;

        do {
            index2 = UnityEngine.Random.Range(0, playerPerks_Available.Count);
        } while (index2 == index1); // Get different perks

        PerkCardSO perk1 = enemyPerks_Available[index1];
        PerkCardSO perk2 = enemyPerks_Available[index2];

        return (perk1, perk2);
    }

    public void AddPerkToList(bool isPlayerPerk, PerkCardSO newPerk) {
        List<PerkCardSO> perkList_Chosen;
        List<PerkCardSO> perklist_Available;

        if (isPlayerPerk) {
            perkList_Chosen = playerPerks_Chosen;
            perklist_Available = playerPerks_Available;
        } else {
            perkList_Chosen = enemyPerks_Chosen;
            perklist_Available = enemyPerks_Available;
        }
        perkList_Chosen.Add(newPerk);
        if(!newPerk.reSelectable)
            perklist_Available.Remove(newPerk);
    }



    // APPLY PERK LOGIC EKLICEM BURAYA ;(



    private List<PerkCardSO> GetAvailablePlayerPerkListFromResource() {
        PerkCardListSO perkListSO = Resources.Load<PerkCardListSO>("PlayerPerksList");
        return new List<PerkCardSO>(perkListSO.perkList);
    }

    private List<PerkCardSO> GetAvailableEnemyPerkListFromResource() {
        PerkCardListSO perkListSO = Resources.Load<PerkCardListSO>("EnemyPerksList");
        return new List<PerkCardSO>(perkListSO.perkList);
    }
    
    private (PerkCardSO, PerkCardSO) GetHomecomingsAndAddQueensToListFromResource() {
        PerkCardListSO queenPerkListSO = Resources.Load<PerkCardListSO>("AddQueenPerks/AddQueenPerksList");
        enemyPerks_Available.AddRange(new List<PerkCardSO>(queenPerkListSO.perkList));

        PerkCardSO queenHomecoming_1 = Resources.Load<PerkCardSO>("AddQueenPerks/QueenHomecoming_1");
        PerkCardSO queenHomecoming_2 = Resources.Load<PerkCardSO>("AddQueenPerks/QueenHomecoming_2");
        return (queenHomecoming_1, queenHomecoming_2);
    }

    public static void ResetStaticVariablesOnDefeat() {
        //Instance = null;
        //PlayerPerks = null;
        //EnemyPerks = null;
    }

    public void onPlayerWin_PerkManager() {
        SendPerksToTheSelectionPanel();
        perkSelectionPanel.SetSelfEnable();

    }

    private void OnDisable() {
    }

}
