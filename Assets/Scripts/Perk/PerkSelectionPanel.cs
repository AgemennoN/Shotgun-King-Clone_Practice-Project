using System.Collections;
using TMPro;
using UnityEngine;



public class PerkSelectionPanel : MonoBehaviour {

    [SerializeField] private PerkChoiseBox perkChoise_1;
    [SerializeField] private PerkChoiseBox perkChoise_2;
    [SerializeField] private CanvasGroup canvasGroup;

    public void UpdateChoiseBoxes(
        PerkCardSO playerPerk_1, PerkCardSO enemyPerk_1,
        PerkCardSO playerPerk_2, PerkCardSO enemyPerk_2) {


        Debug.Log("PerkSelectionPanel ");
        Debug.Log("playerPerk_1 " + playerPerk_1 + "enemyPerk_1 " + enemyPerk_1);
        Debug.Log("playerPerk_2 " + playerPerk_2 + "enemyPerk_2 " + enemyPerk_2);

        perkChoise_1.UpdatePerks(playerPerk_1, enemyPerk_1);
        perkChoise_2.UpdatePerks(playerPerk_2, enemyPerk_2);
    }

    public void SetSelfEnable() {
        gameObject.SetActive(true);
    }

    public IEnumerator FadeIn(float duration = 1f) {
        canvasGroup.alpha = 0;
        float t = 0;
        while (t < duration) {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / duration);
            yield return null;
        }
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
    }

    private void OnEnable() {
        if (canvasGroup == null) {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0;
        StartCoroutine(FadeIn(1f));
    }

    private void OnDisable() {
        canvasGroup.interactable = false;
        canvasGroup.alpha = 0;
    }

}
