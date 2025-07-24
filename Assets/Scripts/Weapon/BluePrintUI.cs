// BlueprintUI.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class BlueprintUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image weaponIcon;
    public TextMeshProUGUI weaponNameText;
    public Button craftButton;

    private WeaponBlueprint currentBlueprint;

    public void Setup(WeaponBlueprint blueprint, UnityAction craftAction)
    {
        currentBlueprint = blueprint;

        weaponNameText.text = blueprint.weaponName;
        weaponIcon.sprite = blueprint.weaponIcon;

        // Butonun eski tüm listener'larýný temizle ve yenisini ekle.
        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(craftAction);
    }

    public void SetCraftableStatus(bool canCraft)
    {
        // Eðer craftlanabilirse, ikonu renkli ve butonu týklanabilir yap.
        if (canCraft)
        {
            weaponIcon.color = Color.white;
            craftButton.interactable = true;
        }
        // Deðilse, ikonu karart (siyah yap) ve butonu devre dýþý býrak.
        else
        {
            weaponIcon.color = Color.black;
            craftButton.interactable = false;
        }
    }

    public WeaponBlueprint GetBlueprint()
    {
        return currentBlueprint;
    }
}