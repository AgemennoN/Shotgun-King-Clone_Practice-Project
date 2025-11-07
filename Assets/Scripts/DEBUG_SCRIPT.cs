using UnityEngine;

public class DEBUG_SCRIPT : MonoBehaviour {
    [SerializeField] Weapon shotgun;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.A)) {
            shotgun.Aim(true);
        }
        if (Input.GetKeyDown(KeyCode.S)) {
            shotgun.Aim(false);
        }
    }
}
