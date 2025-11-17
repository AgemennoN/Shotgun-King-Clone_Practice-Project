using UnityEngine;

public class AmmoUI : FillableUIIcon {
    public static AmmoUI Create(Transform parent, Sprite filled, Sprite empty) {
        GameObject go = new GameObject("AmmoUI");
        go.transform.SetParent(parent, false);

        AmmoUI ui = go.AddComponent<AmmoUI>();
        ui.filledSprite = filled;
        ui.emptySprite = empty;
        ui.Initialize(new Vector2(15, 30));

        ui.SetFilled(true);
        return ui;
    }
}