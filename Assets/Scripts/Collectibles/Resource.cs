using UnityEngine;

public class Resource : MonoBehaviour
{
    public ResourceType type;
    public int amount = 1;

    public void Collect()
{
    var stats = GameObject.FindWithTag("Player")?.GetComponent<PlayerStats>();
    if (stats == null) return;

    switch (type)
    {
        case ResourceType.Stone: stats.AddResource("Stone", amount); break;
        case ResourceType.Wood: stats.AddResource("Wood", amount); break;
        case ResourceType.scrapMetal: stats.AddResource("scrapMetal", amount); break;
    }

    Destroy(gameObject);
}

}
