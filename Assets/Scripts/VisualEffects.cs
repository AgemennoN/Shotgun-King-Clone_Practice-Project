using System.Collections;
using TMPro;
using UnityEngine;



public class VisualEffects : MonoBehaviour {

    [SerializeField] private EnemyPiece enemyPiece;

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
        }
        textMesh.enabled = false;

        enemyPiece = GetComponent<EnemyPiece>();
        enemyPiece.OnDeath += DeathAnimation;
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

    public Coroutine Flicker(Color flickerColor, float flickerDuration, int flickerCount) {
        return StartCoroutine(FlickerRoutine(flickerColor, flickerDuration, flickerCount));
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

    private void DeathAnimation(EnemyPiece enemyPiece) {
        StartCoroutine(DeathAnimationRoutine(enemyPiece));
    }

    private IEnumerator DeathAnimationRoutine(EnemyPiece enemyPiece) {

        float elapsed = 0f;
        float spriteFadeDuration = 1f;
        
        while (elapsed < spriteFadeDuration) {
            elapsed += Time.deltaTime;
            float t = elapsed / textFadeDuration;
            spriteRenderer.color = new Color(originalSpriteColor.r, originalSpriteColor.g, originalSpriteColor.b, 1 - t);
            yield return null;
        }

        // TO DO: DESTROY ON THE ANIMATION SCRIPT?????????????????????????? FIX IT
        Destroy(enemyPiece.gameObject);
    }
}
