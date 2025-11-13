using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PerkCardListSO", menuName = "Scriptable Objects/PerkCardListSO")]
public class PerkCardListSO : ScriptableObject {
    public List<PerkCardSO> perkList;
}
