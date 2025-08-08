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
        if (GameStateManager.IsGamePaused) return; // ← oyun duruyorsa silah dönmesin
        if (Camera.main == null || weaponPivot == null) return;

        Vector3 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector3 aimDirection = (worldPosition - weaponPivot.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        float facingDirection = PlayerMovement.FacingDirection;

        weaponPivot.rotation = Quaternion.Euler(0f, 0f, angle);


        if (weaponSpriteRenderer != null)
        {
            weaponSpriteRenderer.flipY = (facingDirection < 0);
        }

        if (flashlight != null)
        {
            flashlight.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        }
    }
}
