using System;
using UnityEngine;


[RequireComponent(typeof(BulletPool))]
[RequireComponent(typeof(AimIndicator))]
public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected WeaponDataSO weaponData;

    protected int currentMag;         // Current number of shells currently loaded in the weapon
    protected int currentReserveAmmo; // Current number of reserve shells available for reloading
    protected BulletPool bulletPool;
    protected AimIndicator aimIndicator;
    private bool isAiming = false;

    protected virtual void Awake() {
        bulletPool = GetComponent<BulletPool>();
        aimIndicator = GetComponent<AimIndicator>();
    }

    protected virtual void Start() {
        currentMag = weaponData.magCapacity;
        currentReserveAmmo = weaponData.maxReserveAmmo;
        bulletPool.SetInitialBulletSize(weaponData.firePower);
        bulletPool.Initialize();


        aimIndicator.Hide();
        aimIndicator.SetValues(transform, weaponData.fireArc, weaponData.fireMinRange, weaponData.fireMaxRange);
    }

    public abstract bool Shoot(Vector3 to);
    
    public virtual void Aim(bool enable) {
        if (!isAiming && enable && currentMag > 0) {
            isAiming = true;
            aimIndicator.Show();
        }
        else if(isAiming && !enable) {
            isAiming = false;
            aimIndicator.Hide();
        }
    }

    public virtual void Reload() {
        if (currentMag < weaponData.magCapacity && currentReserveAmmo > 0) {
            ReloadMag();
        } else if (currentReserveAmmo < weaponData.maxReserveAmmo) {
            RegenerateReserve();
        }
    }

    protected virtual void ReloadMag() {
        int needed = weaponData.magCapacity - currentMag;
        int toReload = Math.Min(needed, Math.Min(weaponData.reloadAmount, currentReserveAmmo));

        // To Do: Reload Animation
        currentMag += toReload;
        currentReserveAmmo -= toReload;
    }

    protected virtual void RegenerateReserve() {
        int needed = weaponData.maxReserveAmmo - currentReserveAmmo;
        int toReload = Math.Min(needed, weaponData.reloadAmount);

        // To Do: RegenerateReserve animation;
        currentReserveAmmo += toReload;
    }

    public void PrintWeaponInfo() {
        Debug.Log($"Mag:{currentMag}/{weaponData.magCapacity}, Reserve:{currentReserveAmmo}/{weaponData.maxReserveAmmo}");
    }

    private void OnGUI() { // To Do: DEBUG Delete Later
        GUI.Label(new Rect(10, 20, 300, 20), $"Mag: {currentMag}/{weaponData.magCapacity}");
        GUI.Label(new Rect(10, 40, 300, 20), $"Reserve: {currentReserveAmmo}/{weaponData.maxReserveAmmo}");
    }

}
