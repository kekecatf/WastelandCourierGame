// WeaponAim.cs (G�NCELLENM�� VE �Y�LE�T�R�LM�� HAL�)

using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponAim : MonoBehaviour
{
    // Inspector'dan atama yapabilmek i�in bu referanslar� public yap�yoruz.
    // Bu sayede her silah�n farkl� g�rsel ve ate� etme noktas� olabilir.
    [Tooltip("D�necek olan silah�n g�rselini temsil eden Transform.")]
    public Transform weaponVisual;

    [Tooltip("Merminin olu�turulaca�� noktan�n Transform'u.")]
    public Transform firePoint;

    private void Update()
    {
        // E�er Camera.main null ise veya oyun duraklat�ld�ysa i�lem yapma.
        if (Camera.main == null) return;
        
        HandleAiming();
    }

    private void HandleAiming()
{
    Vector3 mousePosition = Mouse.current.position.ReadValue();
    mousePosition.z = -Camera.main.transform.position.z;
    Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

    Vector3 aimDirection = (worldPosition - transform.position).normalized;
    float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
    Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

    if (weaponVisual != null)
    {
        weaponVisual.rotation = targetRotation;
    }
    if (firePoint != null)
    {
        firePoint.rotation = targetRotation;
    }

    HandleFlipping(angle);
}

    private void HandleFlipping(float angle)
    {
        if (weaponVisual == null) return;

        Vector3 currentScale = weaponVisual.localScale;

        // Fare karakterin soluna ge�ti�inde (a�� > 90 veya < -90)
        if (angle > 90f || angle < -90f)
        {
            // Sprite'�n y eksenindeki scale de�erini negatif yap.
            // Mathf.Abs, orijinal scale'in -1 veya 1 olmas�ndan ba��ms�z �al��mas�n� sa�lar.
            currentScale.y = -Mathf.Abs(currentScale.y);
        }
        else
        {
            // Fare sa� taraftayken y eksenindeki scale de�erini pozitif yap.
            currentScale.y = Mathf.Abs(currentScale.y);
        }

        // Hesaplanm�� yeni scale de�erini uygula.
        weaponVisual.localScale = currentScale;
    }
}