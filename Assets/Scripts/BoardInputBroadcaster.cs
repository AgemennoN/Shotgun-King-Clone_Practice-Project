using System;
using UnityEngine;

public class BoardInputBroadcaster : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask boardTileLayerMask;

    // --- Events ---
    public event Action<BoardTile> OnTileHovered;
    public event Action<BoardTile, Vector3> OnTileClicked;

    private BoardTile currentHoveredTile;
    private Vector3 lastMousePosition;

    private void Start() {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update() {
        // Only check if mouse moves or on click
        if (Input.mousePosition != lastMousePosition) {
            lastMousePosition = Input.mousePosition;
            CheckTileHover();
        }

        if (Input.GetMouseButtonDown(0)) {
            CheckTileClick();
        }
    }

    private void CheckTileHover() {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, boardTileLayerMask);

        BoardTile hoveredTile = hit.collider ? hit.collider.GetComponent<BoardTile>() : null;

        if (hoveredTile != currentHoveredTile) {
            currentHoveredTile = hoveredTile;
            OnTileHovered?.Invoke(currentHoveredTile); // Broadcast hover change
        }
    }

    private void CheckTileClick() {
        if (currentHoveredTile != null) {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log($"Clicked v3: {mouseWorldPos}");

            OnTileClicked?.Invoke(currentHoveredTile, mouseWorldPos); // Broadcast click
        }
    }
}
