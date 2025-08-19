using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public class NPCInteraction : MonoBehaviour
{
    [Header("Trade Data")]
    public List<TradeOffer> tradeOffers;

    [Header("UI Refs")]
    public GameObject tradeUIPanel;
    public GameObject interactPromptUI;         // "E - Trade" gibi
    public Transform tradeOffersContainer;      // Teklif butonlarının parent'ı
    public GameObject tradeOfferButtonPrefab;   // Üzerinde TradeOfferButton olan prefab

    [Header("Auto Load (optional)")]
    public bool autoLoadFromResources = true;
    public string resourcesFolder = "Animals"; // Resources/NPC

    private bool isPlayerNearby = false;
    private bool isPanelOpen = false;
    private PlayerStats playerStats;

    public static NPCInteraction Instance { get; private set; }
    public static bool IsTradeOpen =>
        Instance != null && Instance.tradeUIPanel != null && Instance.tradeUIPanel.activeSelf;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
        if (tradeUIPanel != null) tradeUIPanel.SetActive(false);
        if (interactPromptUI != null) interactPromptUI.SetActive(false);

        if (autoLoadFromResources)
        {
            var loaded = Resources.LoadAll<TradeOffer>(resourcesFolder);
            if (loaded != null && loaded.Length > 0)
            {
                // mevcut listeye ekle (çiftleri önle)
                var set = new HashSet<TradeOffer>(tradeOffers);
                foreach (var t in loaded)
                    if (!set.Contains(t)) tradeOffers.Add(t);
            }
        }
    }

    private void OnDisable()
    {
        // Panel açıkken bu bileşen disable olursa oyun hızını normale al.
        if (IsTradeOpen) Time.timeScale = 1f;
    }

    private void Update()
    {
        // Yakındaysa E ile aç/kapat
        if (isPlayerNearby && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleTradePanel();
        }

        // Panel açıksa ESC ile kapat
        if (IsTradeOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseTradePanel();
        }
    }

    public void CloseTradePanel()
    {
        if (tradeUIPanel == null) return;

        tradeUIPanel.SetActive(false);
        isPanelOpen = false;
        Time.timeScale = 1f;

        // Yakındaysa prompt’u tekrar gösterebilirsin
        if (isPlayerNearby && interactPromptUI != null)
            interactPromptUI.SetActive(true);
    }

    private void ToggleTradePanel()
    {
        if (tradeUIPanel == null) return;

        bool shouldOpen = !tradeUIPanel.activeSelf;
        tradeUIPanel.SetActive(shouldOpen);
        isPanelOpen = shouldOpen;

        if (shouldOpen)
        {
            PopulateTradeOffers();
            Time.timeScale = 0f; // Oyunu durdur
            // Panel açılınca prompt’u gizle
            if (interactPromptUI != null) interactPromptUI.SetActive(false);
        }
        else
        {
            Time.timeScale = 1f; // Oyunu devam ettir
            // Panel kapanınca yakındaysan prompt’u göster
            if (isPlayerNearby && interactPromptUI != null)
                interactPromptUI.SetActive(true);
        }
    }

    private void PopulateTradeOffers()
    {
        if (tradeOffersContainer == null || tradeOfferButtonPrefab == null) return;

        // Eski butonları temizle
        for (int i = tradeOffersContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(tradeOffersContainer.GetChild(i).gameObject);
        }

        // Yeni butonları oluştur
        foreach (TradeOffer offer in tradeOffers)
        {
            GameObject buttonObject = Instantiate(tradeOfferButtonPrefab, tradeOffersContainer);

            buttonObject.GetComponent<TradeOfferButton>().Setup(offer, playerStats);
        }
    }

    // --- Kaynak kontrol & düşüm yardımcıları ---

    private bool HasEnoughResources(TradeOffer offer)
    {
        if (playerStats == null || offer == null) return false;

        return playerStats.GetResourceAmount("Stone") >= offer.requiredStone
            && playerStats.GetResourceAmount("Wood") >= offer.requiredWood
            && playerStats.GetResourceAmount("scrapMetal") >= offer.requiredScrapMetal
            && playerStats.GetResourceAmount("Meat") >= offer.requiredMeat
            && playerStats.GetResourceAmount("DeerHide") >= offer.requiredDeerHide
            && playerStats.GetResourceAmount("RabbitHide") >= offer.requiredRabbitHide
            && playerStats.GetResourceAmount("Herb") >= offer.requiredHerb;
    }

    private void DeductResources(TradeOffer offer)
    {
        if (playerStats == null || offer == null) return;

        if (offer.requiredStone > 0) playerStats.RemoveResource("Stone", offer.requiredStone);
        if (offer.requiredWood > 0) playerStats.RemoveResource("Wood", offer.requiredWood);
        if (offer.requiredScrapMetal > 0) playerStats.RemoveResource("scrapMetal", offer.requiredScrapMetal);
        if (offer.requiredMeat > 0) playerStats.RemoveResource("Meat", offer.requiredMeat);
        if (offer.requiredDeerHide > 0) playerStats.RemoveResource("DeerHide", offer.requiredDeerHide);
        if (offer.requiredRabbitHide > 0) playerStats.RemoveResource("RabbitHide", offer.requiredRabbitHide);
        if (offer.requiredHerb > 0) playerStats.RemoveResource("Herb", offer.requiredHerb); // ⬅️ YENİ
    }

    // --- Takas işlemi ---

    public void ExecuteTrade(TradeOffer offer)
    {
        if (offer == null) return;

        if (HasEnoughResources(offer))
        {
            DeductResources(offer);

            if (playerStats != null)
            {
                playerStats.CollectWeaponPart(offer.partToGive, offer.amountToGive);
            }

            Debug.Log($"Takas başarılı! {offer.amountToGive} adet {offer.partToGive} alındı.");

            // UI'ı tazele (buton interaktifliği vs. güncellensin)
            PopulateTradeOffers();
        }
        else
        {
            Debug.Log("Takas başarısız! Yeterli materyal yok.");
        }

        if (offer.rewardKind == RewardKind.WeaponPart)
        {
            if (playerStats != null)
                playerStats.CollectWeaponPart(offer.partToGive, offer.amountToGive);

            Debug.Log($"Takas başarılı! {offer.amountToGive} x {offer.partToGive} alındı.");
        }
        else // RewardKind.Resource
        {
            if (playerStats != null && offer.resourceAmountToGive > 0)
            {
                // PlayerStats AddResource isimleriyle birebir uyumlu olmalı:
                switch (offer.resourceToGive)
                {
                    case ResourceType.Meat:
                        playerStats.AddResource("Meat", offer.resourceAmountToGive);
                        break;
                    case ResourceType.DeerHide:
                        playerStats.AddResource("DeerHide", offer.resourceAmountToGive);
                        break;
                    case ResourceType.RabbitHide:
                        playerStats.AddResource("RabbitHide", offer.resourceAmountToGive);
                        break;
                    case ResourceType.Stone:
                        playerStats.AddResource("Stone", offer.resourceAmountToGive);
                        break;
                    case ResourceType.Wood:
                        playerStats.AddResource("Wood", offer.resourceAmountToGive);
                        break;
                    case ResourceType.scrapMetal:
                        playerStats.AddResource("scrapMetal", offer.resourceAmountToGive);
                        break;
                    case ResourceType.Arrow:
                        playerStats.AddResource("Arrow", offer.resourceAmountToGive);
                        break;
                    case ResourceType.Spear:
                        playerStats.AddResource("Spear", offer.resourceAmountToGive);
                        break;
                    default:
                        Debug.LogWarning($"Desteklenmeyen resource ödülü: {offer.resourceToGive}");
                        break;

                    case ResourceType.Herb:
                        playerStats.AddResource("Herb", offer.resourceAmountToGive);
                        break;

                }
                Debug.Log($"Takas başarılı! {offer.resourceAmountToGive} x {offer.resourceToGive} alındı.");
            }
        }

    }

    // --- Trigger alanı ---

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerNearby = true;

        // Panel kapalıysa prompt’u göster
        if (!IsTradeOpen && interactPromptUI != null)
            interactPromptUI.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerNearby = false;

        // Alandan çıkınca prompt’u gizle
        if (interactPromptUI != null)
            interactPromptUI.SetActive(false);

        // İsteğe bağlı: Alandan çıkınca paneli kapat
        // CloseTradePanel();
    }

    [ContextMenu("Reload Trade Offers")]
    private void ReloadTradeOffersInEditor()
    {
        var loaded = Resources.LoadAll<TradeOffer>(resourcesFolder);
        tradeOffers = loaded.ToList();
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}
