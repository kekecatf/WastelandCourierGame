using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public TextMeshProUGUI stoneText;
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI meteoriteText;

    private int stoneCount = 0;
    private int woodCount = 0;
    private int meteoriteCount = 0;

    // Eğer PlayerInventory hâlâ kullanılıyorsa burası kalabilir, yoksa silebiliriz.
    void Update()
    {
        stoneText.text = "Stone: " + PlayerInventory.Instance.GetAmount("Stone");
        woodText.text = "Wood: " + PlayerInventory.Instance.GetAmount("Wood");
        meteoriteText.text = "Meteorite: " + PlayerInventory.Instance.GetAmount("Meteorite");
    }


    public void AddResource(ResourceType type, int amount)
    {
        switch (type)
        {
            case ResourceType.Stone:
                stoneCount += amount;
                break;
            case ResourceType.Wood:
                woodCount += amount;
                break;
            case ResourceType.Meteorite:
                meteoriteCount += amount;
                break;
        }
    }
}
