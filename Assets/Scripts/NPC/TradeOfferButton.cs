using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections.Generic;

public class TradeOfferButton : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI offerText; // Örn: "30 Wood -> 1 Barrel"
    public Button tradeButton;

    private TradeOffer currentOffer;
    private NPCInteraction npc;

    public void Setup(TradeOffer offer, PlayerStats stats)
    {
        // --- Maliyet metni (yalnızca >0 olanları göster) ---
        var costs = new System.Collections.Generic.List<string>();
        if (offer.requiredWood > 0) costs.Add($"{offer.requiredWood} Wood");
        if (offer.requiredStone > 0) costs.Add($"{offer.requiredStone} Stone");
        if (offer.requiredScrapMetal > 0) costs.Add($"{offer.requiredScrapMetal} ScrapMetal");
        if (offer.requiredMeat > 0) costs.Add($"{offer.requiredMeat} Meat");
        if (offer.requiredDeerHide > 0) costs.Add($"{offer.requiredDeerHide} DeerHide");
        if (offer.requiredRabbitHide > 0) costs.Add($"{offer.requiredRabbitHide} RabbitHide");
        if (offer.requiredHerb > 0) costs.Add($"{offer.requiredHerb} Herb"); // ⬅️ YENİ


        bool canAfford =
        stats.GetResourceAmount("Stone") >= offer.requiredStone &&
        stats.GetResourceAmount("Wood") >= offer.requiredWood &&
        stats.GetResourceAmount("scrapMetal") >= offer.requiredScrapMetal &&
        stats.GetResourceAmount("Meat") >= offer.requiredMeat &&
        stats.GetResourceAmount("DeerHide") >= offer.requiredDeerHide &&
        stats.GetResourceAmount("RabbitHide") >= offer.requiredRabbitHide &&
        stats.GetResourceAmount("Herb") >= offer.requiredHerb; // varsa


        tradeButton.interactable = canAfford;

        tradeButton.onClick.RemoveAllListeners();
        tradeButton.onClick.AddListener(() => npc.ExecuteTrade(offer));

        string costText = costs.Count > 0 ? "İstenen: " + string.Join(", ", costs) : "İstenen: Yok";

        // --- Ödül metni ---
        string rewardText;
        if (offer.rewardKind == RewardKind.WeaponPart)
        {
            rewardText = $"Verilen: {offer.amountToGive} x {offer.partToGive}";
        }
        else
        {
            rewardText = $"Verilen: {offer.resourceAmountToGive} x {offer.resourceToGive}";
        }

        offerText.text = $"{costText}\n{rewardText}";

    }
}
