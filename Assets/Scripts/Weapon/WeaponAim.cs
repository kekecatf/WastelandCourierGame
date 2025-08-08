// WeaponAim.cs (SİMETRİ SORUNU ÇÖZÜLMÜŞ, EN BASİT VE DOĞRU HALİ)

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class WeaponAim : MonoBehaviour
{
    [Header("Components")]
    [Tooltip("Silahın ve altındaki her şeyin döneceği ana pivot. Genellikle silahın kendisidir.")]
    public Transform weaponPivot; 
    
    [Tooltip("Bu silaha ait Light 2D objesi.")]
    public Light2D flashlight;

    // FirePoint referansına artık kod içinde ihtiyacımız yok, çünkü pozisyonunu değiştirmeyeceğiz.
    // public Transform firePoint; 

    private SpriteRenderer weaponSpriteRenderer;

    private void Awake()
    {
        if (weaponPivot != null)
        {
            weaponSpriteRenderer = weaponPivot.GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void OnDisable()
    {
        if (flashlight != null) flashlight.gameObject.SetActive(false);
    }
    
    private void OnEnable()
    {
        if (flashlight != null) flashlight.gameObject.SetActive(true);
    }

    private void LateUpdate()
    {
        if (Camera.main == null || weaponPivot == null) return;

        // --- Gerekli Verileri Al ---
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector3 aimDirection = (worldPosition - weaponPivot.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        float facingDirection = PlayerMovement.FacingDirection;

        // --- Sadece Rotasyon ve Görsel Düzeltme ---

        // 1. Silahın PİVOTUNU fareye doğru döndür.
        // Bu, altındaki FirePoint'in de doğru yöne bakmasını sağlar.
        weaponPivot.rotation = Quaternion.Euler(0f, 0f, angle);

        // 2. Silahın SPRITE'ını, parent'ın scale'inden kaynaklanan görsel bozulmayı
        // düzeltecek şekilde çevir (flipY).
        if (weaponSpriteRenderer != null)
        {
            weaponSpriteRenderer.flipY = (facingDirection < 0);
        }

        // 3. IŞIĞIN rotasyonunu, parent'ın scale etkisinden BAĞIMSIZ olarak ayarla.
        if (flashlight != null)
        {
            flashlight.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }
    }
}
