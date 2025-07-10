using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    private Dictionary<string, int> resources = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void Add(string type, int amount)
    {
        if (!resources.ContainsKey(type))
            resources[type] = 0;

        resources[type] += amount;
        Debug.Log($"{type} toplandı! Toplam: {resources[type]}");
    }

    public int GetAmount(string type)
    {
        return resources.ContainsKey(type) ? resources[type] : 0;
    }
}
