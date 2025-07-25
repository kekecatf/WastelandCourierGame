// PlayerWeapon.cs (EKSİKLERİ GİDERİLMİŞ VE TAM HALİ)

using System.Collections;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    // --- INSPECTOR'DA ATANACAK ALANLAR ---
    [Header("Weapon Configuration")]
    public WeaponData weaponData;

    [Header("Weapon Components")]
    [Tooltip("Merminin oluşturulacağı nokta.")]
    public Transform firePoint;

    [Tooltip("Bu silahın ateşleyeceği mermi prefab'ı.")]
    public GameObject bulletPrefab;

    [Header("Melee Components")]
    [Tooltip("Düşmanların bulunduğu katmanı seçin.")]
    public LayerMask enemyLayer; // Düşmanları filtrelemek için


    // --- SİSTEM DEĞİŞKENLERİ ---
    private int currentAmmoInClip;
    private float nextTimeToFire = 0f;
    private bool isReloading = false;

    private void OnEnable()
    {
        isReloading = false;
    }

    private void Update()
    {
        // Şarjör değiştiriyorsa başka bir işlem yapmasın.
        if (isReloading) return;
    }

    public void Shoot()
    {
        // 1. KONTROL: Kılıç gibi mermisi olmayan veya ateş edemeyen bir silah mı?
        // clipSize 0 ise ve fireRate 0 ise, ateş edemez.
        if (weaponData.clipSize <= 0 && weaponData.fireRate <= 0)
        {
            // Yakın dövüş saldırı mantığı buraya eklenebilir.
            // Örneğin: MeleeAttack();
            return;
        }
        if (weaponData.clipSize <= 0)
        {
            MeleeAttack();
        }
        else // Değilse, bu bir menzilli silahtır.
        {
            RangedAttack();
        }
    }

    private void RangedAttack()
    {
        if (isReloading || currentAmmoInClip <= 0)
        {
            if (currentAmmoInClip <= 0 && !isReloading)
            {
                WeaponSlotManager.Instance.StartReload();
            }
            return;
        }

        if (isReloading || Time.time < nextTimeToFire || currentAmmoInClip <= 0)
        {
            if (currentAmmoInClip <= 0 && !isReloading)
            {
                WeaponSlotManager.Instance.StartReload();
            }
            return;
        }

        nextTimeToFire = Time.time + 1f / weaponData.fireRate;
        currentAmmoInClip--;

        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bulletObject = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            WeaponBullet bulletScript = bulletObject.GetComponent<WeaponBullet>();
            if (bulletScript != null)
            {
                bulletScript.damage = weaponData.damage;
                bulletScript.Launch(firePoint.right);
            }

        }
        else
        {
            if (weaponData.clipSize > 0)
            {
                Debug.LogWarning($"Ateş etmeye çalışıldı ama '{gameObject.name}' silahının 'bulletPrefab' veya 'firePoint' referansı eksik!");
            }
        }

        WeaponSlotManager.Instance.UpdateAmmoText();
    }

    private void MeleeAttack()
    {
        // Saldırı hızını ayarla
        nextTimeToFire = Time.time + 1f / weaponData.fireRate;

        // 1. Animasyonu tetikle (varsa)


        // 2. Saldırı alanındaki düşmanları tespit et
        // firePoint'in merkez olduğu, attackRange yarıçaplı bir daire çiz ve içindeki düşmanları bul.
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(firePoint.position, weaponData.attackRange, enemyLayer);

        // 3. Tespit edilen her düşmana hasar ver
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            Debug.Log($"Vurulan Düşman: {enemyCollider.name}");
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(weaponData.damage);
            }
        }
        Debug.Log("Saldırı Yapıldı");
    }

    // Şarjör değiştirme Coroutine'i
    public IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log($"Reloading {weaponData.weaponName}...");

        yield return new WaitForSeconds(weaponData.reloadTime);

        WeaponSlotManager.Instance.FinishReload();

        isReloading = false;
        Debug.Log("Reload finished.");
    }

    // Yardımcı Fonksiyonlar
    public void SetAmmoInClip(int amount) => currentAmmoInClip = amount;
    public int GetCurrentAmmoInClip() => currentAmmoInClip;
    public bool IsReloading() => isReloading;
}