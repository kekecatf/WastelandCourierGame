using UnityEngine;

public class TribeArea : MonoBehaviour
{
    public GameObject tribeUIPanel; // Atanacak

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (tribeUIPanel != null)
                tribeUIPanel.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (tribeUIPanel != null)
                tribeUIPanel.SetActive(false);
        }
    }
}
