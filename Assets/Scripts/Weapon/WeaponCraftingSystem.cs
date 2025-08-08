// WeaponCraftingSystem.cs (TEMİZLENMİŞ VE TAM HALİ)

using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class WeaponCraftingSystem : MonoBehaviour
{
    public static WeaponCraftingSystem Instance { get; private set; }

    [Header("Crafting Data")]
    public List<WeaponBlueprint> availableBlueprints;

    [Header("UI References")]
    public GameObject craftingPanel;
    public Transform requirementsContainer;
    public GameObject requirementLinePrefab;
    public TextMeshProUGUI craftPromptText;
    private List<BlueprintUI> blueprintUIElements = new List<BlueprintUI>();
    private PlayerStats playerStats;
    private WeaponBlueprint selectedBlueprint;
    public static bool IsCraftingOpen =>
    Instance != null && Instance.craftingPanel != null && Instance.craftingPanel.activeSelf;



    void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
        if (playerStats == null) Debug.LogError("Sahnede PlayerStats bulunamadı!");

        InitializeBlueprintList();
        if (craftingPanel != null) craftingPanel.SetActive(false);
        ClearDetailInfo();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame && CraftingStation.IsPlayerInRange)
        {
            ToggleCraftingPanel();
        }

        if (craftingPanel.activeSelf && selectedBlueprint != null && CanCraft(selectedBlueprint))
        {
            if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
            {
                TryCraftWeapon();
            }
        }
    }


    private void InitializeBlueprintList()
    {

    }

    public void ToggleCraftingPanel()
    {
        bool isActive = !craftingPanel.activeSelf;
        craftingPanel.SetActive(isActive);

        if (isActive)
        {
            Time.timeScale = 0f; // oyun dursun
            UpdateAllBlueprintUI();
        }
        else
        {
            Time.timeScale = 1f; // oyun devam etsin
            selectedBlueprint = null;
            ClearDetailInfo();
        }
    }


    public void SelectBlueprint(WeaponBlueprint blueprint)
    {
        Debug.Log($"<color=yellow>SEÇİLDİ:</color> {blueprint.weaponName} tarifi inceleniyor.");
        selectedBlueprint = blueprint;
        UpdateDetailPanel();
    }


    public void UpdateAllBlueprintUI()
    {

        foreach (var uiElement in blueprintUIElements)
        {
            uiElement.UpdateStatus();
        }
    }

    private void UpdateDetailPanel()
    {
        if (selectedBlueprint == null)
        {
            ClearDetailInfo();
            return;
        }

        foreach (Transform child in requirementsContainer) Destroy(child.gameObject);
        Debug.Log($"{selectedBlueprint.weaponName} için {selectedBlueprint.requiredParts.Count} adet parça gereksinimi var.");
        foreach (var partReq in selectedBlueprint.requiredParts)
        {
            int currentAmount = playerStats.GetWeaponPartCount(partReq.partType);
            AddRequirementLine(partReq.partType.ToString(), currentAmount, partReq.amount);
        }

        if (craftPromptText != null)
        {
            craftPromptText.gameObject.SetActive(CanCraft(selectedBlueprint));
        }
    }

    private void ClearDetailInfo()
    {
        foreach (Transform child in requirementsContainer) Destroy(child.gameObject);
        if (craftPromptText != null) craftPromptText.gameObject.SetActive(false);
    }

    private void AddRequirementLine(string itemName, int current, int required)
    {
        if (required > 0 && requirementLinePrefab != null)
        {
            GameObject lineObject = Instantiate(requirementLinePrefab, requirementsContainer);
            lineObject.GetComponent<RequirementLineUI>().Setup(itemName, current, required);
        }
    }

    public bool CanCraft(WeaponBlueprint blueprint)
    {
        if (blueprint == null || playerStats == null) return false;

        if (CaravanInventory.Instance != null && CaravanInventory.Instance.IsWeaponStored(blueprint))
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

    public void TryCraftWeapon()
    {
        if (CanCraft(selectedBlueprint))
        {
            playerStats.ConsumeWeaponParts(selectedBlueprint.requiredParts);

            CaravanInventory.Instance.StoreWeapon(selectedBlueprint);

            UpdateDetailPanel();
            UpdateAllBlueprintUI();

            Debug.Log($"Craft BAŞARILI: {selectedBlueprint.weaponName} üretildi ve depoya gönderildi!");
        }
    }
    public void CloseCraftingPanel()
    {
        if (craftingPanel != null && craftingPanel.activeSelf)
        {
            craftingPanel.SetActive(false);
            selectedBlueprint = null;
            ClearDetailInfo();
        }
    }

}