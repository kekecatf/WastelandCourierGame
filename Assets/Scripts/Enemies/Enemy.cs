using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
    private AudioSource audioSource; // YENİ

    [Header("Audio")]
    [Tooltip("Düşman doğduğunda veya belirli aralıklarla çalınacak sesler.")]
    public List<AudioClip> ambientSounds;
    [Tooltip("Düşman hasar aldığında çalınacak sesler.")]
    public List<AudioClip> hurtSounds;
    [Tooltip("Düşman öldüğünde çalınacak ses.")]
    public AudioClip deathSound; // Genellikle tek bir ölüm sesi olur.

    [Tooltip("Rastgele ortam seslerinin çalınma aralığı (min ve max saniye).")]
    public Vector2 ambientSoundInterval = new Vector2(5f, 15f);




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
        audioSource = GetComponent<AudioSource>();

        StartCoroutine(PlayAmbientSounds());

    }


    void Update()
    {
        if (target == null) return;

        Vector2 direction = (target.position - transform.position).normalized;
        transform.position += (Vector3)(direction * Time.deltaTime * moveSpeed);


        // Can barı pozisyon güncellemesi artık gerekli değil çünkü parent olarak ayarlandı
        // if (hpBarInstance != null)
        //     hpBarInstance.transform.position = transform.position + Vector3.up * 1f;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        PlayRandomSound(hurtSounds);
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
            animator.Play("Die");
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
            Die();
        }
        if (enemyType == EnemyType.Exploder)
        {
            // Patlama efekti (ekleyebilirsin)
            Debug.Log("💥 Patlayan mutant kendini yok etti!");

            if (hpBarInstance != null)
                Destroy(hpBarInstance);

            // Oyuncuya da zarar verebilirsin burada
            Destroy(gameObject);
        }
        if (collision.CompareTag("Player"))
        {
            Debug.Log("😈 Düşman oyuncuya çarptı!");
            // Hasar kodu buraya
        }

    }
    private void Die()
    {
        Debug.Log("Düşman öldü!");
        // Önce bileşenleri devre dışı bırak ki tekrar hasar almasın veya hareket etmesin.
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false; // Bu script'i devre dışı bırakır.

        // YENİ: Ölüm sesini çal.
        if (deathSound != null)
        {
            // Ölüm sesi, obje yok olmadan önce duyulabilsin diye yeni bir obje üzerinde çalınır.
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }

        animator.Play("Die"); // Ölüm animasyonunu oynat

        // Loot düşürme
        if (goldPrefab != null && Random.value < 0.25f) Instantiate(goldPrefab, transform.position, Quaternion.identity);
        if (blueprintPrefabs.Length > 0 && Random.value < 0.75f)
        {
            int index = Random.Range(0, blueprintPrefabs.Length);
            Instantiate(blueprintPrefabs[index], transform.position, Quaternion.identity);
        }
        
        // Obje ve can barını, animasyon bittikten sonra yok et (örneğin 2 saniye sonra).
        if (hpBarInstance != null) Destroy(hpBarInstance, 2f);
        Destroy(gameObject, 2f);
    }
    
    // YENİ: Rastgele ortam sesi çalan Coroutine
    private System.Collections.IEnumerator PlayAmbientSounds()
    {
        while (true) // Sonsuz döngü
        {
            // Rastgele bir süre bekle
            yield return new WaitForSeconds(Random.Range(ambientSoundInterval.x, ambientSoundInterval.y));
            
            // Rastgele bir ortam sesi çal
            PlayRandomSound(ambientSounds);
        }
    }

    // YENİ: Verilen listeden rastgele bir ses çalan yardımcı fonksiyon
    private void PlayRandomSound(List<AudioClip> clips)
    {
        if (clips != null && clips.Count > 0 && audioSource != null)
        {
            int index = Random.Range(0, clips.Count);
            audioSource.PlayOneShot(clips[index]);
        }
    }

}