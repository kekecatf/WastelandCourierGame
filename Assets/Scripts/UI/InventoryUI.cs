using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public TextMeshProUGUI stoneText;
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI meteoriteText;
    public TextMeshProUGUI capacityText;

    private PlayerStats stats;

    void Start()
    {
        stats = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (stats == null) return;

        stoneText.text = "Stone: " + stats.GetResourceAmount("Stone");
        woodText.text = "Wood: " + stats.GetResourceAmount("Wood");
        meteoriteText.text = "Meteorite: " + stats.GetResourceAmount("Meteorite");

        capacityText.text = $"Capacity: {stats.GetTotalResourceAmount()}/{stats.inventoryCapacity}";
    }
}
