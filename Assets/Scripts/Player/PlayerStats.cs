using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float moveSpeed = 5f;
    public int inventoryCapacity = 20;
    public int gold = 10;

    private Dictionary<string, int> resources = new Dictionary<string, int>();
    private HashSet<string> unlockedBlueprints = new HashSet<string>();

    public int maxHunger = 100;
    public int currentHunger;
    public float hungerDecreaseInterval = 5f; // her 5 saniyede bir azalır
    public int hungerDecreaseAmount = 1;

    private float hungerTimer;

    void Start()
    {
        currentHunger = maxHunger;
        hungerTimer = hungerDecreaseInterval;
    }

    void Update()
    {
        HandleHunger();
    }

    void HandleHunger()
    {
        hungerTimer -= Time.deltaTime;
        if (hungerTimer <= 0f)
        {
            currentHunger = Mathf.Max(0, currentHunger - hungerDecreaseAmount);
            hungerTimer = hungerDecreaseInterval;
            Debug.Log("🥩 Açlık: " + currentHunger);
        }
    }

    public void UpgradeSpeed()
    {
        if (gold >= 3)
        {
            moveSpeed += 1f;
            gold -= 3;
            Debug.Log("🏃‍♂️ Hız geliştirildi!");
        }
    }

    public void UpgradeInventory()
    {
        if (gold >= 5)
        {
            inventoryCapacity += 10;
            gold -= 5;
            Debug.Log("🎒 Envanter genişletildi!");
        }
    }

    public void AddResource(string type, int amount)
    {
        int total = GetTotalResourceAmount();
        if (total + amount > inventoryCapacity)
        {
            Debug.Log("🚫 Envanter dolu!");
            return;
        }

        if (!resources.ContainsKey(type))
            resources[type] = 0;

        resources[type] += amount;
        Debug.Log($"{type} toplandı! Toplam: {resources[type]}");
    }

    public bool RemoveResource(string type, int amount)
    {
        if (resources.ContainsKey(type) && resources[type] >= amount)
        {
            resources[type] -= amount;
            return true;
        }
        return false;
    }

    public int GetResourceAmount(string type)
    {
        return resources.ContainsKey(type) ? resources[type] : 0;
    }

    public int GetTotalResourceAmount()
    {
        int total = 0;
        foreach (var entry in resources)
            total += entry.Value;
        return total;
    }

    public void UnlockBlueprint(string blueprintId)
    {
        if (unlockedBlueprints.Add(blueprintId))
            Debug.Log($"📘 Blueprint açıldı: {blueprintId}");
    }

    public bool HasBlueprint(string blueprintId)
    {
        return unlockedBlueprints.Contains(blueprintId);
    }
}
