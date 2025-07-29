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

    public int currentXP = 0;
    public int level = 1;
    public int skillPoints = 0;
    public int xpToNextLevel = 100;
    [Header("Sağlık")]
    public int maxHealth = 100;
    public int currentHealth;

    public delegate void OnLevelUp();
    public event OnLevelUp onLevelUp;

    private HashSet<WeaponPartType> collectedParts = new HashSet<WeaponPartType>();
    private Dictionary<WeaponPartType, int> weaponParts = new Dictionary<WeaponPartType, int>();

    void Start()
    {
        currentHunger = maxHunger;
        hungerTimer = hungerDecreaseInterval;

        currentHealth = maxHealth;
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

    public void TakeDamage(int damage)
    {

        PlayerHealthUI healthUI = FindObjectOfType<PlayerHealthUI>();
        if (healthUI != null)
        {
            healthUI.SetHealth(currentHealth, maxHealth);
        }
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("☠️ Oyuncu öldü!");
            GameManager.Instance.GameOver();
        }
        else
        {
            Debug.Log($"❤️ Oyuncu hasar aldı: {currentHealth}/{maxHealth}");
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


    public void CollectWeaponPart(WeaponPartType part)
    {
        if (!weaponParts.ContainsKey(part))
            weaponParts[part] = 0;

        weaponParts[part]++;
        Debug.Log($"🧩 {part} parçası toplandı. Şu an: {weaponParts[part]}");

        WeaponPartsUI.Instance?.UpdatePartText(part, weaponParts[part]);
    }
    public int GetWeaponPartCount(WeaponPartType part)
    {
        return weaponParts.ContainsKey(part) ? weaponParts[part] : 0;
    }


    public bool HasAllWeaponParts()
    {
        foreach (WeaponPartType part in System.Enum.GetValues(typeof(WeaponPartType)))
        {
            if (!collectedParts.Contains(part))
                return false;
        }
        return true;
    }

    public void ResetCollectedParts()
    {
        collectedParts.Clear();
    }


    public void AddXP(int amount)
    {
        currentXP += amount;
        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        level++;
        currentXP -= xpToNextLevel;
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.2f); // zorluk artışı
        skillPoints++;
        Debug.Log("🎉 Seviye atladın! Yeni puan: " + skillPoints);
        onLevelUp?.Invoke();
    }

    public void ConsumeWeaponParts(List<PartRequirement> partsToConsume)
    {
        foreach (var partInfo in partsToConsume)
        {
            if (weaponParts.ContainsKey(partInfo.partType) && weaponParts[partInfo.partType] >= partInfo.amount)
            {
                weaponParts[partInfo.partType] -= partInfo.amount;
                // UI Güncellemesi
                WeaponPartsUI.Instance?.UpdatePartText(partInfo.partType, weaponParts[partInfo.partType]);
            }
        }
    }

    public void EatCookedMeat()
    {
        if (RemoveResource("CookedMeat", 1))
        {
            currentHunger = Mathf.Min(maxHunger, currentHunger + 30); // daha fazla doyurur
            Debug.Log("🍗 Pişmiş et yendi! Açlık: " + currentHunger);
        }
    }
}
