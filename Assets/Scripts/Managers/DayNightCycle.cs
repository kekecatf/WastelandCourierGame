using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public float dayDuration = 30f;
    public float nightDuration = 30f;
    private float timer = 0f;

    private bool isDay = true;

    public ResourceSpawner spawner;  // Kaynak spawn sistemi
    [Range(0f, 1f)]
    public float regenerationRatio = 0.5f; // Sabah kaynakların ne kadarı yenilensin

    public GameObject enemyPrefab;
    public Transform[] spawnPoints; // Düşman spawn noktaları

    void Start()
    {
        isDay = true;
        timer = dayDuration;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            isDay = !isDay;

            if (isDay)
            {
                Debug.Log("☀️ Sabah oldu - Kaynaklar yenileniyor");
                spawner.RegenerateResources(regenerationRatio);
                timer = dayDuration;
            }
            else
            {
                Debug.Log("🌙 Gece başladı - Düşmanlar geliyor!");
                SpawnEnemies();
                timer = nightDuration;
            }
        }
    }

    void SpawnEnemies()
    {
        foreach (Transform point in spawnPoints)
        {
            GameObject enemy = Instantiate(enemyPrefab, point.position, Quaternion.identity);
            // Düşman hedefini kendi içinde ayarlayacak
        }
    }
}
