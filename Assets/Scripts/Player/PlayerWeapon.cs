using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(AudioSource))]
public class PlayerWeapon : MonoBehaviour
{
    // References
    private GameObject currentModel;
    private AudioSource audioSource;
    private Animator animator;

    private WeaponSlotManager slotManager;

    [Header("Weapon Configuration")]
    public WeaponData weaponData;

    [Header("Components")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public LayerMask enemyLayer;

    [Header("Muzzle Flash")]
    public Light2D muzzleFlash;
    public float muzzleFlashDuration = 0.05f;
    public float muzzleFlashIntensity = 3f;
    private Coroutine muzzleFlashCo;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip emptyClipSound;

    // Input System
    private PlayerControls controls;

    // State
    private bool isReloading = false;
    private bool isAiming = false;

    private float lastAutoFireTime = 0f;

    // Reload Hold Detection
    private float reloadPressTime;
    public float reloadHoldThreshold = 0.4f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();

        controls = new PlayerControls();

        if (muzzleFlash != null)
            muzzleFlash.enabled = false;
    }

    private void Start()
    {
        slotManager = WeaponSlotManager.Instance;

        if (slotManager != null)
            LoadFromSlot(slotManager.activeSlotIndex);
        else
            Debug.LogError("[PlayerWeapon] Slot Manager null!");
    }

    // ============================
    // INPUT SYSTEM ONENABLE / OFF
    // ============================
    private void OnEnable()
    {
        controls.Gameplay.Enable();

        // Slot Switching
        controls.Gameplay.Weapon1.performed += ctx => SwitchToSlot(0);
        controls.Gameplay.Weapon2.performed += ctx => SwitchToSlot(1);
        controls.Gameplay.Weapon3.performed += ctx => SwitchToSlot(2);

        // ADS
        controls.Gameplay.ADS.started += ctx => StartADS();
        controls.Gameplay.ADS.canceled += ctx => StopADS();

        // Reload Hold Detection
        controls.Gameplay.Reload.started += ctx =>
        {
            reloadPressTime = Time.time;
        };

        controls.Gameplay.Reload.canceled += ctx =>
        {
            float held = Time.time - reloadPressTime;

            if (held >= reloadHoldThreshold)
                StartCoroutine(MagCheck());
            else
                StartCoroutine(ReloadRoutine());
        };
    }

    private void OnDisable()
    {
        controls.Gameplay.Disable();
    }

    private void SwitchToSlot(int slot)
    {
        WeaponSlotManager.Instance.SwitchSlot(slot);
    }

    // ============================
    // SHOOT (NEW INPUT SYSTEM)
    // ============================

    private void HandleShootStart()
    {

        if (!weaponData.isAutomatic)
            Shoot();
    }


    private void Update()
{
    if (PauseMenu.IsPaused || weaponData == null || isReloading)
        return;

    // 🔫 Tekli atış (semi-auto)
    if (!weaponData.isAutomatic)
    {
        if (Input.GetMouseButtonDown(0))
            Shoot();
    }

    // 🔥 Otomatik atış (auto)
    if (weaponData.isAutomatic)
    {
        if (Input.GetMouseButton(0))
            AutoFire();
    }
}


    private void AutoFire()
    {
        float fireDelay = 1f / weaponData.fireRate;

        if (Time.time - lastAutoFireTime >= fireDelay)
        {
            lastAutoFireTime = Time.time;
            Shoot();
        }
    }

    // ============================
    // ACTUAL SHOOT LOGIC
    // ============================
    public void Shoot()
    {
        if (isReloading) return;

        int slot = slotManager != null ? slotManager.activeSlotIndex : 0;
        var ammo = slotManager != null ? slotManager.GetAmmo(slot) : (clip: 0, reserve: 0);

        if (ammo.clip <= 0)
        {
            PlayEmptyClipSound();
            Debug.Log("Boş! Ateş yok.");
            return;
        }

        slotManager.SetAmmo(slot, ammo.clip - 1, ammo.reserve);

        RangedAttack();
        Debug.Log($"Ateş edildi! Şarjör: {ammo.clip - 1}/{weaponData.clipSize}");
    }

    private void RangedAttack()
    {
        if (shootSound != null)
            audioSource.PlayOneShot(shootSound);

        animator?.SetTrigger("Shoot");

        if (weaponData.isShotgun)
            FireShotgunPellets();
        else
            FireSingleBullet();

        TryMuzzleFlash();
    }

    private void FireSingleBullet()
    {
        var bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        if (bullet.TryGetComponent(out WeaponBullet b))
        {
            b.damage = weaponData.damage;
            b.owner = transform;
            b.weaponType = weaponData.weaponType;
            b.knockbackForce = weaponData.knockbackForce;
            b.knockbackDuration = weaponData.knockbackDuration;
        }
    }

    private void FireShotgunPellets()
    {
        int pellets = Mathf.Max(weaponData.pelletsPerShot, 1);
        float spread = weaponData.pelletSpreadAngle;

        for (int i = 0; i < pellets; i++)
        {
            float t = pellets == 1 ? 0f : (float)i / (pellets - 1);
            float angle = Mathf.Lerp(-spread, spread, t);
            Quaternion rot = firePoint.rotation * Quaternion.Euler(0, 0, angle);

            var bullet = Instantiate(bulletPrefab, firePoint.position, rot);

            if (bullet.TryGetComponent(out WeaponBullet b))
            {
                b.damage = weaponData.damage;
                b.owner = transform;
                b.weaponType = weaponData.weaponType;
                b.knockbackForce = weaponData.knockbackForce;
                b.knockbackDuration = weaponData.knockbackDuration;
            }
        }
    }

    // ============================
    // ADS
    // ============================
    private void StartADS()
    {
        isAiming = true;
        animator?.SetBool("ADS", true);
    }

    private void StopADS()
    {
        isAiming = false;
        animator?.SetBool("ADS", false);
    }

    // ============================
    // RELOAD (SHORT OR LONG HOLD)
    // ============================
    private IEnumerator ReloadRoutine()
    {
        if (isReloading) yield break;

        int slot = slotManager != null ? slotManager.activeSlotIndex : 0;
        var ammo = slotManager != null ? slotManager.GetAmmo(slot) : (clip: 0, reserve: 0);

        int needed = weaponData.clipSize - ammo.clip;
        if (needed <= 0)
            yield break;

        string ammoType = weaponData.ammoType.ToString();
        Inventory inv = Inventory.Instance;

        // Rezervi doldurmak için envanterden çek
        if (inv != null && ammo.reserve < needed)
        {
            int maxReserveSpace = weaponData.maxAmmoCapacity - ammo.reserve;
            int takeFromInv = Mathf.Min(maxReserveSpace, inv.GetAmmoAmount(ammoType));
            if (takeFromInv > 0 && inv.TryUseAmmo(ammoType, takeFromInv))
            {
                ammo.reserve += takeFromInv;
            }
        }

        if (ammo.reserve <= 0)
        {
            Debug.Log("Yedek mermi yok.");
            yield break;
        }

        isReloading = true;

        audioSource.PlayOneShot(reloadSound);
        yield return new WaitForSeconds(weaponData.reloadTime);

        int load = Mathf.Min(needed, ammo.reserve);
        slotManager.SetAmmo(slot, ammo.clip + load, ammo.reserve - load);

        Debug.Log($"Şarjör yenilendi → {ammo.clip + load}/{weaponData.clipSize} | Rezerv: {ammo.reserve - load}");

        isReloading = false;
    }

    // ============================
    // MAGAZINE CHECK (HOLD R)
    // ============================
    IEnumerator MagCheck()
    {
        yield return new WaitForSeconds(0.3f);

        int slot = slotManager != null ? slotManager.activeSlotIndex : 0;
        var ammo = slotManager != null ? slotManager.GetAmmo(slot) : (clip: 0, reserve: 0);
        string ammoType = weaponData != null ? weaponData.ammoType.ToString() : "";
        int stored = Inventory.Instance != null ? Inventory.Instance.GetAmmoAmount(ammoType) : 0;

        Debug.Log($"Mermi Bilgisi → Şarjör: {ammo.clip}/{weaponData.clipSize}, Rezerv: {ammo.reserve}, Envanter: {stored}");
    }

    // ============================
    // MUZZLE FLASH
    // ============================
    private void TryMuzzleFlash()
    {
        if (muzzleFlash == null) return;

        muzzleFlash.transform.position = firePoint.position;

        if (muzzleFlashCo != null)
            StopCoroutine(muzzleFlashCo);

        muzzleFlashCo = StartCoroutine(MuzzleFlashRoutine());
    }

    private IEnumerator MuzzleFlashRoutine()
    {
        float original = muzzleFlash.intensity;
        muzzleFlash.enabled = true;
        muzzleFlash.intensity = muzzleFlashIntensity;

        yield return new WaitForSeconds(muzzleFlashDuration);

        muzzleFlash.intensity = original;
        muzzleFlash.enabled = false;
    }

    public void PlayEmptyClipSound()
    {
        if (emptyClipSound != null)
            audioSource.PlayOneShot(emptyClipSound);
    }

    // ============================
    // SLOT / WEAPON LOAD
    // ============================
    public void LoadFromSlot(int slot)
    {
        var data = slotManager.GetEquippedWeapon(slot);
        if (data == null) return;

        weaponData = data;
        lastAutoFireTime = 0f;
        isReloading = false;

        if (currentModel != null)
            Destroy(currentModel);

        if (weaponData.prefab != null)
        {
            currentModel = Instantiate(weaponData.prefab, transform);
            Transform fp = currentModel.transform.Find("FirePoint");
            if (fp != null)
                firePoint = fp;
        }
    }

    public void SetWeapon(WeaponData data)
    {
        if (data == null)
            return;

        weaponData = data;

        if (currentModel != null)
            Destroy(currentModel);

        if (weaponData.prefab != null)
        {
            currentModel = Instantiate(weaponData.prefab, transform);
            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localRotation = Quaternion.identity;

            Transform fp = currentModel.transform.Find("FirePoint");
            if (fp != null)
                firePoint = fp;
        }

        Debug.Log($"[PlayerWeapon] Yeni silah takıldı → {weaponData.name}.");
    }
}
