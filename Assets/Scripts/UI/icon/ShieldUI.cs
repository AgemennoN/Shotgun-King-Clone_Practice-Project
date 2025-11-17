using UnityEngine;

public class ShieldUI : FillableUIIcon {
    public static ShieldUI Create(Transform parent, Sprite filled, Sprite empty) {
        GameObject go = new GameObject("ShieldUI");
        go.transform.SetParent(parent, false);

        ShieldUI ui = go.AddComponent<ShieldUI>();
        ui.filledSprite = filled;
        ui.emptySprite = empty;
        ui.Initialize(new Vector2(30, 30));

        ui.SetFilled(true);
        return ui;
    }
}
