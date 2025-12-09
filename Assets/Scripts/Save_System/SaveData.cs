using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public float posX, posY, posZ;

    public int currentHealth;
    public int maxHealth;

    public float currentStamina;
    public float maxStamina;

    public int currentHunger;
    public int maxHunger;

    public int gold;

    public List<InventoryItemData> inventory = new();
    public List<string> unlockedWeaponIDs = new();

    // Ammo storage snapshot
    public List<AmmoEntry> ammoEntries = new();

    public string[] equippedWeaponIDs = new string[3];
    public int[] slotClip = new int[3];
    public int[] slotReserve = new int[3];
    public int activeSlotIndex;

    // 🔥 Karavan silahları
    public CaravanSaveData caravanSave;
}

[System.Serializable]
public class InventoryItemData
{
    public string itemID;
    public int amount;
}

[System.Serializable]
public class AmmoEntry
{
    public string ammoId;
    public int amount;
}
