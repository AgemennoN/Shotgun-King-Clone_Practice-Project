using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Shotgun : Weapon
{
    //[SerializeField] private AimIndicator aimIndicator;
    //[SerializeField] private ArrowIndicator aimIndicator;

    public override void Shoot(Vector3 from, Vector3 to) {
        if (weaponData == null) return;

        Vector2 direction = (to - from);
        direction.Normalize();

        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float halfArc = weaponData.fireArc / 2f;

        for (int i = 0; i < weaponData.firePower; i++) {
            float randomAngle = UnityEngine.Random.Range(-halfArc, halfArc);
            Quaternion rotation = Quaternion.Euler(0, 0, baseAngle + randomAngle);

            Bullet bullet = bulletPool.GetBullet().GetComponent<Bullet>();
            bullet.transform.position = from;
            bullet.transform.rotation = rotation;
            bullet.gameObject.SetActive(true);

            bullet.Initialize(
                weaponData.bulletSpeed,
                weaponData.bulletDamage,
                weaponData.fireRange,
                bulletPool,
                i
            );
        }

    }

    public override void Aim(bool enable) {
        //if (enable) {
        //    aimIndicator.Show(new Vector3(),new Vector3());
        //} else {
        //    aimIndicator.Hide();
        //}
        throw new System.NotImplementedException();
    }


    protected override void Reload() {
        throw new System.NotImplementedException();
    }
}
