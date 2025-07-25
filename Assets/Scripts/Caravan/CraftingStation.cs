// CraftingStation.cs

using UnityEngine;

public class CraftingStation : MonoBehaviour
{
    public GameObject interactionPrompt; // "E'ye bas" uyar�s� i�in UI objesi
    private bool playerInRange = false;

    private void Start()
    {
        // Ba�lang��ta uyar�y� gizle
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        // E�er oyuncu menzildeyse ve 'E' tu�una basarsa...
        if (playerInRange && Input.GetKeyDown(KeyCode.U))
        {
            // CraftingSystem'e paneli a�mas� i�in komut g�nder.
            CraftingSystem.Instance.OpenCraftingPanel();

            if (CraftingSystem.Instance != null)
            {
                CraftingSystem.Instance.OpenCraftingPanel();
            }
            else
            {
                // E�er bu hata g�r�n�rse, CraftingSystem hala sahnede yok demektir.
            }
        }

    }

    // Oyuncu karavan�n alan�na girdi�inde
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
        }
    }

    // Oyuncu karavan�n alan�ndan ��kt���nda
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
            // Oyuncu uzakla�t���nda craft panelini de kapat.
            CraftingSystem.Instance.CloseCraftingPanel();
        }
    }
}