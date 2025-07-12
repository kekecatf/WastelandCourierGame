using UnityEngine;

public class Resource : MonoBehaviour
{
    public ResourceType type;  // Taş, odun, meteorit...
    public int amount = 1;

    public void Collect()
{
    var player = GameObject.FindWithTag("Player");
    if (player == null) return;

    var stats = player.GetComponent<PlayerStats>();
    if (stats == null) return;

    switch (type)
    {
        case ResourceType.Stone:
            stats.AddResource("Stone", amount);
            break;
        case ResourceType.Wood:
            stats.AddResource("Wood", amount);
            break;
        case ResourceType.scrapMetal:
            stats.AddResource("scrapMetal", amount);
            break;
    }

    Destroy(gameObject);
}


}
