using System;
using UnityEngine;


[RequireComponent(typeof(BulletPool))]
[RequireComponent(typeof(AimIndicator))]
public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected WeaponDataSO baseWeaponData;
    [SerializeField] protected WeaponDataSO weaponData;

    protected int currentMag;         // Current number of shells currently loaded in the weapon
    protected int currentReserveAmmo; // Current number of reserve shells available for reloading
    protected BulletPool bulletPool;
    protected AimIndicator aimIndicator;
    private bool isAiming = false;
    public Action<int> OnShoot; // send CurrentMag
    public Action<int,int> OnReloadMag; // send CurrentMag,CurrentReserve
    public Action<int> OnRegenerateReserve; // send CurrentReserve

    protected virtual void Awake() {
        bulletPool = GetComponent<BulletPool>();
        aimIndicator = GetComponent<AimIndicator>();
    }

    protected virtual void Start() {
        aimIndicator.Hide();
    }

    public void InitializeWeapon(WeaponModifierData weaponModifierData = null) {
        ApplyWeaponModifier(this, weaponModifierData);

        currentMag = weaponData.magCapacity;
        currentReserveAmmo = weaponData.maxReserveAmmo;

        bulletPool.SetInitialBulletSize(weaponData.firePower);
        bulletPool.Initialize();

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

        currentMag += toReload;
        currentReserveAmmo -= toReload;
        OnReloadMag?.Invoke(currentMag, currentReserveAmmo); // Weapon UI subscribe this

    }

    protected virtual void RegenerateReserve() {
        int needed = weaponData.maxReserveAmmo - currentReserveAmmo;
        int toReload = Math.Min(needed, weaponData.reloadAmount);

        currentReserveAmmo += toReload;
        OnRegenerateReserve?.Invoke(currentReserveAmmo); // Weapon UI subscribe this

    }

    internal static void ApplyWeaponModifier(Weapon weapon, WeaponModifierData weaponModifierData) {
        weapon.weaponData = Instantiate(weapon.baseWeaponData);
        if (weaponModifierData != null) {
            weapon.weaponData.magCapacity += weaponModifierData.magCapacityChange;
            weapon.weaponData.maxReserveAmmo += weaponModifierData.maxReserveAmmoChange;
            weapon.weaponData.reloadAmount += weaponModifierData.reloadAmountChange;

            weapon.weaponData.firePower += weaponModifierData.firePowerChange;
            weapon.weaponData.fireMaxRange += weaponModifierData.fireRangeChange;
            weapon.weaponData.fireMinRange += weaponModifierData.fireRangeChange;
            weapon.weaponData.fireArc += weaponModifierData.fireArcChange;
        }

    }

    internal WeaponDataSO GetWeaponData() {
        return weaponData;
    }
}
