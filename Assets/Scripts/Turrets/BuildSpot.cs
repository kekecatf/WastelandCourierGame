using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuildSpot : MonoBehaviour
{
    [Header("Taret Seviyeleri")]
    public TurretLevelData[] levels;

    [Header("İnşa UI")]
    public float buildTime = 2f;
    public Slider progressBar;
    public GameObject progressCanvas;

    private bool isPlayerNearby = false;
    private float holdTimer = 0f;
    private bool isBuilding = false;

    private int currentLevel = 0;
    private GameObject currentTurret;

    private void Start()
    {
        if (progressCanvas != null)
            progressCanvas.SetActive(false);
    }

    void Update()
{
    if (!isPlayerNearby) return;

    // Her seviyede (ilk kurulum + yükseltme) basılı tutma olacak
    if (Keyboard.current.eKey.isPressed)
    {
        if (currentLevel >= levels.Length)
        {
            Debug.Log("⚠️ Zaten maksimum seviyede.");
            return;
        }

        if (!HasEnoughResources())
        {
            Debug.Log("🚫 Yetersiz kaynak!");
            return;
        }

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
            BuildOrUpgradeTurret();
    }
    else if (isBuilding)
    {
        ResetBuild();
    }
}


    void BuildOrUpgradeTurret()
    {
        if (currentLevel >= levels.Length)
        {
            Debug.Log("⚠️ Maksimum seviyeye ulaşıldı.");
            return;
        }

        TurretLevelData levelData = levels[currentLevel];

        PlayerInventory.Instance.Remove("Stone", levelData.requiredStone);
        PlayerInventory.Instance.Remove("Wood", levelData.requiredWood);

        if (currentTurret != null)
            Destroy(currentTurret);

        Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y, -1f); // 👈 böyle
        currentTurret = Instantiate(levelData.prefab, spawnPos, Quaternion.identity);
        currentLevel++;

        ResetBuild();

        Debug.Log($"✅ Kule seviyesi {currentLevel} oldu!");
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
        if (collision.CompareTag("Player"))
            isPlayerNearby = true;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            ResetBuild();
        }
    }

    bool HasEnoughResources()
    {
        if (currentLevel >= levels.Length)
            return false;

        TurretLevelData levelData = levels[currentLevel];

        return PlayerInventory.Instance.GetAmount("Stone") >= levelData.requiredStone &&
               PlayerInventory.Instance.GetAmount("Wood") >= levelData.requiredWood;
    }
}
