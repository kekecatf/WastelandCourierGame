using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;

    public void SpawnEnemies()
    {
        if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0)
        {
            Debug.LogWarning("❌ EnemyManager: Prefab veya spawn noktası eksik!");
            return;
        }

        foreach (Transform point in spawnPoints)
        {
            int index = Random.Range(0, enemyPrefabs.Length);
            Instantiate(enemyPrefabs[index], point.position, Quaternion.identity);
        }
    }

}
