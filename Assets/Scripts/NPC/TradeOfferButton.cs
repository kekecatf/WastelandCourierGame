using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TradeOfferButton : MonoBehaviour
{
    [Header("UI Prefabs")]
    [SerializeField] private GameObject offerTextPrefab;   // Senin Text prefab'in
    [SerializeField] private Button tradeButton;           // Buton
    [SerializeField] private Image iconImage;              // İkonu gösterecek Image (boşsa butonun Image'ı kullanılır)

    [Header("Layout")]
    [SerializeField] private float gap = 10f;

    [Header("Sprites: Weapon Parts")]
    [SerializeField] private List<PartSpriteEntry> partSprites = new();
    [Header("Sprites: Resources")]
    [SerializeField] private List<ResourceSpriteEntry> resourceSprites = new();
    [SerializeField] private Sprite fallbackSprite; // eşleşme yoksa

    private Dictionary<WeaponPartType, Sprite> partMap;
    private Dictionary<ResourceType, Sprite> resourceMap;

    private TradeOffer currentOffer;
    private NPCInteraction npc;
    private PlayerStats stats;
    private TextMeshProUGUI offerTextInstance;

    [System.Serializable]
    public struct PartSpriteEntry
    {
        public WeaponPartType part;
        public Sprite sprite;
    }

    [System.Serializable]
    public struct ResourceSpriteEntry
    {
        public ResourceType resource;
        public Sprite sprite;
    }

    void Awake()
    {
        // List → Dictionary
        partMap = new Dictionary<WeaponPartType, Sprite>();
        foreach (var e in partSprites)
            if (e.sprite && !partMap.ContainsKey(e.part)) partMap.Add(e.part, e.sprite);

        resourceMap = new Dictionary<ResourceType, Sprite>();
        foreach (var e in resourceSprites)
            if (e.sprite && !resourceMap.ContainsKey(e.resource)) resourceMap.Add(e.resource, e.sprite);

        if (!iconImage && tradeButton) iconImage = tradeButton.GetComponent<Image>();
    }

    public void Setup(TradeOffer offer, PlayerStats statsRef)
    {
        currentOffer = offer;
        stats = statsRef;
        npc = FindObjectOfType<NPCInteraction>();
        if (!tradeButton || currentOffer == null) return;

        // Teklif metni yoksa oluştur ve butonun sağına hizala
        if (!offerTextInstance && offerTextPrefab)
        {
            var txtObj = Instantiate(offerTextPrefab, transform);
            offerTextInstance = txtObj.GetComponent<TextMeshProUGUI>();

            var btnRT = tradeButton.GetComponent<RectTransform>();
            var txtRT = txtObj.GetComponent<RectTransform>();
            txtRT.anchorMin = new Vector2(0, 0.5f);
            txtRT.anchorMax = new Vector2(0, 0.5f);
            txtRT.pivot    = new Vector2(0, 0.5f);
            txtRT.anchoredPosition = new Vector2(btnRT.sizeDelta.x + gap, 0f);
        }

        // İkon
        ApplyIconForOffer(currentOffer);

        // --- METİN: İstenenler + Verilenler ---
        if (offerTextInstance)
        {
            // İstenenler (maliyetler)
            string costText =
                $"İstenen: {offer.requiredWood} Odun, {offer.requiredStone} Taş, {offer.requiredScrapMetal} Metal";

            // Ek maliyetler varsa ekle
            if (offer.requiredMeat > 0)        costText += $", {offer.requiredMeat} Et";
            if (offer.requiredDeerHide > 0)    costText += $", {offer.requiredDeerHide} Geyik Derisi";
            if (offer.requiredRabbitHide > 0)  costText += $", {offer.requiredRabbitHide} Tavşan Derisi";
            if (offer.requiredHerb > 0)        costText += $", {offer.requiredHerb} Şifalı Ot";

            // Verilen (ödül)
            string rewardText = currentOffer.rewardKind == RewardKind.WeaponPart
                ? $"Verilen: {offer.amountToGive} x {offer.partToGive}"
                : $"Verilen: {offer.resourceAmountToGive} x {offer.resourceToGive}"; // <-- DÜZELTME

            offerTextInstance.text = $"{costText}\n{rewardText}";
        }

        // Buton aktifliği
        bool canAfford = stats &&
            stats.GetResourceAmount("Stone")      >= offer.requiredStone &&
            stats.GetResourceAmount("Wood")       >= offer.requiredWood &&
            stats.GetResourceAmount("scrapMetal") >= offer.requiredScrapMetal &&
            stats.GetResourceAmount("Meat")       >= offer.requiredMeat &&
            stats.GetResourceAmount("DeerHide")   >= offer.requiredDeerHide &&
            stats.GetResourceAmount("RabbitHide") >= offer.requiredRabbitHide &&
            stats.GetResourceAmount("Herb")       >= offer.requiredHerb;

        tradeButton.interactable = canAfford;
        tradeButton.onClick.RemoveAllListeners();
        tradeButton.onClick.AddListener(OnTradeClicked);
    }

    private void ApplyIconForOffer(TradeOffer offer)
{
    if (!iconImage) return;

    Sprite s = null;

    if (offer.rewardKind == RewardKind.WeaponPart)
    {
        partMap?.TryGetValue(offer.partToGive, out s);
    }
    else if (offer.rewardKind == RewardKind.Resource)
    {
        resourceMap?.TryGetValue(offer.resourceToGive, out s);
    }

    // Eğer hiç eşleşme yoksa fallback sprite kullan
    iconImage.sprite = s ? s : fallbackSprite;
    iconImage.enabled = (iconImage.sprite != null);

    if (iconImage.enabled)
    {
        iconImage.preserveAspect = true;
    }
}


    private void OnTradeClicked()
    {
        var inst = NPCInteraction.Instance;
        if (inst && inst.tradeScrollRect)
        {
            var sr = inst.tradeScrollRect;
            sr.StopMovement();
            sr.velocity = Vector2.zero;
            sr.verticalNormalizedPosition = 1f;
        }

        if (!npc || currentOffer == null) return;
        npc.ExecuteTrade(currentOffer);
    }
}
