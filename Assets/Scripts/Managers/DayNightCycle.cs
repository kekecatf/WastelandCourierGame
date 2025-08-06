using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public float dayDuration = 30f;
    public float nightDuration = 30f;
    private float timer = 0f;

    public LightController lightController;
    public ResourceSpawner spawner;
    public EnemyManager enemyManager;

    [Range(0f, 1f)]
    public float regenerationRatio = 0.5f;

    private bool isDay = true;

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
            timer = isDay ? dayDuration : nightDuration;

            if (isDay)
                HandleDayStart();
            else
                HandleNightStart();
        }
    }

    void HandleDayStart()
    {
        Debug.Log("☀️ Sabah oldu - Kaynaklar yenileniyor");
        spawner.RegenerateResources(regenerationRatio);
        SetAnimalsNightState(false);
        lightController?.SetDay(true);
        MusicManager.Instance?.SetDay(true);
    }

    void HandleNightStart()
    {
        Debug.Log("🌙 Gece başladı - Düşmanlar geliyor!");
        enemyManager?.SpawnEnemies();
        SetAnimalsNightState(true);
        lightController?.SetDay(false);
        MusicManager.Instance?.SetDay(false);
    }

    void SetAnimalsNightState(bool isNight)
    {
        Animal[] allAnimals = FindObjectsOfType<Animal>();
        foreach (var animal in allAnimals)
            animal.SetNight(isNight);
    }
}
