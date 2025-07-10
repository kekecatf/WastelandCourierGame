using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuildSpot : MonoBehaviour
{
    public GameObject turretPrefab;
    public float buildTime = 2f;
    public Slider progressBar;
    public GameObject progressCanvas;

    private bool isPlayerNearby = false;
    private float holdTimer = 0f;
    private bool isBuilt = false;
    private bool isBuilding = false;

    private void Start()
    {
        if (progressCanvas != null)
            progressCanvas.SetActive(false);
    }

    void Update()
    {
        if (isPlayerNearby && !isBuilt)
        {
            if (Keyboard.current.eKey.isPressed)
            {
                if (!isBuilding)
                {
                    isBuilding = true;
                    if (progressCanvas != null)
                        progressCanvas.SetActive(true);
                }

                holdTimer += Time.deltaTime;
                if (progressBar != null)
                    progressBar.value = holdTimer / buildTime;

                if (holdTimer >= buildTime)
                    BuildTurret();
            }
            else
            {
                if (isBuilding)
                {
                    ResetBuild();
                }
            }
        }
    }

    void BuildTurret()
    {
        Instantiate(turretPrefab, transform.position, Quaternion.identity);
        isBuilt = true;
        if (progressCanvas != null)
            progressCanvas.SetActive(false);
        Debug.Log("✅ Taret yerleştirildi!");
    }

    void ResetBuild()
    {
        isBuilding = false;
        holdTimer = 0f;
        if (progressBar != null)
            progressBar.value = 0f;
        if (progressCanvas != null)
            progressCanvas.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isBuilt)
        {
            isPlayerNearby = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            ResetBuild();
        }
    }
}
