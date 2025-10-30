using UnityEngine;

public class Bullet : MonoBehaviour {
    private float speed;
    private int damage;
    private float maxDistance;
    private Vector3 startPosition;

    private BulletPool pool;

    public void Initialize(float speed, int damage, float maxDistance, BulletPool pool, int bulletNumberDebug) {
        this.speed = speed;
        this.damage = damage;
        this.maxDistance = maxDistance;
        this.pool = pool;
        startPosition = transform.position;
        Debug.Log($"BULLET_Initialize_{bulletNumberDebug}");
    }

    private void Update() {
        transform.Translate(Vector3.up * speed * Time.deltaTime); // move forward

        if (Vector3.Distance(startPosition, transform.position) > maxDistance) {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        var enemy = collision.GetComponent<EnemyPiece>();
        if (enemy != null) {
            enemy.TakeDamage(damage);
            ReturnToPool();
        }
    }

    private void ReturnToPool() {
        Debug.Log("BULLET_ReturnToPool");

        pool.ReturnBullet(this.gameObject);
    }
}
