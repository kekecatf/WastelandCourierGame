// WeaponAim.cs (GÖRSEL MARKER SİSTEMİ İLE ÇALIŞAN SON HALİ)

using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponAim : MonoBehaviour
{
    [Header("Core Components")]
    public Transform weaponPivot; 
    public Transform firePoint;
    private SpriteRenderer weaponSpriteRenderer;

    [Header("Position Markers (Visual Setup)")]
    [Tooltip("Yanlara nişan alırken FirePoint'in pozisyonunu belirleyen marker.")]
    public Transform markerSide;
    [Tooltip("Yukarı nişan alırken FirePoint'in pozisyonunu belirleyen marker.")]
    public Transform markerUp;
    [Tooltip("Aşağı nişan alırken FirePoint'in pozisyonunu belirleyen marker.")]
    public Transform markerDown;

    [Header("Aim Settings")]
    [Range(1, 180)]
    public float aimAngleLimit = 90f;

    private void Awake()
    {
        if (weaponPivot != null)
        {
            weaponSpriteRenderer = weaponPivot.GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void LateUpdate()
    {
        if (Camera.main == null || weaponPivot == null || firePoint == null) return;
        
        // Bu iki fonksiyon, pozisyonu ve rotasyonu yönetir.
        UpdateFirePointTransform();
        HandleAimRotation();
    }
    
    // YENİ: Bu fonksiyon, FirePoint'in pozisyonunu ve rotasyonunu marker'lara göre ayarlar.
    private void UpdateFirePointTransform()
    {
        Vector2 moveDirection = PlayerMovement.LastDiscretizedDirection;
        float facingDirectionX = (moveDirection.x != 0) ? Mathf.Sign(moveDirection.x) : PlayerMovement.FacingDirection;

        Transform targetMarker = markerSide; // Varsayılan olarak yan marker'ı kullan

        // Oyuncunun ana hareket yönüne göre doğru marker'ı seç
        if (Mathf.Abs(moveDirection.y) > Mathf.Abs(moveDirection.x)) // Dikey hareket baskınsa
        {
            targetMarker = (moveDirection.y > 0) ? markerUp : markerDown;
        }
        
        if (targetMarker != null)
        {
            // FirePoint'in yerel pozisyonunu ve rotasyonunu marker'ınkiyle eşitle.
            firePoint.localPosition = targetMarker.localPosition;
            firePoint.localRotation = targetMarker.localRotation;

            // Eğer yan marker kullanılıyorsa, X ekseninde simetriyi uygula.
            if (targetMarker == markerSide)
            {
                Vector3 pos = firePoint.localPosition;
                pos.x *= facingDirectionX;
                firePoint.localPosition = pos;
            }
        }
    }
    
    private void HandleAimRotation()
    {
        // ... (Bu fonksiyonun içeriği bir önceki cevaptaki gibi, HİÇBİR DEĞİŞİKLİK YOK) ...
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector3 aimDirection = (worldPosition - weaponPivot.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        Vector2 moveDirection = PlayerMovement.LastDiscretizedDirection;
        float facingDirectionX = (moveDirection.x != 0) ? Mathf.Sign(moveDirection.x) : PlayerMovement.FacingDirection;

        if (weaponSpriteRenderer != null)
        {
            weaponSpriteRenderer.flipX = (facingDirectionX < 0);
            weaponSpriteRenderer.flipY = (facingDirectionX < 0);
        }
        
        float baseAngle = 0f;
        if (Mathf.Abs(moveDirection.y) > Mathf.Abs(moveDirection.x))
        {
            baseAngle = (moveDirection.y > 0) ? 90f : -90f;
        }
        else
        {
            baseAngle = (facingDirectionX < 0) ? 180f : 0f;
        }

        float clampedAngle = Mathf.DeltaAngle(baseAngle, angle);
        float halfAngleLimit = aimAngleLimit / 2f;
        clampedAngle = Mathf.Clamp(clampedAngle, -halfAngleLimit, halfAngleLimit);
        float finalAngle = baseAngle + clampedAngle;

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, finalAngle);
        weaponPivot.rotation = targetRotation;
    }
}