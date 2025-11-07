using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDataSO", menuName = "Scriptable Objects/WeaponDataSO")]
public class WeaponDataSO : ScriptableObject
{
    public int firePower;           // Number of bullets fired per shot
    public int fireMaxRange;        // Max bullet travel distance
    public int fireMinRange;        // Max bullet travel distance
    public int fireArc;             // Spread angle in degrees
    public float bulletSpeed;       // Bullet travel speed
    public int bulletDamage;

    public int magCapacity;         // Max shells loaded into weapon
    public int maxReserveAmmo;      // Max reserve shells player can carry
    public int reloadAmount;        // Shells loaded per reload action

    //public AudioClip shootSound;
}
