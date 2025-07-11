using UnityEngine;

public class Resource : MonoBehaviour
{
    public ResourceType type;  // Taş, odun, meteorit...
    public int amount = 1;

    public void Collect()
    {
        // Ekleme işlemi sadece PlayerInventory üzerinden yapılmalı
        switch (type)
        {
            case ResourceType.Stone:
                PlayerInventory.Instance.Add("Stone", amount);
                break;
            case ResourceType.Wood:
                PlayerInventory.Instance.Add("Wood", amount);
                break;
            case ResourceType.Meteorite:
                PlayerInventory.Instance.Add("Meteorite", amount);
                break;
        }

        Destroy(gameObject);
    }

}
