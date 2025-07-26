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
    public EnemyType enemyType = EnemyType.Normal;
    public float moveSpeed = 2f;
    public int damageToCaravan = 1;
    private Animator animator;
    private Transform player;
    public float playerDetectRange = 4f;





    public Transform target;

    void Start()
    {
        // 1. Tür bazlı maxHealth ve hareket ayarları
        switch (enemyType)
        {
            case EnemyType.Armored:
                maxHealth *= 3;
                moveSpeed = 1f;
                break;

            case EnemyType.Exploder:
                maxHealth = 1;
                moveSpeed = 2.5f;
                damageToCaravan = 3;
                break;

            case EnemyType.Fast:
                moveSpeed = 4f;
                maxHealth = Mathf.RoundToInt(maxHealth * 0.7f);
                break;

            default: // Normal
                moveSpeed = 2f;
                break;
        }

        // 2. Şimdi currentHealth doğru maxHealth'e göre ayarlanmalı
        currentHealth = maxHealth;

        // 3. Can barını oluştur
        if (hpBarPrefab != null)
        {
            hpBarInstance = Instantiate(hpBarPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
            hpBarInstance.transform.SetParent(transform, true);

            Transform fill = hpBarInstance.transform.Find("Background/Fill");
            if (fill != null)
            {
                hpFillImage = fill.GetComponent<Image>();
            }
        }

        animator = GetComponent<Animator>();

        player = GameObject.FindWithTag("Player")?.transform;

    }


    void Update()
    {
        if (player == null || target == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);
        Transform currentTarget = (distToPlayer <= playerDetectRange) ? player : target;

        Vector2 direction = (currentTarget.position - transform.position).normalized;
        transform.position += (Vector3)(direction * Time.deltaTime * moveSpeed);



        // Can barı pozisyon güncellemesi artık gerekli değil çünkü parent olarak ayarlandı
        // if (hpBarInstance != null)
        //     hpBarInstance.transform.position = transform.position + Vector3.up * 1f;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (hpFillImage != null)
        {
            float fillValue = Mathf.Clamp01((float)currentHealth / maxHealth);
            hpFillImage.fillAmount = fillValue;
        }

        if (currentHealth <= 0)
        {
            animator.Play("Die");

            // ALTIN DÜŞÜRME %25 ŞANS
            if (goldPrefab != null && Random.value < 0.25f)
            {
                Instantiate(goldPrefab, transform.position, Quaternion.identity);
            }

            if (blueprintPrefabs.Length > 0 && Random.value < 0.75f)
            {
                int index = Random.Range(0, blueprintPrefabs.Length);
                Instantiate(blueprintPrefabs[index], transform.position, Quaternion.identity);
            }


            if (hpBarInstance != null)
                Destroy(hpBarInstance);
            Destroy(gameObject);
            PlayerStats playerStats = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.AddXP(10); // İstersen düşmana özel bir değer verebilirsin
            }

        }

    }

    void OnTriggerEnter2D(Collider2D collision)
{
    if (enemyType == EnemyType.Exploder)
    {
        // Patlama hasarı uygula
        if (collision.CompareTag("Player"))
        {
            PlayerStats stats = collision.GetComponent<PlayerStats>();
            if (stats != null)
                stats.TakeDamage(damageToCaravan);
        }

        if (collision.CompareTag("Caravan"))
        {
            CaravanHealth caravan = collision.GetComponent<CaravanHealth>();
            if (caravan != null)
                caravan.TakeDamage(damageToCaravan);
        }

        animator?.Play("Die");
        Destroy(hpBarInstance);
        Destroy(gameObject);
    }
}

}