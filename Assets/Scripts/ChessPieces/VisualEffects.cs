using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.VFX;



public class VisualEffects : MonoBehaviour {

    [Header("Sprite Effects")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    private Vector3 originalSpriteLocalPos;
    private Color originalSpriteColor;
    private Coroutine shakeCoroutine;

    [Header("Text Effects")]
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private float textFloatSpeed = 0.75f;
    [SerializeField] private float textFadeDuration = 1f;
    [SerializeField] private Color textColor = Color.red;
    [SerializeField] private Vector3 textLocalPosOffset = new Vector3(0.3f, 0.3f, 0);


    private void Awake() {
        if (spriteRenderer == null) {
            spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
        }
        originalSpriteLocalPos = spriteRenderer.transform.localPosition;
        originalSpriteColor = spriteRenderer.color;

        if (textMesh == null) {
            textMesh = gameObject.GetComponentInChildren<TextMeshPro>();
            if (textMesh != null) textMesh.enabled = false;
        }
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
        spriteRenderer.transform.localPosition = originalSpriteLocalPos;
    }

    private IEnumerator Shake(float intensity, float speed) {
        while (true) {
            float shakeX = Mathf.Sin(Time.time * speed) * intensity;
            spriteRenderer.transform.localPosition = originalSpriteLocalPos + new Vector3(shakeX, 0f, 0f);
            yield return null;
        }
    }

    public void Flicker(Color flickerColor, float flickerDuration, int flickerCount, bool register=false) {
        if (register) {
            TurnManager.Instance.RegisterAction(FlickerRoutine(flickerColor, flickerDuration, flickerCount));
        } else {
            StartCoroutine(FlickerRoutine(flickerColor, flickerDuration, flickerCount));
        }
    }

    public IEnumerator FlickerRoutine(Color flickerColor, float flickerDuration, int flickerCount) {
        for (int i = 0; i < flickerCount; i++) {
            spriteRenderer.color = flickerColor;
            yield return new WaitForSeconds(flickerDuration);
            spriteRenderer.color = originalSpriteColor;
            yield return new WaitForSeconds(flickerDuration);
        }
    }

    public void ShowDamage(int amount) {
        if (textMesh == null) return;

        StopCoroutine(nameof(FloatAndFade)); // ensure no overlap
        textMesh.text = amount.ToString();
        textMesh.color = textColor;

        // Reset text position
        textMesh.transform.localPosition = textLocalPosOffset;
        textMesh.enabled = true;

        StartCoroutine(FloatAndFade());
    }

    private IEnumerator FloatAndFade() {
        float elapsed = 0f;
        Vector3 startPos = textLocalPosOffset;

        while (elapsed < textFadeDuration) {
            elapsed += Time.deltaTime;
            float t = elapsed / textFadeDuration;

            textMesh.transform.localPosition = startPos + Vector3.up * (textFloatSpeed * t);
            textMesh.color = new Color(textColor.r, textColor.g, textColor.b, 1 - t);

            yield return null;
        }

        textMesh.enabled = false;
    }

    public void SpriteFadeOutAnimation(float fadeDuration = 1f, bool register = false) {
        if (register) {
            TurnManager.Instance.RegisterAction(SpriteFadeOutAnimationRoutine(fadeDuration));
        } else {
            StartCoroutine(SpriteFadeOutAnimationRoutine(fadeDuration));
        }
    }

    private IEnumerator SpriteFadeOutAnimationRoutine(float fadeDuration = 1f) {
        float elapsed = 0f;
        while (elapsed < fadeDuration) {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            spriteRenderer.color = new Color(originalSpriteColor.r, originalSpriteColor.g, originalSpriteColor.b, 1 - t);
            yield return null;
        }
    }

    public void SpriteFadeInAnimation(float fadeDuration = 1f, bool register=false) {
        if (register) {
            TurnManager.Instance.RegisterAction(SpriteFadeInAnimationRoutine(fadeDuration));
        } else {
            StartCoroutine(SpriteFadeInAnimationRoutine(fadeDuration));
        }
    }

    private IEnumerator SpriteFadeInAnimationRoutine(float fadeDuration = 1f) {
        float elapsed = 0f;
        while (elapsed < fadeDuration) {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            spriteRenderer.color = new Color(originalSpriteColor.r, originalSpriteColor.g, originalSpriteColor.b, t);
            yield return null;
        }
    }

    public Coroutine SpawnAnimation_Descend(float startDelay = 0.5f) {
        return StartCoroutine(SpawnAnimation_DescendRoutine(startDelay));
    }

    private IEnumerator SpawnAnimation_DescendRoutine(float startDelay, float duration = 0.6f) {
        spriteRenderer.color = new Color(originalSpriteColor.r, originalSpriteColor.g, originalSpriteColor.b, 0);
        spriteRenderer.transform.localPosition = originalSpriteLocalPos + new Vector3(0, 2.5f, 0);
        
        yield return new WaitForSeconds(startDelay);

        float elapsed = 0f;
        Vector3 startPos = spriteRenderer.transform.localPosition;
        Vector3 endPos = originalSpriteLocalPos;

        Color startColor = spriteRenderer.color;
        Color endColor = originalSpriteColor;

        // Smoothly move down and fade in
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0, 1, t); // ease in-out curve

            spriteRenderer.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            spriteRenderer.color = Color.Lerp(startColor, endColor, t);

            yield return null;
        }

        // Snap to final position & color (in case of float rounding)
        spriteRenderer.transform.localPosition = endPos;
        spriteRenderer.color = endColor;
    }



    public Coroutine DestroyAnimation_Ascend(float duration = 0.5f, float ascendDistance = 1.5f) {
        return StartCoroutine(DestroyAnimation_AscendRoutine(duration, ascendDistance));
    }

    private IEnumerator DestroyAnimation_AscendRoutine(float duration = 0.5f, float ascendDistance = 1.5f) {
        float elapsed = 0f;
        Vector3 startPos = spriteRenderer.transform.localPosition;
        Vector3 endPos = startPos + new Vector3(0, ascendDistance, 0);

        Color startColor = spriteRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0); ;

        // Smoothly move down and fade in
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0, 1, t); // ease in-out curve

            spriteRenderer.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            spriteRenderer.color = Color.Lerp(startColor, endColor, t);

            yield return null;
        }

        Destroy(gameObject);
    }

}
