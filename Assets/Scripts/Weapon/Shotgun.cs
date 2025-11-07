using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Shotgun : Weapon
{

    public override bool Shoot(Vector3 to) {
        Vector3 from = transform.position;
        bool isShooted = false;
        if (weaponData == null) return isShooted;
        if (currentMag == 0) return isShooted;
        isShooted = true;

        Vector2 direction = (to - from);
        direction.Normalize();

        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float halfArc = weaponData.fireArc / 2f;

        int bulletsRemaining = weaponData.firePower;
        TurnManager.Instance.RegisterAction(WaitForBulletsToFinish(() => bulletsRemaining == 0));

        for (int i = 0; i < weaponData.firePower; i++) {
            float randomAngle = UnityEngine.Random.Range(-halfArc, halfArc);
            Quaternion rotation = Quaternion.Euler(0, 0, baseAngle + randomAngle);

            Bullet bullet = bulletPool.GetBullet().GetComponent<Bullet>();
            bullet.transform.position = from;
            bullet.transform.rotation = rotation;
            bullet.gameObject.SetActive(true);

            float fireRange = weaponData.fireMaxRange;
            if (i > (weaponData.firePower / 3)-1) {
                fireRange = UnityEngine.Random.Range((float)weaponData.fireMinRange, (float)weaponData.fireMaxRange);
            }

            bullet.Initialize(
                    weaponData.bulletSpeed,
                    weaponData.bulletDamage,
                    fireRange,
                    bulletPool,
                    i
                );

            //TO DO: Check if this method prevents memory leak
            Action<Bullet> onFinish = null;
            onFinish = (b) => {
                bulletsRemaining--;
                bullet.OnBulletFinished -= onFinish;
            };
            bullet.OnBulletFinished += onFinish;
        }

        currentMag -= 1;
        Aim(false);
        return isShooted;
    }

    private IEnumerator WaitForBulletsToFinish(Func<bool> allDoneCondition) {
        // TO DO: Is this the best placement for this IEnumerator?
        yield return new WaitUntil(allDoneCondition);
    }
}
