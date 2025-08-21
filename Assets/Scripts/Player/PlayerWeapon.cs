using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(AudioSource))]
public class PlayerWeapon : MonoBehaviour
{
    // --- INSPECTOR'DA ATANACAK ALANLAR ---
    [Header("Weapon Configuration")]
    public WeaponData weaponData;

    [Header("Weapon Components")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public LayerMask enemyLayer;

    [Header("Muzzle Flash")]
    [Tooltip("Namlu ucunda anlık parlayan Light2D.")]
    public Light2D muzzleFlash;
    [Tooltip("Parlama süresi (saniye).")]
    public float muzzleFlashDuration = 0.05f;
    [Tooltip("Parlama sırasında kullanılacak yoğunluk.")]
    public float muzzleFlashIntensity = 3f;

    private Coroutine muzzleFlashCo;

    [Header("Audio Clips")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip emptyClipSound;

    private Animator animator;
    private AudioSource audioSource;
    private int currentAmmoInClip;
    private float nextTimeToFire = 0f;
    private bool isReloading = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();

        if (muzzleFlash != null) muzzleFlash.enabled = false;
    }

    private void OnEnable()
    {
        isReloading = false;
        if (muzzleFlash != null) muzzleFlash.enabled = false;
    }
    
    private void OnDisable()
    {
        if (muzzleFlash != null) muzzleFlash.enabled = false;
    }

    public void Shoot()
    {
        if (PauseMenu.IsPaused || isReloading || Time.time < nextTimeToFire)
            return;
        if (isReloading || Time.time < nextTimeToFire) return;

        if (weaponData.clipSize <= 0) // Melee check
        {
            MeleeAttack();
        }
        else // Ranged
        {
            RangedAttack();
        }
    }

    private void RangedAttack()
{
    if (currentAmmoInClip <= 0) return;

    nextTimeToFire = Time.time + 1f / weaponData.fireRate;
    currentAmmoInClip--;

    if (shootSound != null)
    {
        audioSource.PlayOneShot(shootSound);
        SoundEmitter.EmitSound(transform.position, 7f);
    }

    if (animator != null) animator.SetTrigger("Shoot");

    if (bulletPrefab != null && firePoint != null)
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        var bulletScript = bullet.GetComponent<WeaponBullet>();
        if (bulletScript != null) bulletScript.damage = weaponData.damage;

        // >>> Muzzle flash
        TryMuzzleFlash();
    }

    WeaponSlotManager.Instance.UpdateAmmoText();
}



    private void MeleeAttack()
    {
        nextTimeToFire = Time.time + 1f / weaponData.fireRate;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(firePoint.position, weaponData.attackRange, enemyLayer);
        foreach(Collider2D enemyCollider in hitEnemies)
        {
            // Düşmana hasar verme mantığı...
        }
    }
    
    public void PlayEmptyClipSound()
    {
        if (emptyClipSound != null && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(emptyClipSound);
        }
    }
    // Şarjör değiştirme Coroutine'i
    public IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log($"Reloading {weaponData.weaponName}...");

        if (reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }

        yield return new WaitForSeconds(weaponData.reloadTime);

        WeaponSlotManager.Instance.FinishReload();

        isReloading = false;
        
        Debug.Log("Reload finished.");
    }

    private void TryMuzzleFlash()
{
    if (muzzleFlash == null || firePoint == null) return;

    // MuzzleLight'ı tam namlu ucuna taşı (emin olmak için)
    muzzleFlash.transform.position = firePoint.position;

    // Aynı anda birden fazla coroutine çalışmasın
    if (muzzleFlashCo != null) StopCoroutine(muzzleFlashCo);
    muzzleFlashCo = StartCoroutine(MuzzleFlashRoutine());
}

private IEnumerator MuzzleFlashRoutine()
{
    // Yoğunluk değişken olabilir; isterseniz bir "pop" efekti gibi kısa yükselip düşürtebilirsiniz.
    float originalIntensity = muzzleFlash.intensity;

    muzzleFlash.intensity = muzzleFlashIntensity;
    muzzleFlash.enabled = true;

    yield return new WaitForSeconds(muzzleFlashDuration);

    muzzleFlash.enabled = false;
    muzzleFlash.intensity = originalIntensity;
    muzzleFlashCo = null;
}

    // Yardımcı Fonksiyonlar
    public void SetAmmoInClip(int amount) => currentAmmoInClip = amount;
    public int GetCurrentAmmoInClip() => currentAmmoInClip;
    public bool IsReloading() => isReloading;
}