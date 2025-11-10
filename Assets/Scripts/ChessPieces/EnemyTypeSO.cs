using System.Collections.Generic;
using UnityEngine;

public enum MovementType {
    FiniteStep,     // Moves N tiles in a direction (e.g., 2 tiles diagonally)
    InfiniteStep,   // Moves in a direction until blocked or board boundary
    Jump            // Moves to a specific offset regardless of obstacles
}

[System.Serializable]
public struct MovementPattern {
    public Vector2Int direction;
    public MovementType movementType;
    public int maxDistance;
}


[CreateAssetMenu(fileName = "EnemyTypeSO", menuName = "Scriptable Objects/EnemyTypeSO")]
public class EnemyTypeSO : ScriptableObject
{
    public EnemyType enemyType;
    public int maxHealth;
    public int speed;

    public List<MovementPattern> movementPatterns;
    public bool isThreatSameWithMovement = false;
    public List<MovementPattern> threatPatterns;

}
