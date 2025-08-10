using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Düşman Ayarları")]
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;

    [Header("Gece Başına Ayarlar")]
    public int baseEnemyCount = 5;
    public float enemyCountIncreasePerDay = 2f; // her gece artan düşman sayısı

    private int currentDay = 1;

    public void SpawnEnemies()
    {
        if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0)
        {
            Debug.LogWarning("❌ EnemyManager: Prefab veya spawn noktası eksik!");
            return;
        }

        int totalEnemiesToSpawn = Mathf.RoundToInt(baseEnemyCount + (currentDay - 1) * enemyCountIncreasePerDay);
        Debug.Log($"🌙 Gece {currentDay}. Toplam düşman: {totalEnemiesToSpawn}");

        for (int i = 0; i < totalEnemiesToSpawn; i++)
        {
            // Rastgele düşman türü
            int enemyIndex = Random.Range(0, enemyPrefabs.Length);

            // Döngüsel olarak spawn noktası seç (eşit dağılım)
            int spawnPointIndex = i % spawnPoints.Length;

            Instantiate(enemyPrefabs[enemyIndex], spawnPoints[spawnPointIndex].position, Quaternion.identity);
        }

        currentDay++; // Gece sayısını artır
    }

    public void ResetDayCount()
    {
        currentDay = 1;
    }
}
