using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Linq;

public class WeaponCraftingSystem : MonoBehaviour
{
    public static WeaponCraftingSystem Instance { get; private set; }

    private int TypeKey(WeaponBlueprint bp) => (bp != null) ? bp.weaponSlotIndexToUnlock : -1;

    [Header("Crafting Data")]
    public List<WeaponBlueprint> availableBlueprints;

    [Header("UI References")]
    public GameObject craftingPanel;
    public Transform requirementsContainer;
    public GameObject requirementLinePrefab;
    public TextMeshProUGUI craftPromptText;
    public Button craftButton;
    public Button swapButton;

    private readonly List<BlueprintUI> blueprintUIElements = new List<BlueprintUI>();
    private PlayerStats playerStats;
    private WeaponBlueprint selectedBlueprint;

    public static bool IsCraftingOpen =>
        Instance != null && Instance.craftingPanel != null && Instance.craftingPanel.activeSelf;

    void Awake()
    {
        if (Instance == null) Instance = this; else { Destroy(gameObject); return; }
    }

    void Start()
    {

        for (int i = 0; i < blueprintUIElements.Count; i++)
        {
            var ui = blueprintUIElements[i];
            Debug.Log($"[CraftUI] Card {i} -> {ui.blueprint?.weaponName} (slotIndex={ui.blueprint?.weaponSlotIndexToUnlock})");
        }


        playerStats = FindObjectOfType<PlayerStats>();
        if (craftingPanel != null) craftingPanel.SetActive(false);
        ClearDetailInfo();

        WireGlobalButtons();
        InitializeBlueprintList();
        UpdateAllBlueprintUI();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame && CraftingStation.IsPlayerInRange)
            ToggleCraftingPanel();
    }

    private void WireGlobalButtons()
    {
        if (craftButton != null)
        {
            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(TryCraftWeapon);
            craftButton.interactable = false;
        }
        if (swapButton != null)
        {
            swapButton.onClick.RemoveAllListeners();
            swapButton.onClick.AddListener(TrySwapSelected);
            swapButton.interactable = false;
        }
    }

    private void InitializeBlueprintList()
{
    blueprintUIElements.Clear();

    // 1) UI kartlarını al ve pozisyona göre (soldan sağa, yukarıdan aşağıya) sırala
    var uis = craftingPanel.GetComponentsInChildren<BlueprintUI>(true)
                           .OrderBy(ui => {
                               var rt = ui.transform as RectTransform;
                               // önce satır (y küçük -> üst), sonra sütun (x küçük -> sol)
                               return (rt ? (rt.anchoredPosition.y * -10000f + rt.anchoredPosition.x) : 0f);
                           })
                           .ToList();
    blueprintUIElements.AddRange(uis);

    // 2) Blueprint’leri slot indexine göre sırala (0:Machinegun, 1:Pistol, 2:Sniper, 3:Shotgun ...)
    var bps = (availableBlueprints ?? new List<WeaponBlueprint>())
                  .Where(bp => bp != null)
                  .OrderBy(bp => bp.weaponSlotIndexToUnlock)
                  .ToList();

    // 3) Eşleştir ve kur
    int count = Mathf.Min(bps.Count, blueprintUIElements.Count);
    for (int i = 0; i < count; i++)
        blueprintUIElements[i].Setup(bps[i]);

    // 4) İlk açılışta depo sayaçlarını doğru yaz
    UpdateAllBlueprintUI();
}


    public void ToggleCraftingPanel()
    {
        bool isActive = !craftingPanel.activeSelf;
        craftingPanel.SetActive(isActive);

        if (isActive)
        {
            Time.timeScale = 0f;
            UpdateAllBlueprintUI();
            selectedBlueprint = null;
            ClearDetailInfo();
            SetGlobalButtons(false, false);
        }
        else
        {
            Time.timeScale = 1f;
            selectedBlueprint = null;
            ClearDetailInfo();
            StartCoroutine(ReapplyAmmoNextFrame());
        }
    }

    public void SelectBlueprint(WeaponBlueprint blueprint)
    {
        selectedBlueprint = blueprint;
        UpdateDetailPanel();
    }

    private void UpdateDetailPanel()
    {
        foreach (Transform c in requirementsContainer) Destroy(c.gameObject);

        if (selectedBlueprint == null)
        {
            SetGlobalButtons(false, false);
            return;
        }

        // Gereksinimleri yaz
        foreach (var req in selectedBlueprint.requiredParts)
        {
            int have = playerStats.GetWeaponPartCount(req.partType);
            AddRequirementLine(req.partType.ToString(), have, req.amount);
        }

        if (craftPromptText) craftPromptText.gameObject.SetActive(false);

        // --- Yeni mantık ---
        bool canCraftNow = CanCraft(selectedBlueprint);

        int selectedKey = TypeKey(selectedBlueprint);
        int storedCount = CaravanInventory.Instance.GetStoredCountForType(selectedKey);

        // Swap ancak:
        // 1) Oyuncu karavan menzilinde
        // 2) Aktif slot seçili tür ile aynı
        // 3) Depoda bu türden en az 1 kopya var
        bool inRange = CraftingStation.IsPlayerInRange;
        bool rightSlot = (WeaponSlotManager.Instance != null &&
                          WeaponSlotManager.Instance.activeSlotIndex == selectedKey);
        bool canSwapNow = inRange && rightSlot && (storedCount > 0);

        SetGlobalButtons(canCraftNow, canSwapNow);

        Debug.Log($"[DETAIL] selectedKey={selectedKey}  active={WeaponSlotManager.Instance?.activeSlotIndex}  inRange={CraftingStation.IsPlayerInRange}  stored={storedCount}  canCraftParts={canCraftNow}  canSwap={canSwapNow}");

    }



    private void SetGlobalButtons(bool craftInteractable, bool swapInteractable)
    {
        if (craftButton != null) craftButton.interactable = craftInteractable;
        if (swapButton != null) swapButton.interactable = swapInteractable;
    }

    private void ClearDetailInfo()
    {
        foreach (Transform child in requirementsContainer) Destroy(child.gameObject);
        if (craftPromptText != null) craftPromptText.gameObject.SetActive(false);
    }

    private void AddRequirementLine(string itemName, int current, int required)
    {
        if (requirementLinePrefab == null || required <= 0) return;
        var go = Instantiate(requirementLinePrefab, requirementsContainer);
        var ui = go.GetComponent<RequirementLineUI>();
        ui.Setup(itemName, current, required);
    }

    public bool CanCraft(WeaponBlueprint bp)
    {
        if (bp == null || playerStats == null) return false;
        foreach (var r in bp.requiredParts)
            if (playerStats.GetWeaponPartCount(r.partType) < r.amount)
                return false;
        return true;
    }


    private void ShowMsg(string text)
    {
        if (craftPromptText == null) return;
        craftPromptText.text = text;
        craftPromptText.gameObject.SetActive(true);
    }

    // === CRAFT ===
    public void TryCraftWeapon()
    {
        if (selectedBlueprint == null) { ShowMsg("Önce bir silah seç."); return; }
        foreach (var r in selectedBlueprint.requiredParts)
            if (playerStats.GetWeaponPartCount(r.partType) < r.amount)
            { ShowMsg("Eksik parçalar var"); return; }

        playerStats.ConsumeWeaponParts(selectedBlueprint.requiredParts);
        bool stored = CaravanInventory.Instance.StoreCraftResult(selectedBlueprint);
        ShowMsg(stored ? "Yeni silah üretildi (depo: +1)" : "Depoya eklenemedi.");

        UpdateDetailPanel();
        UpdateAllBlueprintUI();
    }

    // === SWAP ===
    public void TrySwapSelected()
    {
        if (!CraftingStation.IsPlayerInRange) { ShowMsg("Karavan menzilinde değilsin."); return; }
        if (selectedBlueprint == null) { ShowMsg("Önce bir silah seç."); return; }

        var wsm = WeaponSlotManager.Instance;
        int activeSlot = wsm.activeSlotIndex;
        int selectedKey = TypeKey(selectedBlueprint);

        if (selectedKey != activeSlot) { ShowMsg("Önce bu türün slotuna geç."); return; }

        int count = CaravanInventory.Instance.GetStoredCountForType(selectedKey);
        if (count <= 0) { ShowMsg("Depoda bu türden silah yok (önce craft et)."); return; }

        CaravanInventory.Instance.SwapNextStoredForActiveType();

        UpdateAllBlueprintUI();
        StartCoroutine(ReapplyAmmoNextFrame());

        var bpNow = wsm.GetBlueprintForSlot(activeSlot);
        ShowMsg($"{bpNow?.weaponName ?? "Silah"} kuşanıldı. (depo:{CaravanInventory.Instance.GetStoredCountForType(selectedKey)} kaldı)");


        var name = CaravanInventory.Instance.GetActiveInstanceNameForSlot(wsm.activeSlotIndex);
        ShowMsg($"{(string.IsNullOrEmpty(name) ? wsm.GetBlueprintForSlot(wsm.activeSlotIndex)?.weaponName : name)} kuşanıldı.");

    }

    // Tüm blueprint kartlarının "depo adedi" vb. durumlarını tazeler
    public void UpdateAllBlueprintUI()
    {
        foreach (var ui in blueprintUIElements)
            if (ui != null) ui.UpdateStatus();
    }

    private IEnumerator ReapplyAmmoNextFrame()
    {
        yield return null;
        WeaponSlotManager.Instance?.ForceReapplyActiveAmmo();
    }

    public void CloseCraftingPanel()
    {
        if (craftingPanel != null && craftingPanel.activeSelf)
        {
            craftingPanel.SetActive(false);
            Time.timeScale = 1f;

            selectedBlueprint = null;
            ClearDetailInfo();
            SetGlobalButtons(false, false);

            // panel kapandıktan sonra aktif silahın mermisini 1 frame sonra tekrar bastır
            StartCoroutine(ReapplyAmmoNextFrame());
        }
    }

}
