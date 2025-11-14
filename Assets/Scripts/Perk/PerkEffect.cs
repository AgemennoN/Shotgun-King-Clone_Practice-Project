using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PerkEffect {
    public PerkEffectType perkEffectType;
    public virtual void ApplyEffect(PerkManager perkManager) { }
}

[System.Serializable]
public class PerkEffect_Enemy : PerkEffect {
    public EnemyTypeSO effectedType;
    public int effectAmount;
    public override void ApplyEffect(PerkManager perkManager) {
        perkManager.Apply_EnemyEffect(perkEffectType, effectedType, effectAmount);
    }
}

[System.Serializable]
public class PerkEffect_Pattern : PerkEffect {
    public List<MovementPattern> patternList;
    public EnemyTypeSO effectedType;
    public bool changeCurrentPattern;
    public bool isForMovement;
    public bool isForThreat;

    public override void ApplyEffect(PerkManager perkManager) {
        perkManager.Apply_PatternEffect(patternList, effectedType, isForMovement, isForThreat, changeCurrentPattern);
    }

}

[System.Serializable]
public class PerkEffect_Weapon : PerkEffect {
    public int effectAmount;
    public override void ApplyEffect(PerkManager perkManager) {
        perkManager.Apply_WeaponEffect(perkEffectType, effectAmount);
    }
}

[System.Serializable]
public class PerkEffect_Soul : PerkEffect {
    public int effectAmount;
    public override void ApplyEffect(PerkManager perkManager) {
        perkManager.Apply_SoulEffect(perkEffectType, effectAmount);
    }
}