using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public float fireRate = 0.2f;

    private float fireTimer = 0f;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        fireTimer -= Time.deltaTime;

        if (Mouse.current.leftButton.wasPressedThisFrame && fireTimer <= 0f)
        {
            FireTowardMouse();
            fireTimer = fireRate;
        }
    }

    void FireTowardMouse()
    {
        Vector3 screenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = mainCam.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;

        Debug.Log("Mouse World Pos: " + worldPos);

        Vector3 direction = (worldPos - transform.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * bulletSpeed;
        }
    }

}
