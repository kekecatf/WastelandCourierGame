using UnityEngine;
using UnityEngine.UI;

public class Animal : MonoBehaviour
{
    public string animalType = "Geyik";
    public int maxHealth = 3;
    public float moveSpeed = 2f;
    public float roamRadius = 5f;
    public float detectionRadius = 4f;
    public float fleeSpeedMultiplier = 1.5f;

    public GameObject meatPrefab;
    public GameObject hidePrefab;
    public GameObject hpBarPrefab;

    private int currentHealth;
    private GameObject hpBarInstance;
    private Image hpFillImage;

    private Vector2 roamTarget;
    private AnimalState currentState = AnimalState.Roaming;

    private Transform threatTarget;

    void Start()
    {
        currentHealth = maxHealth;
        PickNewRoamTarget();

        if (hpBarPrefab != null)
        {
            hpBarInstance = Instantiate(hpBarPrefab, transform.position + Vector3.up * 1f, Quaternion.identity);
            hpBarInstance.transform.SetParent(transform, true);

            Transform fill = hpBarInstance.transform.Find("Background/Fill");
            if (fill != null)
                hpFillImage = fill.GetComponent<Image>();
        }
    }

    void Update()
    {
        DetectThreats();

        switch (currentState)
        {
            case AnimalState.Roaming:
                Roam();
                break;

            case AnimalState.Fleeing:
                Flee();
                break;
        }
    }

    void DetectThreats()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player") || hit.CompareTag("Enemy"))
            {
                currentState = AnimalState.Fleeing;
                threatTarget = hit.transform;
                return;
            }
        }

        // Eğer tehdit artık yoksa tekrar dolaşmaya dön
        if (currentState == AnimalState.Fleeing && threatTarget == null)
        {
            currentState = AnimalState.Roaming;
            PickNewRoamTarget();
        }
    }

    void Roam()
    {
        float distance = Vector2.Distance(transform.position, roamTarget);
        if (distance < 0.5f)
        {
            PickNewRoamTarget();
        }

        Vector2 direction = (roamTarget - (Vector2)transform.position).normalized;
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }

    void Flee()
    {
        if (threatTarget == null)
        {
            currentState = AnimalState.Roaming;
            PickNewRoamTarget();
            return;
        }

        Vector2 direction = ((Vector2)transform.position - (Vector2)threatTarget.position).normalized;
        transform.Translate(direction * moveSpeed * fleeSpeedMultiplier * Time.deltaTime);
    }

    void PickNewRoamTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * roamRadius;
        roamTarget = (Vector2)transform.position + randomOffset;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (hpFillImage != null)
        {
            float fillValue = Mathf.Clamp01((float)currentHealth / maxHealth);
            hpFillImage.fillAmount = fillValue;
        }

        if (currentHealth <= 0)
        {
            DropLoot();

            if (hpBarInstance != null)
                Destroy(hpBarInstance);

            Destroy(gameObject);
        }
    }

    void DropLoot()
    {
        Instantiate(meatPrefab, transform.position, Quaternion.identity);
        Instantiate(hidePrefab, transform.position + Vector3.right * 0.5f, Quaternion.identity);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}

