using UnityEngine;

public class Turret : MonoBehaviour
{
    public float attackRange = 5f;
    public float fireRate = 1f;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;

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
                Shoot(enemy.transform.position);
                fireTimer = 1f / fireRate;
                break; // İlk düşmanı vur, çık
            }
        }
    }

    void Shoot(Vector3 targetPosition)
{
    Vector3 direction = (targetPosition - transform.position).normalized;

    GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

    WeaponBullet bulletScript = bullet.GetComponent<WeaponBullet>();
    if (bulletScript != null)
    {
        bulletScript.Launch(direction);
    }
}


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
