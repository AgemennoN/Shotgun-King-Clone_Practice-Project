using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PerkEffect {
    public PerkEffectType PerkEffectType;
}

[System.Serializable]
public class PerkEffect_Pattern : PerkEffect {
    public List<MovementPattern> patternList;
    public EnemyTypeSO effectedType;
    public bool changeCurrentPattern;
    public bool isForMovement;
    public bool isForThreat;
}

[System.Serializable]
public class PerkEffect_Enemy : PerkEffect {
    public EnemyTypeSO effectedType;
    public int effectAmount;
}

[System.Serializable]
public class PerkEffect_Weapon : PerkEffect {
    public int effectAmount;
}

[System.Serializable]
public class PerkEffect_Soul : PerkEffect {
    public int effectAmount;
}