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

    private Dictionary<EnemyType, EnemyModifierData> enemyModifierDictionary;
    private Dictionary<EnemyType, int> enemySpawnModificationDict;
    private WeaponModifierData weaponModifierData;
    private SoulModifierData soulModifierData;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        enemyModifierDictionary = new Dictionary<EnemyType, EnemyModifierData>();
        enemySpawnModificationDict =  new Dictionary<EnemyType, int>();
        weaponModifierData = null;
        soulModifierData = null;
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

        int index1 = UnityEngine.Random.Range(0, enemyPerks_Available.Count);
        int index2;

        do {
            index2 = UnityEngine.Random.Range(0, enemyPerks_Available.Count);
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

        ApplyPerk(newPerk);
    }

    private void ApplyPerk(PerkCardSO perk) {
        List<PerkEffect> perkEffects = perk.perkEffectList;

        foreach(PerkEffect perkEffect in perkEffects) {
            perkEffect.ApplyEffect(this);
        }
    }


    internal void Apply_PatternEffect(List<MovementPattern> patternList, EnemyTypeSO effectedTypeSO, bool isForMovement, bool isForThreat, bool changeCurrentPattern) {
        if (!enemyModifierDictionary.TryGetValue(effectedTypeSO.enemyType, out EnemyModifierData modifierData))
            modifierData = new EnemyModifierData(effectedTypeSO);

        if (changeCurrentPattern) {
            if (isForMovement)
                modifierData.movementPatterns = new List<MovementPattern>(patternList);
            if (isForThreat)
                modifierData.threatPatterns = new List<MovementPattern>(patternList);
        } else {
            if (isForMovement)
                modifierData.movementPatterns.AddRange(patternList);
            if (isForThreat)
                modifierData.threatPatterns.AddRange(patternList);
        }

        enemyModifierDictionary[effectedTypeSO.enemyType] = modifierData;
    }
    
    internal void Apply_EnemyEffect(PerkEffectType perkEffectType, EnemyTypeSO effectedTypeSO, int effectAmount) {
        if (perkEffectType == PerkEffectType.EnemyNumber) {
            if (enemySpawnModificationDict.TryGetValue(effectedTypeSO.enemyType, out int changedNumber)) {
                changedNumber += effectAmount;
            } else {
                changedNumber = effectAmount;
            }
            enemySpawnModificationDict[effectedTypeSO.enemyType] = changedNumber;
        } else {
            if (!enemyModifierDictionary.TryGetValue(effectedTypeSO.enemyType, out EnemyModifierData modifierData))
                modifierData = new EnemyModifierData(effectedTypeSO);
            if (perkEffectType == PerkEffectType.EnemyHP) {
                modifierData.healthChange += effectAmount;
            } else if (perkEffectType == PerkEffectType.EnemySpeed) {
                modifierData.speedChange -= effectAmount;   // Substract because, speed means faster cd. +1 in description means -1 in code
            }
            enemyModifierDictionary[effectedTypeSO.enemyType] = modifierData;
        }
    }
    internal void Apply_WeaponEffect(PerkEffectType perkEffectType, int effectAmount) {
        if (weaponModifierData == null)
            weaponModifierData = new WeaponModifierData();

        switch (perkEffectType) {
            case PerkEffectType.AmmoWeapon:
                weaponModifierData.magCapacityChange += effectAmount;
                break;
            case PerkEffectType.AmmoReserve:
                weaponModifierData.maxReserveAmmoChange += effectAmount;
                break;
            case PerkEffectType.AmmoRegen:
                weaponModifierData.reloadAmountChange += effectAmount;
                break;
            case PerkEffectType.FirePower:
                weaponModifierData.firePowerChange += effectAmount;
                break;
            case PerkEffectType.FireRange:
                weaponModifierData.fireRangeChange += effectAmount;
                break;
            case PerkEffectType.FireArc:
                weaponModifierData.fireArcChange += effectAmount;
                break;
            default:
                break;
        }
    }

    internal void Apply_SoulEffect(PerkEffectType perkEffectType, int effectAmount) {
        if (soulModifierData == null)
            soulModifierData = new SoulModifierData();

        switch (perkEffectType) {
            case PerkEffectType.SoulSlot:
                soulModifierData.soulSlotChange += effectAmount;
                break;
            case PerkEffectType.MoveAfterSoul:
                soulModifierData.moveAfterSoulUsageEnable = true;
                break;
            default:
                break;
        }
    }

    private List<PerkCardSO> GetAvailablePlayerPerkListFromResource() {
        PerkCardListSO perkListSO = Resources.Load<PerkCardListSO>("PerkList/PlayerPerksList");
        return new List<PerkCardSO>(perkListSO.perkList);
    }

    private List<PerkCardSO> GetAvailableEnemyPerkListFromResource() {
        PerkCardListSO perkListSO = Resources.Load<PerkCardListSO>("PerkList/EnemyPerksList");
        return new List<PerkCardSO>(perkListSO.perkList);
    }
    
    private (PerkCardSO, PerkCardSO) GetHomecomingsAndAddQueensToListFromResource() {
        PerkCardListSO queenPerkListSO = Resources.Load<PerkCardListSO>("PerkList/AddQueenPerksList");
        enemyPerks_Available.AddRange(new List<PerkCardSO>(queenPerkListSO.perkList));

        PerkCardSO queenHomecoming_1 = Resources.Load<PerkCardSO>("SpecialPerk/Special_QueenHomecoming_1");
        PerkCardSO queenHomecoming_2 = Resources.Load<PerkCardSO>("SpecialPerk/Special_QueenHomecoming_2");
        return (queenHomecoming_1, queenHomecoming_2);
    }


    public EnemyModifierData GetEnemyModifierFor(EnemyType enemyType) {
        enemyModifierDictionary.TryGetValue(enemyType, out var enemyModifier);
        return enemyModifier;
    }

    public Dictionary<EnemyType, int> GetEnemySpawnModificationDict() {
        return enemySpawnModificationDict;
    }

    public WeaponModifierData GetWeaponModifierData() {
        return weaponModifierData;
    }

    public SoulModifierData GetSoulModifierData() {
        return soulModifierData;
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
