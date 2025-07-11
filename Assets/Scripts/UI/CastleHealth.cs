using UnityEngine;
using UnityEngine.UI;

public class CastleHealth : MonoBehaviour
{
    public int maxHealth = 10;
    private int currentHealth;

    public Slider healthSlider;  // UI'dan bağlanacak

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        UpdateUI();

        if (currentHealth <= 0)
        {
            Debug.Log("🏚️ Kale yıkıldı! Oyun bitti.");
            GameManager.Instance.GameOver();
        }

    }

    void UpdateUI()
    {
        if (healthSlider != null)
            healthSlider.value = (float)currentHealth / maxHealth;
    }
}
