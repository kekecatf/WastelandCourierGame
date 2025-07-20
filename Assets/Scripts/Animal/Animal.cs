using UnityEngine;

public class Animal : MonoBehaviour
{
    public string animalType = "Geyik"; // Inspector'dan atanır
    public int maxHealth = 3;
    public float moveSpeed = 2f;

    public GameObject meatPrefab;
    public GameObject hidePrefab; // türüne özel

    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        // Basit rastgele gezinme örneği (geliştirilebilir)
        transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            DropLoot();
            Destroy(gameObject);
        }
    }

    void DropLoot()
    {
        Instantiate(meatPrefab, transform.position, Quaternion.identity);
        Instantiate(hidePrefab, transform.position + Vector3.right * 0.5f, Quaternion.identity);
    }
}
