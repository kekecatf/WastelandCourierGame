using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;
    public float lifeTime = 3f;
    private Rigidbody2D rb;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void Launch(Vector2 direction, float speed)
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = direction.normalized * speed;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            return; // oyuncuya çarpmasın

        // devamı:
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        Animal animal = collision.GetComponent<Animal>();
        if (animal != null)
        {
            animal.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        Destroy(gameObject);
    }


}

