using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyModifierData {
    public EnemyType enemyType;
    public int healthChange;
    public int speedChange;
    public List<MovementPattern> movementPatterns;
    public List<MovementPattern> threatPatterns;

    public EnemyModifierData(EnemyTypeSO enemyTypeSO) {
        enemyType = enemyTypeSO.enemyType;
        healthChange = 0;
        speedChange = 0;

        movementPatterns = new List<MovementPattern>(enemyTypeSO.movementPatterns);
        threatPatterns = new List<MovementPattern>(enemyTypeSO.threatPatterns);
    }
}

[System.Serializable]
public class WeaponModifierData {
    public int firePowerChange;     // Number of bullets fired per shot
    public int fireRangeChange;     // Bullet travel distance
    public int fireArcChange;       // Spread angle in degrees

    public int magCapacityChange;         // Max shells loaded into weapon
    public int maxReserveAmmoChange;      // Max reserve shells player can carry
    public int reloadAmountChange;        // Shells loaded per reload action

    public WeaponModifierData() {
        firePowerChange = 0;
        fireRangeChange = 0;
        fireArcChange = 0;

        magCapacityChange = 0;
        maxReserveAmmoChange = 0;
        reloadAmountChange = 0;
    }
}

[System.Serializable]
public class SoulModifierData {
    public int soulSlotChange;
    public bool moveAfterSoulUsageEnable;
    public static readonly int soulSlotDefault = 1;

    public SoulModifierData() {
        soulSlotChange = 0;
        moveAfterSoulUsageEnable = false;
    }
}