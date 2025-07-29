using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteraction : MonoBehaviour
{
    public GameObject tradeUIPanel;
    public GameObject interactPromptUI;

    private bool isPlayerNearby = false;
    private bool isPanelOpen = false;

    void Update()
    {
        if (isPlayerNearby && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!isPanelOpen)
                OpenTrade();
            else
                CloseTrade();
        }

        // Eğer panel açıksa fare mutlaka açık olmalı
        if (isPanelOpen)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            interactPromptUI?.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        isPlayerNearby = false;

        // interactPromptUI sahnede yoksa hata verir
        if (interactPromptUI != null)
            interactPromptUI.SetActive(false);

        // tradeUIPanel destroy edilmiş olabilir, kontrol şart
        if (tradeUIPanel != null)
            CloseTrade();
    }
}


    void OpenTrade()
    {
        tradeUIPanel?.SetActive(true);
        isPanelOpen = true;
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void CloseTrade()
{
    // Obje Destroy edilmişse artık kullanılamaz, önce null kontrolü yap
    if (tradeUIPanel != null)
    {
        tradeUIPanel.SetActive(false);
    }

    isPanelOpen = false;
    Time.timeScale = 1f;

    Cursor.visible = false;
    Cursor.lockState = CursorLockMode.None;
}

}
