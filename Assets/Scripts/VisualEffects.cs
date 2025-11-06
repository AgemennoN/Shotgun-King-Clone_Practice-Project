using System.Collections;
using UnityEngine;

public class VisualEffects : MonoBehaviour {

    [SerializeField] private SpriteRenderer spriteRenderer;
    private Vector3 originalLocalPos;
    private Color originalColor;

    private Coroutine shakeCoroutine;
    //private Coroutine flickerCoroutine;

    private void Awake() {
        if (spriteRenderer == null) {
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        }
        originalLocalPos = transform.localPosition;
        originalColor = spriteRenderer.color;
    }


    public void StartShake(float intensity = 0.035f, float speed = 30f) {
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(Shake(intensity, speed));
    }

    public void StopShake() {
        if (shakeCoroutine != null) {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
        transform.localPosition = originalLocalPos;
    }

    private IEnumerator Shake(float intensity, float speed) {
        while (true) {
            float shakeX = Mathf.Sin(Time.time * speed) * intensity;
            transform.localPosition = originalLocalPos + new Vector3(shakeX, 0f, 0f);
            yield return null;
        }
    }



    public void Flicker(Color flickerColor, float flickerDuration, int flickerCount) {
        StartCoroutine(FlickerRoutine(flickerCount, flickerDuration, flickerColor));
    }

    private IEnumerator FlickerRoutine(int flickerCount, float flickerDuration, Color flickerColor) {
        for (int i = 0; i < flickerCount; i++) {
            spriteRenderer.color = flickerColor;
            yield return new WaitForSeconds(flickerDuration);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flickerDuration);
        }
    }
}
