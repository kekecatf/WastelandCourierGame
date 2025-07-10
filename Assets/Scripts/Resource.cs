using UnityEngine;

public class Resource : MonoBehaviour
{
    public ResourceType type;  // Taş, odun, meteorit...
    public int amount = 1;

    public void Collect()
    {
        FindObjectOfType<InventoryUI>().AddResource(type, amount);
        Destroy(gameObject);
    }
}
