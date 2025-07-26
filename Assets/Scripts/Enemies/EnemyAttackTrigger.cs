using UnityEngine;

public class EnemyAttackTrigger : MonoBehaviour
{
    private Enemy enemy;
    private float damageCooldown = 2f;
    private float lastDamageTime = -999f;
    private Animator animator;

    void Start()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time - lastDamageTime < damageCooldown) return;

        if (other.CompareTag("Player"))
        {
            PlayerStats player = other.GetComponent<PlayerStats>();
            if (player != null)
            {
                player.TakeDamage(enemy.damageToCaravan); // ya da ayrı bir damage değeri
                animator?.Play("Hitting");
                lastDamageTime = Time.time;
            }
        }

        if (other.CompareTag("Caravan"))
        {
            CaravanHealth caravan = other.GetComponent<CaravanHealth>();
            if (caravan != null)
            {
                caravan.TakeDamage(enemy.damageToCaravan);
                animator?.Play("Hitting");
                lastDamageTime = Time.time;
            }
        }
    }
}
