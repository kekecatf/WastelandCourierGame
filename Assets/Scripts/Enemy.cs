using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 2f;
    public int damage = 1;
    public Transform target;

    void Start()
    {
        if (target == null)
        {
            GameObject t = GameObject.FindWithTag("Castle");
            if (t != null)
                target = t.transform;
        }
    }

    void Update()
    {
        if (target == null) return;

        Vector2 direction = (target.position - transform.position).normalized;
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    public void TakeDamage(int amount)
    {
        // İleride sağlık eklenebilir
        Destroy(gameObject);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Castle"))
        {
            CastleHealth castle = collision.GetComponent<CastleHealth>();
            if (castle != null)
            {
                castle.TakeDamage(damage);
                Destroy(gameObject); // Kendini yok et
            }
        }
    }

}
