using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PerkChoiseBox : MonoBehaviour {

    [SerializeField] private GameObject playerPerkOverlay;
    [SerializeField] private GameObject enemyPerkOverlay;

    [SerializeField] private PerkCardSO playerPerk;
    [SerializeField] private PerkCardSO enemyPerk;

    private Button button;
    public void UpdatePerks(PerkCardSO playerPerk, PerkCardSO enemyPerk) {
        this.playerPerk = playerPerk;
        this.enemyPerk = enemyPerk;


        Debug.Log("-------------------------------------------------------");
        Debug.Log("PerkChoiseBox ");
        Debug.Log("playerPerk.title: " + playerPerk.title);
        Debug.Log("playerPerk.description: " + playerPerk.description);
        Debug.Log("enemyPerk.title: " + enemyPerk.title);
        Debug.Log("enemyPerk.description: " + enemyPerk.description);
        Debug.Log("-------------------------------------------------------");


        //playerPerkOverlay.transform.Find("image").GetComponent<Image>().sprite = playerPerk.sprite;
        playerPerkOverlay.transform.Find("title").GetComponent<TextMeshProUGUI>().text = playerPerk.title;
        playerPerkOverlay.transform.Find("description").GetComponent<TextMeshProUGUI>().text = playerPerk.description;


        //enemyPerkOverlay.transform.Find("image").GetComponent<Image>().sprite = enemyPerk.sprite;
        enemyPerkOverlay.transform.Find("title").GetComponent<TextMeshProUGUI>().text = enemyPerk.title;
        enemyPerkOverlay.transform.Find("description").GetComponent<TextMeshProUGUI>().text = enemyPerk.description;


        button ??= gameObject.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnPerkSelected);
    }

    private void OnPerkSelected() {
        PerkManager.Instance.AddPerkToList(true, playerPerk);
        PerkManager.Instance.AddPerkToList(false, enemyPerk);
    }
}
