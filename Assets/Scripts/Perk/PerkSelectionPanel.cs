using System.Collections;
using TMPro;
using UnityEditor.UI;
using UnityEngine;



public class PerkSelectionPanel : MonoBehaviour {

    [SerializeField] private PerkChoiseBox perkChoise_1;
    [SerializeField] private PerkChoiseBox perkChoise_2;
    private FadingPanel fadingPanel;

    public void UpdateChoiseBoxes(
        PerkCardSO playerPerk_1, PerkCardSO enemyPerk_1,
        PerkCardSO playerPerk_2, PerkCardSO enemyPerk_2) {

        perkChoise_1.UpdatePerks(playerPerk_1, enemyPerk_1);
        perkChoise_2.UpdatePerks(playerPerk_2, enemyPerk_2);
    }

    public IEnumerator FadeIn(float duration = 0.5f) {
        gameObject.SetActive(true);
        fadingPanel ??= gameObject.AddComponent<FadingPanel>();
        yield return fadingPanel.FadeIn(duration);
    }

    public IEnumerator FadeOut(float duration = 0.5f) {
        gameObject.SetActive(false);
        fadingPanel ??= gameObject.AddComponent<FadingPanel>();
        yield return fadingPanel.FadeOut(duration);
    }

}
