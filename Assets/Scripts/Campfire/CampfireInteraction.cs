using UnityEngine;
using UnityEngine.InputSystem;

public class CampfireInteraction : MonoBehaviour
{
    private bool isPlayerNearby = false;
    private PlayerStats playerStats;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            playerStats = player.GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (isPlayerNearby && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryCookMeat();
        }
    }

    void TryCookMeat()
    {
        if (playerStats == null) return;

        if (playerStats.GetResourceAmount("Meat") > 0)
        {
            playerStats.RemoveResource("Meat", 1);
            playerStats.AddResource("CookedMeat", 1);
            Debug.Log("🔥 Et pişirildi!");
        }
        else
        {
            Debug.Log("🥩 Etin yok!");
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
            isPlayerNearby = false;
    }
}
