using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSoulController : MonoBehaviour {

    public static PlayerSoulController Instance { get; private set; }

    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private GameObject soulTemplate;
    [SerializeField] private Transform soulUI;


    [SerializeField] private List<SoulSlot> soulSlots;
    private SoulSlot activeSoulSlot;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    private void Start() {
        playerManager = PlayerManager.Instance;

        playerManager.OnSoulMovement = ConsumeActiveSoul;
        EnemyPiece.OnSoulHarvested += HarvestSoul;
    }

    public void Initialize() {
        soulSlots = new List<SoulSlot>();
        while (soulSlots.Count < SoulModifierData.soulSlotDefault) {
            AddNewEmptySlotToList();
        }
    }

    public void HarvestSoul(EnemyPiece enemyPiece) {
        for (int i = 0; i < soulSlots.Count; i++) {
            if (soulSlots[i].soul == null) {
                soulSlots[i].SetSoul(enemyPiece.enemyTypeSO);
                return;
            }
        }
    }

    public void ConsumeActiveSoul() {
        if (activeSoulSlot == null) {
            return;
        }

        // Remove active slot and create a new one
        soulSlots.Remove(activeSoulSlot);
        Destroy(activeSoulSlot.gameObject);

        ExitSoulMode();

        AddNewEmptySlotToList();
    }

    private void AddNewEmptySlotToList() {
        GameObject obj = Instantiate(soulTemplate, soulUI);
        SoulSlot soulSlot = obj.GetComponent<SoulSlot>();
        soulSlot.playerSoulController = this;
        soulSlots.Add(soulSlot);
    }

    private void EnterSoulMode() {
        playerManager.EnterSoulMode(activeSoulSlot.soul);
    }

    private void ExitSoulMode() {
        playerManager.ExitSoulMode();
    }

    public void OnSlotClicked(SoulSlot slot) {
        if (TurnManager.Instance.IsPlayerTurn() == false) return;

        if (activeSoulSlot == slot) {   // Toggle by clicking same soul
            DeselectCurrent();
            return;
        }

        if (activeSoulSlot != null)
            activeSoulSlot.SetSelected(false);

        activeSoulSlot = slot;
        slot.SetSelected(true);

        EnterSoulMode();
    }

    public void DeselectCurrent() {
        if (activeSoulSlot != null) {
            activeSoulSlot.SetSelected(false);
            activeSoulSlot = null;

            ExitSoulMode();
        }
    }

    public void ApplySoulModification(SoulModifierData soulModifierData) {
        if (soulModifierData == null) return;

        int newSoulSlotNumber = SoulModifierData.soulSlotDefault + soulModifierData.soulSlotChange;

        newSoulSlotNumber = Math.Max(0, newSoulSlotNumber);
        while (soulSlots.Count > newSoulSlotNumber) {
            SoulSlot slotToBeDestroyed = soulSlots[soulSlots.Count - 1];
            soulSlots.Remove(slotToBeDestroyed);
            Destroy(slotToBeDestroyed.gameObject);
        }
        while (soulSlots.Count < newSoulSlotNumber)
            AddNewEmptySlotToList();
    }

    public IEnumerator NewFloorPreparation(SoulModifierData soulModifierData) {
        ApplySoulModification(soulModifierData);
        yield return soulUI.GetComponent<FadingPanel>().FadeIn(2f);
    }

    public IEnumerator onPlayerWin_PlayerSoulController() {
        yield return soulUI.GetComponent<FadingPanel>().FadeOut(2f);
    }


    private void Update() {
        if (Input.GetMouseButtonDown(1)) {
            DeselectCurrent();
        }
    }

    private void OnDestroy() {
        EnemyPiece.OnSoulHarvested -= HarvestSoul;
    }
}
