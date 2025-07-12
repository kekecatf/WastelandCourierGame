using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 5;
    private int currentHealth;

    public GameObject hpBarPrefab; // Prefab atanacak
    private Image hpFillImage; // STATIC KALDIRILDI
    private GameObject hpBarInstance;
    public GameObject goldPrefab; // Inspector'dan atanacak
    public GameObject[] blueprintPrefabs; // Farklı blueprint objeleri atanabilir



    public Transform target;

    void Start()
    {
        currentHealth = maxHealth;

        if (hpBarPrefab != null)
        {
            hpBarInstance = Instantiate(hpBarPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
            hpBarInstance.transform.SetParent(transform, true); // FALSE -> TRUE

            Transform fill = hpBarInstance.transform.Find("Background/Fill");
            if (fill != null)
            {
                hpFillImage = fill.GetComponent<Image>();
            }
        }
    }

    void Update()
    {
        if (target == null) return;

        Vector2 direction = (target.position - transform.position).normalized;
        transform.position += (Vector3)(direction * Time.deltaTime * 2f);

        // Can barı pozisyon güncellemesi artık gerekli değil çünkü parent olarak ayarlandı
        // if (hpBarInstance != null)
        //     hpBarInstance.transform.position = transform.position + Vector3.up * 1f;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        // Debug için
        Debug.Log($"Enemy took {amount} damage. Current health: {currentHealth}/{maxHealth}");

        if (hpFillImage != null)
        {
            float fillValue = Mathf.Clamp01((float)currentHealth / maxHealth);
            hpFillImage.fillAmount = fillValue;
            Debug.Log($"Fill amount set to: {fillValue}");
        }

        if (currentHealth <= 0)
        {
            Debug.Log("Enemy should die now!");

            // ALTIN DÜŞÜRME %25 ŞANS
            if (goldPrefab != null && Random.value < 0.25f)
            {
                Instantiate(goldPrefab, transform.position, Quaternion.identity);
                Debug.Log("💰 Düşman altın bıraktı!");
            }

            if (blueprintPrefabs.Length > 0 && Random.value < 0.75f)
            {
                int index = Random.Range(0, blueprintPrefabs.Length);
                Instantiate(blueprintPrefabs[index], transform.position, Quaternion.identity);
                Debug.Log("📘 Düşman blueprint düşürdü!");
            }


            if (hpBarInstance != null)
                Destroy(hpBarInstance);
            Destroy(gameObject);
        }

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Caravan"))
        {
            CaravanHealth caravan = collision.GetComponent<CaravanHealth>();
            if (caravan != null)
                caravan.TakeDamage(1);

            if (hpBarInstance != null)
                Destroy(hpBarInstance);
            Destroy(gameObject);
        }
    }
}