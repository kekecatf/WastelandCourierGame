using UnityEngine;
using System.Collections.Generic;

// Bu, bir silahý craftlamak için gereken parçalarý ve miktarlarýný tanýmlar.
[System.Serializable]
public class PartRequirement
{
    public WeaponPartType partType;
    public int amount;
}

[CreateAssetMenu(fileName = "New Weapon Blueprint", menuName = "Crafting/Weapon Blueprint")]
public class WeaponBlueprint : ScriptableObject
{
    public string weaponName;
    public Sprite weaponIcon;

    // Craft edilecek silahýn WeaponSlotManager'daki slot index'i
    // 0: Makineli, 1: Tabanca, 2: Kýlýç ...
    public int weaponSlotIndexToUnlock;

    // Bu tarif için gereken parçalarýn listesi
    public List<PartRequirement> requiredParts;
}