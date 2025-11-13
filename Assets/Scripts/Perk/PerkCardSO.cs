using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "PerkCardSO", menuName = "Scriptable Objects/PerkCardSO")]
public class PerkCardSO : ScriptableObject {
    public string title;
    public string description;
    public Image sprite;

    [SerializeReference]
    public List<PerkEffect> perkEffectList = new List<PerkEffect>();

    public bool reSelectable;

}
