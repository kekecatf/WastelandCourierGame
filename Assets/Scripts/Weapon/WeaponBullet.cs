// WeaponBullet.cs (DÜZELTİLMİŞ VE FİZİK UYUMLU HALİ)

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] // Bu script'in olduğu objede Rigidbody2D olmasını zorunlu kılar.
public class WeaponBullet : MonoBehaviour
{
    public float speed = 20f; // Hızı biraz artıralım, daha gerçekçi olur.
    public int damage = 10;
    private Rigidbody2D rb;
    private bool hasHit = false;

    void Awake()
    {
        // Rigidbody2D referansını en başta al, daha performanslıdır.
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
{
    // Artık yön verme burada yapılmıyor, Launch() ile dışarıdan yapılıyor.
    Destroy(gameObject, 3f);
}


    public void Launch(Vector2 direction)
{
    if (rb == null)
        rb = GetComponent<Rigidbody2D>();

    rb.linearVelocity = direction.normalized * speed;
}


    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return; // Daha önce çarpmışsa tekrar çalışmasın

        if (!collision.CompareTag("Enemy") && !collision.CompareTag("Animal"))
            return;

        hasHit = true;

        if (collision.CompareTag("Enemy"))
        {
            collision.GetComponent<Enemy>()?.TakeDamage(damage);
        }
        else if (collision.CompareTag("Animal"))
        {
            collision.GetComponent<Animal>()?.TakeDamage(damage);
        }

        Destroy(gameObject);
    }



}