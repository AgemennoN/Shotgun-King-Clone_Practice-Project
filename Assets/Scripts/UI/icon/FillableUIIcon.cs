using UnityEngine;
using UnityEngine.UI;

public abstract class FillableUIIcon : MonoBehaviour {
    protected Image iconImage;
    protected Sprite filledSprite;
    protected Sprite emptySprite;

    public void SetFilled(bool isFilled) {
        iconImage.sprite = isFilled ? filledSprite : emptySprite;
    }

    protected void Initialize(Vector2 size) {
        RectTransform rect = gameObject.AddComponent<RectTransform>();
        rect.sizeDelta = size;

        iconImage = gameObject.AddComponent<Image>();
    }
}