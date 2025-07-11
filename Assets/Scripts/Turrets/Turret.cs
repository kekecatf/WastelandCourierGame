using UnityEngine;

public class Turret : MonoBehaviour
{
    public float attackRange = 5f;
    public float fireRate = 1f;
    public GameObject bulletPrefab;

    private float fireTimer = 0f;

    void Update()
    {
        fireTimer -= Time.deltaTime;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null && fireTimer <= 0)
            {
                Shoot(enemy.transform);
                fireTimer = 1f / fireRate;
                break; // İlk düşmanı vur, çık
            }
        }
    }

    void Shoot(Transform enemyTransform)
{
    GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
    Enemy enemy = enemyTransform.GetComponent<Enemy>();
    if (enemy != null)
    {
        bullet.GetComponent<Bullet>().SetTarget(enemy);
    }
}

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
