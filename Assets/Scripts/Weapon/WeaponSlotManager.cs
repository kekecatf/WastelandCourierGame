// WeaponSlotManager.cs (YENİ, SAĞLAM VE TEMİZ HALİ)

using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class WeaponSlotManager : MonoBehaviour
{
    // --- INSPECTOR'DA AYARLANACAK ALANLAR ---
    [Header("Weapon Objects")]
    [Tooltip("Sahnedeki silah GameObject'lerini buraya sırayla atayın (0: Makineli, 1: Tabanca, 2: Kılıç).")]
    public GameObject[] weaponSlots;

    [Header("UI Elements")]
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI reloadPromptText;

    private bool[] unlockedWeapons;

    private WeaponBlueprint[] equippedBlueprints;

    [Header("Starting Equipment")]
    public List<WeaponBlueprint> startingEquippedWeapons;


    // --- SİSTEM DEĞİŞKENLERİ ---
    public static WeaponSlotManager Instance { get; private set; }
    private PlayerWeapon activeWeapon;
    private int activeSlotIndex = -1; // -1, başlangıçta hiçbir silahın aktif olmadığını belirtir.

    private bool emptyClipSoundPlayedThisPress = false;

    // Mermi Yönetimi
    private int[] ammoInClips;
    private int[] totalReserveAmmo;

    // WeaponSlotManager.cs içindeki Awake fonksiyonu

    void Awake()
    {
        // Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // --- YENİ MANTIK ---
        // Kilit dizisini oluştur.
        unlockedWeapons = new bool[weaponSlots.Length];

        equippedBlueprints = new WeaponBlueprint[weaponSlots.Length];

        foreach (var blueprint in startingEquippedWeapons)
        {
            if (blueprint != null)
            {
                EquipBlueprint(blueprint);
            }
        }

        // Başlangıçta, weaponSlots dizisine atanmış olan TÜM silahların kilidini AÇIK yap.
        // Bu, oyuna eklediğiniz ilk 3 silahın direkt kullanılabilir olmasını sağlar.
        for (int i = 0; i < unlockedWeapons.Length; i++)
        {
            // Eğer o slotta bir silah objesi varsa, kilidini açık olarak başlat.
            if (weaponSlots[i] != null)
            {
                unlockedWeapons[i] = true;
                Debug.Log($"Başlangıç silahı '{weaponSlots[i].name}' (Slot {i}) kilidi açık.");
            }
            else
            {
                // Eğer slot boş bırakıldıysa, gelecekteki bir silah için kilitli başlasın.
                unlockedWeapons[i] = false;
            }
        }

        InitializeAmmo();
    }

    public WeaponBlueprint GetBlueprintForSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < equippedBlueprints.Length)
        {
            return equippedBlueprints[slotIndex];
        }
        return null;
    }

    public void EquipBlueprint(WeaponBlueprint blueprintToEquip)
    {
        if (blueprintToEquip == null) return;
        
        int slotIndex = blueprintToEquip.weaponSlotIndexToUnlock;
        if (slotIndex >= 0 && slotIndex < equippedBlueprints.Length)
        {
            equippedBlueprints[slotIndex] = blueprintToEquip;
            Debug.Log($"{blueprintToEquip.weaponName}, Slot {slotIndex}'e kuşandırıldı.");

            // Eğer o an o slot aktifse, değişikliği anında yansıt.
            if (activeSlotIndex == slotIndex)
            {
                SwitchToSlot(slotIndex);
            }
        }
    }



    public bool IsWeaponEquipped(WeaponBlueprint blueprint)
    {
        int slotIndex = blueprint.weaponSlotIndexToUnlock;
        return equippedBlueprints[slotIndex] == blueprint;
    }

    public void UnlockWeapon(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < unlockedWeapons.Length)
        {
            if (!unlockedWeapons[slotIndex])
            {
                unlockedWeapons[slotIndex] = true;
                Debug.Log($"<color=lime>SİLAH KİLİDİ AÇILDI:</color> {weaponSlots[slotIndex].name} artık kullanılabilir!");

                // İsteğe bağlı: Kilidi açılan silaha otomatik geçiş yap
                SwitchToSlot(slotIndex);
            }
        }
        else
        {
            Debug.LogError($"Geçersiz bir silah slot index'i ({slotIndex}) kilidi açılmaya çalışıldı!");
        }
    }
    // WeaponSlotManager.cs'in içine

    public bool IsWeaponUnlocked(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < unlockedWeapons.Length)
        {
            return unlockedWeapons[slotIndex];
        }
        return false;
    }

    void Start()
    {
        // Başlangıçta tüm silahların kapalı olduğundan emin olalım.
        foreach (var slot in weaponSlots)
        {
            if (slot != null) slot.SetActive(false);
        }

        // Başlangıçta 1. slottaki (Handgun) silahı seç.
        SwitchToSlot(1);
    }

    void Update()
    {
        // Tuş girdilerini dinle
        HandleWeaponSwitchingInput();

        // Aktif bir silah yoksa, sonraki işlemleri yapma.
        if (activeWeapon == null) return;

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            emptyClipSoundPlayedThisPress = false;
        }

        HandleShootingInput();
        HandleReloadInput();
        UpdateUI();
    }

    private void InitializeAmmo()
    {
        // Dizileri silah sayısı kadar oluştur.
        ammoInClips = new int[weaponSlots.Length];
        totalReserveAmmo = new int[weaponSlots.Length];

        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] == null)
            {
                Debug.LogError($"Weapon Slot {i} boş! Lütfen Inspector'dan atama yapın.");
                continue;
            }

            // ÖNEMLİ: Prefab deaktif olsa bile GetComponent çalışır.
            PlayerWeapon weapon = weaponSlots[i].GetComponent<PlayerWeapon>();
            if (weapon == null || weapon.weaponData == null)
            {
                Debug.LogError($"Weapon Slot {i} ({weaponSlots[i].name}) üzerinde PlayerWeapon script'i veya WeaponData bulunamadı!");
                continue;
            }

            // Kılıç gibi mermisi olmayan silahları atla
            if (weapon.weaponData.clipSize > 0)
            {
                ammoInClips[i] = weapon.weaponData.clipSize;
                totalReserveAmmo[i] = weapon.weaponData.maxAmmoCapacity;
            }
            else
            {
                ammoInClips[i] = 0;
                totalReserveAmmo[i] = 0;
            }
        }
        Debug.Log("Mermi sistemi başlangıç değerleri yüklendi.");
    }

    private void HandleWeaponSwitchingInput()
{
    if (Keyboard.current == null) return;

    if (Keyboard.current.digit1Key.wasPressedThisFrame) SwitchToSlot(0);
    if (Keyboard.current.digit2Key.wasPressedThisFrame) SwitchToSlot(1);
    if (Keyboard.current.digit3Key.wasPressedThisFrame) SwitchToSlot(2);
}

    public void SwitchToSlot(int newIndex)
    {
        // 1. Gerekli kontroller
        if (newIndex < 0 || newIndex >= weaponSlots.Length)
        {
            Debug.LogError($"Geçersiz silah slotu indexi: {newIndex}");
            return;
        }
        if (newIndex == activeSlotIndex) return; // Zaten o silah seçili.
        if (weaponSlots[newIndex] == null)
        {
            Debug.LogError($"Slot {newIndex} için silah atanmamış!");
            return;
        }

        // 2. Mevcut silahı deaktif et ve durumunu kaydet
        if (activeWeapon != null)
        {
            if (activeWeapon.IsReloading())
            {
                activeWeapon.StopAllCoroutines();
            }
            // Mermili bir silahsı mermisini kaydet
            if (activeWeapon.weaponData.clipSize > 0)
            {
                ammoInClips[activeSlotIndex] = activeWeapon.GetCurrentAmmoInClip();
            }
            weaponSlots[activeSlotIndex].SetActive(false);
        }

        // 3. Yeni silahı aktif et ve referansları ayarla
        activeSlotIndex = newIndex;
        GameObject newWeaponObject = weaponSlots[activeSlotIndex];
        newWeaponObject.SetActive(true);
        activeWeapon = newWeaponObject.GetComponent<PlayerWeapon>();

        if (activeWeapon == null)
        {
            Debug.LogError($"Yeni aktif edilen silah ({newWeaponObject.name}) üzerinde PlayerWeapon script'i yok!");
            return;
        }

        // 4. Yeni silahın mermi durumunu yükle
        if (activeWeapon.weaponData.clipSize > 0)
        {
            activeWeapon.SetAmmoInClip(ammoInClips[activeSlotIndex]);
        }

        Debug.Log($"Başarıyla '{activeWeapon.weaponData.weaponName}' silahına geçildi.");
    }

    // --- Ateş Etme, Şarjör Değiştirme ve UI Fonksiyonları ---
    // Bu kısımlar önceki versiyonlarla büyük ölçüde aynı kalabilir.

    private void HandleShootingInput()
{
    if (Mouse.current == null) return;

        if (activeWeapon.weaponData.isAutomatic)
        {
            if (Mouse.current.leftButton.isPressed)
                
            
            if (activeWeapon.GetCurrentAmmoInClip() > 0)
                {
                    // Mermi varsa, bayrağı sıfırla ve ateş et.
                    emptyClipSoundPlayedThisPress = false;
                    activeWeapon.Shoot();
                }
                else // Mermi yoksa...
                {
                    // "Tık" sesi bu basış sırasında daha önce çalınmadıysa...
                    if (!emptyClipSoundPlayedThisPress)
                    {
                        // ...sesi çal ve bayrağı setle.
                        activeWeapon.PlayEmptyClipSound();
                        emptyClipSoundPlayedThisPress = true;
                        
                        // Otomatik reload'u da sadece bir kez dene.
                        StartReload();
                    }
                }
    }
        else
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
                if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (activeWeapon.GetCurrentAmmoInClip() > 0)
                {
                    activeWeapon.Shoot();
                }
                else
                {
                    activeWeapon.PlayEmptyClipSound();
                    StartReload();
                }
            }
        }
}


    private void HandleReloadInput()
{
    if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
    {
        StartReload();
    }
}

    public void StartReload()
    {
        // Kılıç gibi silahların şarjörü değiştirilemez.
        if (activeWeapon.weaponData.clipSize <= 0) return;

        if (!activeWeapon.IsReloading() && totalReserveAmmo[activeSlotIndex] > 0 && activeWeapon.GetCurrentAmmoInClip() < activeWeapon.weaponData.clipSize)
        {
            activeWeapon.StartCoroutine(activeWeapon.Reload());
        }
    }

    public void FinishReload()
    {
        if (activeWeapon == null || activeWeapon.weaponData.clipSize <= 0) return;

        int clipSize = activeWeapon.weaponData.clipSize;
        int currentAmmo = activeWeapon.GetCurrentAmmoInClip();
        int ammoNeeded = clipSize - currentAmmo;

        int ammoToTransfer = Mathf.Min(ammoNeeded, totalReserveAmmo[activeSlotIndex]);

        activeWeapon.SetAmmoInClip(currentAmmo + ammoToTransfer);
        totalReserveAmmo[activeSlotIndex] -= ammoToTransfer;
    }

    private void UpdateUI()
    {
        UpdateAmmoText();
        UpdateReloadPrompt();
    }

    public void UpdateAmmoText()
    {
        if (activeWeapon != null && ammoText != null)
        {
            // Mermisi olmayan silahlar (kılıç gibi) için UI'ı gizle
            if (activeWeapon.weaponData.clipSize <= 0)
            {
                ammoText.text = "";
            }
            else
            {
                ammoText.text = $"{activeWeapon.GetCurrentAmmoInClip()} / {totalReserveAmmo[activeSlotIndex]}";
            }
        }
    }

   private void UpdateReloadPrompt()
{
    // Aktif silah, UI text ve PlayerWeapon script'i var mı?
    if (activeWeapon == null || reloadPromptText == null) return;

    // 1. Durum: Şarjör değiştiriliyor mu?
    if (activeWeapon.IsReloading())
    {
        reloadPromptText.text = "Reloading...";
        reloadPromptText.gameObject.SetActive(true);
    }
    // 2. Durum: Mermi bitti ve şarjör değiştirilebilir mi?
    else if (activeWeapon.GetCurrentAmmoInClip() <= 0 && totalReserveAmmo[activeSlotIndex] > 0)
    {
        reloadPromptText.text = "Press 'R' to Reload";
        reloadPromptText.gameObject.SetActive(true);
    }
    // 3. Durum: Diğer tüm durumlar
    else
    {
        reloadPromptText.gameObject.SetActive(false);
    }
}
}