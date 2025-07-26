using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public Image fillImage;

    public void SetHealth(float current, float max)
    {
        float amount = Mathf.Clamp01(current / max);
        fillImage.fillAmount = amount;
    }
}
