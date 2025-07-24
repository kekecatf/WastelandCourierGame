// CraftingStation.cs

using UnityEngine;

public class CraftingStation : MonoBehaviour
{
    public GameObject interactionPrompt; // "E'ye bas" uyarýsý için UI objesi
    private bool playerInRange = false;

    private void Start()
    {
        // Baþlangýçta uyarýyý gizle
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        // Eðer oyuncu menzildeyse ve 'E' tuþuna basarsa...
        if (playerInRange && Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("U tuþuna basýldý ve oyuncu menzilde. CraftingSystem'e komut gönderiliyor...");
            // CraftingSystem'e paneli açmasý için komut gönder.
            CraftingSystem.Instance.OpenCraftingPanel();

            if (CraftingSystem.Instance != null)
            {
                CraftingSystem.Instance.OpenCraftingPanel();
            }
            else
            {
                // Eðer bu hata görünürse, CraftingSystem hala sahnede yok demektir.
                Debug.LogError("CraftingSystem.Instance bulunamadý!");
            }
        }

    }

    // Oyuncu karavanýn alanýna girdiðinde
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Oyuncu karavan menziline girdi. playerInRange = true");
            playerInRange = true;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
        }
    }

    // Oyuncu karavanýn alanýndan çýktýðýnda
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Oyuncu karavan menzilinden çýktý. playerInRange = false");
            playerInRange = false;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
            // Oyuncu uzaklaþtýðýnda craft panelini de kapat.
            CraftingSystem.Instance.CloseCraftingPanel();
        }
    }
}