using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

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

[Header("Targets")]
public Transform player;
public Transform caravan; // Inspector’dan da atanabilir

[Header("Armored Settings")]
public float armoredDamageInterval = 1.0f;
public int armoredCaravanDamage = 1;

[Header("Exploder Settings")]
public float explosionRadius = 2f;
public int explosionDamageToPlayer = 2;
public int explosionDamageToCaravan = 2;
public LayerMask explosionHitMask;

[Header("Contact Damage (Normal/Fast -> Player)")]
public float contactDamageInterval = 1.0f;
public int damageToPlayer = 1;


private bool isDamagingPlayer = false;
private Coroutine caravanDamageCo;
private Coroutine playerDamageCo;

private Vector2 lastMoveDir = Vector2.down; // başlangıç bakış yönü

void Awake() {
    audioSource = GetComponent<AudioSource>();
    animator = GetComponent<Animator>();                // <-- ekle
    if (!animator) Debug.LogError("[Enemy] Animator eksik!");
    if (!audioSource) Debug.LogError("[Enemy] AudioSource eksik!");
}

    public Transform target;

void Start()
{
    // Canı başlat
    currentHealth = maxHealth;
    
    if (hpBarPrefab != null)
    {
        hpBarInstance = Instantiate(hpBarPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
        hpBarInstance.transform.SetParent(transform); // düşmanı takip etsin
        Transform fill = hpBarInstance.transform.Find("Background/Fill");
if (fill != null)
    hpFillImage = fill.GetComponent<Image>();
else
    Debug.LogError("Fill Image bulunamadı! Prefab hiyerarşisini kontrol et.");

        hpFillImage.fillAmount = 1f; // başlangıçta dolu
    }


    // Hedefi türe göre ayarla
        if (enemyType == EnemyType.Normal || enemyType == EnemyType.Fast)
        {
            // Inspector'dan atanmadıysa tag ile bul
            if (player == null)
            {
                var go = GameObject.FindGameObjectWithTag("Player");
                if (go != null) player = go.transform;
            }
            target = player;
        }
        else if (enemyType == EnemyType.Armored)
        {
            if (caravan == null)
            {
                var go = GameObject.FindGameObjectWithTag("Caravan");
                if (go != null) caravan = go.transform;
            }
            target = caravan;
        }
        else if (enemyType == EnemyType.Exploder)
        {
            // İstersen player'ı kovalasın:
            if (player == null)
            {
                var go = GameObject.FindGameObjectWithTag("Player");
                if (go != null) player = go.transform;
            }
            target = player;
        }

    // (Opsiyonel) ortam sesleri için
    if (ambientSounds != null && ambientSounds.Count > 0 && audioSource != null)
        StartCoroutine(PlayAmbientSounds());

    // --- Mevcut Start içeriğin (OverlapCircleAll vb.) devamı ---
    Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.5f);
    foreach (var hit in hits)
    {
        if (hit.CompareTag("Player"))
        {
            if (enemyType == EnemyType.Normal || enemyType == EnemyType.Fast)
                StartPlayerDamage(hit.transform);
        }
        else if (hit.CompareTag("Caravan"))
        {
            if (enemyType == EnemyType.Armored)
                StartCaravanDamage(hit.transform);
            else if (enemyType == EnemyType.Exploder)
                Explode();
        }
    }
}




    void Update()
    {
        if (target == null)
{
    if (enemyType == EnemyType.Normal || enemyType == EnemyType.Fast)
    {
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }
        target = player;
    }
    // Diğer türler için benzer...
}


        Vector2 toTarget = (target.position - transform.position);
        Vector2 dir = toTarget.normalized;

        // HAREKET
        transform.position += (Vector3)(dir * Time.deltaTime * moveSpeed);

        float currentSpeed = toTarget.sqrMagnitude > 0.01f ? dir.magnitude : 0f;
    animator.SetFloat("Speed", currentSpeed);
        // Yürüyüşteyken anlık yönü yaz
        animator.SetFloat("MoveX", dir.x);
        animator.SetFloat("MoveY", dir.y);
    
    if (toTarget.sqrMagnitude > 0.0001f) // veya dir.sqrMagnitude > 0
    {
        lastMoveDir = dir;
        animator.SetFloat("LastMoveX", lastMoveDir.x);
        animator.SetFloat("LastMoveY", lastMoveDir.y);
    }


        // Can barı pozisyon güncellemesi artık gerekli değil çünkü parent olarak ayarlandı
        // if (hpBarInstance != null)
        //     hpBarInstance.transform.position = transform.position + Vector3.up * 1f;
    }

    public void TakeDamage(int amount)
    {

        animator.SetTrigger("Hurt");
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
            animator.SetTrigger("Die");
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

void OnTriggerStay2D(Collider2D collision)
{
    if (collision.CompareTag("Player"))
    {
        if (enemyType == EnemyType.Normal || enemyType == EnemyType.Fast)
        {
            StartPlayerDamage(collision.transform);
        }
    }
    else if (collision.CompareTag("Caravan"))
    {
        if (enemyType == EnemyType.Armored)
        {
            StartCaravanDamage(collision.transform);
        }
        else if (enemyType == EnemyType.Exploder)
        {
            Explode();
        }
    }
}


    void OnTriggerEnter2D(Collider2D collision)
    {
        // Karavan ile temas
        if (collision.CompareTag("Caravan"))
        {
            if (enemyType == EnemyType.Armored)
            {
                // Armored: ölme, karavana periyodik hasar vermeye başla
                StartCaravanDamage(collision.transform);
                return;
            }
            else if (enemyType == EnemyType.Exploder)
            {
                // Exploder: patla ve alan hasarı ver
                Explode();
                return;
            }
            else
            {
                // Normal/Fast karavana ulaşırsa istersen yok et veya görmezden gel
                // Die(); // İSTEMİYORSAN yoruma al
            }
        }

        // Oyuncu ile temas
        if (collision.CompareTag("Player"))
        {
            if (enemyType == EnemyType.Exploder)
            {
                Explode();
                return;
            }
            else if (enemyType == EnemyType.Normal || enemyType == EnemyType.Fast)
            {
                StartPlayerDamage(collision.transform);
            }
        }

    }

void OnTriggerExit2D(Collider2D collision)
{
    if (collision.CompareTag("Caravan") && isDamagingCaravan)
    {
        StopCaravanDamage();
    }
    if (collision.CompareTag("Player") && isDamagingPlayer)
    {
        StopPlayerDamage();
    }
}

private void StartCaravanDamage(Transform caravanTransform)
{
    if (!isDamagingCaravan)
        caravanDamageCo = StartCoroutine(DamageCaravanOverTime(caravanTransform));
}

private void StopCaravanDamage()
{
    isDamagingCaravan = false;
    if (caravanDamageCo != null) StopCoroutine(caravanDamageCo);
    caravanDamageCo = null;
}

    private System.Collections.IEnumerator DamageCaravanOverTime(Transform caravanTransform)
{
    isDamagingCaravan = true;
    var ch = caravanTransform.GetComponent<CaravanHealth>();

    while (isDamagingCaravan && ch != null)
    {
        ch.TakeDamage(armoredCaravanDamage);
        yield return new WaitForSeconds(armoredDamageInterval);
    }

    isDamagingCaravan = false;
    caravanDamageCo = null;
}

private void StartPlayerDamage(Transform playerTransform)
{
    if (!isDamagingPlayer)
        playerDamageCo = StartCoroutine(DamagePlayerOverTime(playerTransform));
}

private void StopPlayerDamage()
{
    isDamagingPlayer = false;
    if (playerDamageCo != null) StopCoroutine(playerDamageCo);
    playerDamageCo = null;
}

private System.Collections.IEnumerator DamagePlayerOverTime(Transform playerTransform)
{
    isDamagingPlayer = true;

    var ps = playerTransform.GetComponent<PlayerStats>();
    while (isDamagingPlayer && ps != null &&
           Vector2.Distance(transform.position, playerTransform.position) < 1.1f)
    {
        // Not: Projende PlayerStats.TakeDamage(int) zaten kullanılıyor
        ps.TakeDamage(damageToPlayer);
        yield return new WaitForSeconds(contactDamageInterval);
    }

    isDamagingPlayer = false;
    playerDamageCo = null;
}


    private bool isDamagingCaravan = false;




private void Explode()
{
    // Patlama efekti/animasyonu oynatmak istiyorsan burada tetikle
    Debug.Log("💥 Exploder patladı!");

    // Alan taraması (LayerMask kullanıyorsan):
    // var hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, explosionHitMask);

    // Tag filtreleyerek yapmak istersen:
    var hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

    foreach (var hit in hits)
{
    if (hit == null) continue;

    if (hit.CompareTag("Player"))
    {
        var ps = hit.GetComponent<PlayerStats>();
        if (ps != null) ps.TakeDamage(explosionDamageToPlayer);
    }
    else if (hit.CompareTag("Caravan"))
    {
        var ch = hit.GetComponent<CaravanHealth>();
        if (ch != null) ch.TakeDamage(explosionDamageToCaravan);
    }
}


    // Ses/loot/animasyon vs:
    if (hpBarInstance != null) Destroy(hpBarInstance);
    if (deathSound != null) AudioSource.PlayClipAtPoint(deathSound, transform.position);

    // İstersen kısa gecikmeyle yok et ki animasyon/ses bitsin
    Destroy(gameObject);
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