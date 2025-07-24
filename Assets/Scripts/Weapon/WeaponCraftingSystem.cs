// CraftingSystem.cs (KARAVAN SÝSTEMÝNE UYGUN HALÝ)

using UnityEngine;
using System.Collections.Generic;

public class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem Instance { get; private set; }

    [Header("Crafting Data")]
    public List<WeaponBlueprint> availableBlueprints;

    [Header("UI References")]
    public GameObject craftingPanel; // Ana panel
    public Transform blueprintsContainer; // Tariflerin gösterileceði UI container
    public GameObject blueprintUIPrefab; // Bir tarifi temsil eden UI prefab'ý

    private PlayerStats playerStats;
    private List<BlueprintUI> blueprintUIElements = new List<BlueprintUI>();

    void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
        InitializeCraftingPanel();
        craftingPanel.SetActive(false); // Baþlangýçta paneli gizle
    }

    // --- Panel Kontrol Fonksiyonlarý (CraftingStation tarafýndan çaðrýlacak) ---
    public void OpenCraftingPanel()
    {
        // 4. KONTROL: Bu fonksiyon çaðrýlýyor mu?
        Debug.Log("OpenCraftingPanel() fonksiyonu ÇAÐRILDI.");

        if (craftingPanel != null)
        {
            craftingPanel.SetActive(true);
            // 5. KONTROL: Panel aktif edildi mi?
            Debug.Log("craftingPanel.SetActive(true) komutu çalýþtýrýldý. Panelin sahnede görünmesi lazým.");
            UpdateAllBlueprintUI();
        }
        else
        {
            // EÐER BU HATA GÖRÜNÜRSE, SORUN BUDUR!
            Debug.LogError("HATA: CraftingSystem üzerindeki 'craftingPanel' referansý atanmamýþ (null)!");
        }
    }

    public void CloseCraftingPanel()
    {
        craftingPanel.SetActive(false);
    }

    // --- Kurulum ve UI Güncelleme ---
    private void InitializeCraftingPanel()
    {
        foreach (Transform child in blueprintsContainer) Destroy(child.gameObject);
        blueprintUIElements.Clear();

        foreach (var blueprint in availableBlueprints)
        {
            GameObject uiObject = Instantiate(blueprintUIPrefab, blueprintsContainer);
            BlueprintUI blueprintUI = uiObject.GetComponent<BlueprintUI>();

            // UI elemanýna gerekli bilgileri ve fonksiyonu ata
            blueprintUI.Setup(blueprint, () => TryCraftWeapon(blueprint));

            blueprintUIElements.Add(blueprintUI);
        }
    }

    public void UpdateAllBlueprintUI()
    {
        foreach (var uiElement in blueprintUIElements)
        {
            bool canBeCrafted = CanCraft(uiElement.GetBlueprint());
            uiElement.SetCraftableStatus(canBeCrafted);
        }
    }

    // --- Craft Mantýðý ---
    private bool CanCraft(WeaponBlueprint blueprint)
    {
        // Eðer bu silahýn kilidi zaten açýksa, tekrar craftlanamaz.
        if (WeaponSlotManager.Instance.IsWeaponUnlocked(blueprint.weaponSlotIndexToUnlock))
        {
            return false;
        }

        foreach (var requirement in blueprint.requiredParts)
        {
            if (playerStats.GetWeaponPartCount(requirement.partType) < requirement.amount)
            {
                return false;
            }
        }
        return true;
    }

    public void TryCraftWeapon(WeaponBlueprint blueprint)
    {
        if (CanCraft(blueprint))
        {
            playerStats.ConsumeWeaponParts(blueprint.requiredParts);
            WeaponSlotManager.Instance.UnlockWeapon(blueprint.weaponSlotIndexToUnlock);
            UpdateAllBlueprintUI(); // Craft sonrasý UI'ý tekrar güncelle

            Debug.Log($"Craft BAÞARILI: {blueprint.weaponName} üretildi!");
        }
        else
        {
            Debug.LogWarning("Craft BAÞARISIZ: Yeterli parça yok veya silah zaten açýk.");
        }
    }
}