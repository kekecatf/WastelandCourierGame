using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class NPCInteraction : MonoBehaviour
{
    public List<TradeOffer> tradeOffers;
    public GameObject tradeUIPanel;

    private bool isPlayerNearby = false;
    private bool isPanelOpen = false;
    public GameObject interactPromptUI;
    public Transform tradeOffersContainer;
    public GameObject tradeOfferButtonPrefab;
    private PlayerStats playerStats;
    public static NPCInteraction Instance { get; private set; }


    private void Awake()
    {
        Instance = this;
    }

    public static bool IsTradeOpen =>
        Instance != null && Instance.tradeUIPanel != null && Instance.tradeUIPanel.activeSelf;
    void Start()
    {
        // PlayerStats referansını oyun başında bir kere alalım, daha verimli.
        playerStats = FindObjectOfType<PlayerStats>();
        if (tradeUIPanel != null) tradeUIPanel.SetActive(false);
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
    }

        public void CloseTradePanel()
    {
        if (tradeUIPanel != null)
        {
            tradeUIPanel.SetActive(false);
            isPanelOpen = false;
            Time.timeScale = 1f;
        }
    }

    void Update()
    {
        if (isPlayerNearby && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleTradePanel();
        }
    }
    private void ToggleTradePanel()
    {
        bool shouldOpen = !tradeUIPanel.activeSelf;
        tradeUIPanel.SetActive(shouldOpen);

        if (shouldOpen)
        {
            // Panel açıldığında, teklifleri UI'da oluştur.
            PopulateTradeOffers();
            Time.timeScale = 0f; // Oyunu durdur
        }
        else
        {
            Time.timeScale = 1f; // Oyunu devam ettir
        }
    }

    private void PopulateTradeOffers()
    {

        foreach (Transform child in tradeOffersContainer)
        {
            Destroy(child.gameObject);
        }


        foreach (TradeOffer offer in tradeOffers)
        {
            GameObject buttonObject = Instantiate(tradeOfferButtonPrefab, tradeOffersContainer);

            buttonObject.GetComponent<TradeOfferButton>().Setup(offer, playerStats);
        }
    }

    public void ExecuteTrade(TradeOffer offer)
    {

        if (playerStats.GetResourceAmount("Stone") >= offer.requiredStone &&
            playerStats.GetResourceAmount("Wood") >= offer.requiredWood &&
            playerStats.GetResourceAmount("scrapMetal") >= offer.requiredScrapMetal)
        {
            // 2. Materyalleri oyuncunun envanterinden düş.
            playerStats.RemoveResource("Stone", offer.requiredStone);
            playerStats.RemoveResource("Wood", offer.requiredWood);
            playerStats.RemoveResource("scrapMetal", offer.requiredScrapMetal);

            playerStats.CollectWeaponPart(offer.partToGive, offer.amountToGive);

            Debug.Log($"Takas başarılı! {offer.amountToGive} adet {offer.partToGive} alındı.");

            PopulateTradeOffers();
        }
        else
        {
            Debug.Log("Takas başarısız! Yeterli materyal yok.");

        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (interactPromptUI != null)
                interactPromptUI.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (interactPromptUI != null)
                interactPromptUI.SetActive(false);
        }
    }

    

}
