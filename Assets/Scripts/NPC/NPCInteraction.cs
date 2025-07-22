using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteraction : MonoBehaviour
{
    public GameObject tradeUIPanel;

    private bool isPlayerNearby = false;
    private bool isPanelOpen = false;
    public GameObject interactPromptUI; // "E" yazan UI objesi


    void Update()
    {
        if (isPlayerNearby && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!isPanelOpen)
                OpenTrade();
            else
                CloseTrade();
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
            CloseTrade();
            if (interactPromptUI != null)
                interactPromptUI.SetActive(false);
        }
    }

    void OpenTrade()
    {
        if (tradeUIPanel != null)
        {
            tradeUIPanel.SetActive(true);
            isPanelOpen = true;
            Time.timeScale = 0f;
            Cursor.visible = true;
        }
    }

    void CloseTrade()
    {
        if (tradeUIPanel != null)
        {
            tradeUIPanel.SetActive(false);
            isPanelOpen = false;
            Time.timeScale = 1f;
            Cursor.visible = false;
        }
    }
}
