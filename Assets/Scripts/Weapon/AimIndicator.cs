using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AimIndicator : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Transform aimOrigin;

    [Header("Arc Settings")]
    [SerializeField] private float fireArc = 57f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 5f;
    [SerializeField] private int segmentCount = 32;

    [Header("Visual Settings")]
    [SerializeField] private Color nearColor = new Color(190,0,0);
    [SerializeField] private Color farColor = Color.black;
    [SerializeField] private float maxFlickerSpeed = 25f;
    [SerializeField] private float lineWidth = 0.05f;

    private LineRenderer line;
    private Camera cam;
    private bool isActive;

    private void Awake() {
        line = GetComponent<LineRenderer>();
        cam = Camera.main;

        line.useWorldSpace = true;
        line.loop = false;
        line.positionCount = segmentCount + 1;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.material = new Material(Shader.Find("Sprites/Default"));

        line.enabled = false;
        isActive = false;
    }

    private void Update() {
        if (!isActive || aimOrigin == null) return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector3 dir = mouseWorld - aimOrigin.position;
        float distance = dir.magnitude;
        float clampedDistance = Mathf.Min(distance, maxDistance);

        float angleToMouse = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        DrawArc(aimOrigin.position, clampedDistance, fireArc, angleToMouse);
        UpdateLineColor(distance);
    }

    private void DrawArc(Vector3 origin, float radius, float arcDegrees, float centerAngle) {
        float startAngle = centerAngle - arcDegrees / 2f;
        float endAngle = centerAngle + arcDegrees / 2f;

        for (int i = 0; i <= segmentCount; i++) {
            float t = (float)i / segmentCount;
            float angle = Mathf.Lerp(startAngle, endAngle, t) * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius + origin;
            line.SetPosition(i, pos);
        }
    }

    private void UpdateLineColor(float distance) {
        if (distance <= minDistance) {
            line.startColor = line.endColor = nearColor;
        } else if (distance >= maxDistance) {
            line.startColor = line.endColor = farColor;
        } else {
            float t = Mathf.InverseLerp(minDistance, maxDistance, distance);
            float flickerSpeed = Mathf.Lerp(1f, maxFlickerSpeed, t);
            float flicker = Mathf.Abs(Mathf.Sin(Time.time * flickerSpeed));
            Color flickerColor = Color.Lerp(nearColor, farColor, flicker);
            line.startColor = line.endColor = flickerColor;
        }
    }

    // --- Public controls for weapon script ---

    public void Show() {
        line.enabled = true;
        isActive = true;
    }

    public void Hide() {
        line.enabled = false;
        isActive = false;
    }

    public void SetValues(Transform aimOrigin, float arc, float minDist, float maxDist) {
        this.aimOrigin = aimOrigin;
        fireArc = arc;
        minDistance = minDist;
        maxDistance = maxDist;
    }
}
