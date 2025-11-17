using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadingPanel : MonoBehaviour {

    [SerializeField] public bool startInvisible = true;
    private CanvasGroup canvasGroup;
    private Coroutine fadeRoutine;

    private void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();

        if (startInvisible) {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
        }
    }

    public Coroutine FadeIn(float duration = 0.5f) {
        gameObject.SetActive(true);
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(0f, 1f, duration));
        return fadeRoutine;
    }

    public Coroutine FadeOut(float duration = 0.5f) {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(1f, 0f, duration));
        return fadeRoutine;
    }

    private IEnumerator FadeRoutine(float from, float to, float duration) {
        float t = 0f;

        canvasGroup.interactable = false;

        while (t < duration) {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }

        canvasGroup.alpha = to;
        canvasGroup.interactable = (to == 1f);
    }
}
