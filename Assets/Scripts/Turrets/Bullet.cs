using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    private Enemy targetEnemy;
    private bool hasHit = false; // Hasar verildi mi kontrolü

    public void SetTarget(Enemy enemy)
    {
        targetEnemy = enemy;
    }

    void Update()
    {
        // Düşman yok olduysa veya null ise mermiyi yok et
        if (targetEnemy == null || targetEnemy.gameObject == null)
        {
            Destroy(gameObject);
            return;
        }

        // Zaten hasar verdiyse hareket etme
        if (hasHit)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 dir = (targetEnemy.transform.position - transform.position).normalized;
        transform.position += (Vector3)(dir * speed * Time.deltaTime);

        float distance = Vector2.Distance(transform.position, targetEnemy.transform.position);
        
        // Mesafe kontrolü - daha büyük bir değer kullan
        if (distance < 0.3f && !hasHit)
        {
            hasHit = true;
            Debug.Log($"Bullet hit enemy! Dealing {damage} damage. Distance: {distance}");
            targetEnemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    // Collision ile de hasar verebilmek için
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return; // Zaten hasar verdiyse tekrar verme
        
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null && enemy == targetEnemy)
        {
            hasHit = true;
            Debug.Log("Bullet hit via collision!");
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}