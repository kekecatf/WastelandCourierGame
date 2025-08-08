using UnityEngine;
using System.Collections.Generic;

public class CaravanInventory : MonoBehaviour
{
    public static CaravanInventory Instance { get; private set; }


    [Header("Starting Stored Weapons")]
    public List<WeaponBlueprint> startingWeapons;
    private HashSet<WeaponBlueprint> storedWeapons = new HashSet<WeaponBlueprint>();

    [Header("Starting Storage")]
    public List<WeaponBlueprint> startingStoredWeapons;
    void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
        foreach (var blueprint in startingWeapons)
        {
            if (blueprint != null)
            {
                StoreWeapon(blueprint);
            }
        }
    }


    public void StoreWeapon(WeaponBlueprint blueprint)
    {
        if (blueprint != null && !storedWeapons.Contains(blueprint))
        {
            storedWeapons.Add(blueprint);
            Debug.Log($"{blueprint.weaponName} karavan deposuna eklendi.");
        }
    }

    public void SwapWeapon(WeaponBlueprint weaponToEquip)
    {
        if (!storedWeapons.Contains(weaponToEquip))
        {
            Debug.LogError($"{weaponToEquip.weaponName} depoda değil! Değiştirilemez.");
            return;
        }

        int targetSlotIndex = weaponToEquip.weaponSlotIndexToUnlock;

        WeaponBlueprint weaponToStore = WeaponSlotManager.Instance.GetBlueprintForSlot(targetSlotIndex);

        WeaponBlueprint currentlyEquipped = WeaponSlotManager.Instance.GetBlueprintForSlot(targetSlotIndex);

        storedWeapons.Remove(weaponToEquip);
        if (currentlyEquipped != null)
        {
            storedWeapons.Add(weaponToStore);
        }


        WeaponSlotManager.Instance.EquipBlueprint(weaponToEquip);

        Debug.Log($"Oyuncu {weaponToEquip.weaponName} kuşandı. {weaponToStore?.weaponName ?? "Boş Slot"} depoya kaldırıldı.");

        if (WeaponCraftingSystem.Instance != null)
        {
            WeaponCraftingSystem.Instance.UpdateAllBlueprintUI();
        }
    }

    public bool IsWeaponStored(WeaponBlueprint blueprint)
    {
        return storedWeapons.Contains(blueprint);
    }
}