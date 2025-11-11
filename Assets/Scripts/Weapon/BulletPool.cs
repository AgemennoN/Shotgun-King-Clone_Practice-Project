using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour {
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int initialSize = 5;

    private Queue<GameObject> pool = new Queue<GameObject>();


    public void Initialize(){
        for (int i = 0; i < initialSize; i++) {
            GameObject bullet = Instantiate(bulletPrefab, transform);
            bullet.gameObject.SetActive(false);
            pool.Enqueue(bullet);
        }

    }

    public GameObject GetBullet() {
        if (pool.Count > 0)
            return pool.Dequeue();
        else
            return Instantiate(bulletPrefab, transform);
    }

    public void ReturnBullet(GameObject bullet) {
        bullet.gameObject.SetActive(false);
        pool.Enqueue(bullet);
    }

    public void SetInitialBulletSize(int bulletNumber) {
        initialSize = bulletNumber;
    }
}
