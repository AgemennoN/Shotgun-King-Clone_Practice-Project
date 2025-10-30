using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected WeaponDataSO weaponData;

    protected int currentMag;         // Current number of shells currently loaded in the weapon
    protected int currentReserveAmmo; // Current number of reserve shells available for reloading
    [SerializeField] protected BulletPool bulletPool;

    protected virtual void Start() {
        currentMag = weaponData.magCapacity;
        currentReserveAmmo = weaponData.maxReserveAmmo;
        bulletPool.SetInitialBulletSize(weaponData.firePower);
        bulletPool.Initialize();
    }

    public abstract void Shoot(Vector3 from, Vector3 to);
    public abstract void Aim(bool enable);
    protected abstract void Reload();

}
