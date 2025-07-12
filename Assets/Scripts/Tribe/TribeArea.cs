using UnityEngine;
using UnityEngine.InputSystem;

public class TribeArea : MonoBehaviour
{
    public GameObject tribeUIPanel;

    private bool isPlayerNearby = false;
    private bool isPanelOpen = false;

    void Update()
    {
        if (isPlayerNearby && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!isPanelOpen)
            {
                tribeUIPanel.SetActive(true);
                isPanelOpen = true;
            }
            else
            {
                tribeUIPanel.SetActive(false);
                isPanelOpen = false;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerNearby = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            isPanelOpen = false;
            tribeUIPanel.SetActive(false);
        }
    }
}
