using UnityEngine;
using System.Collections.Generic;

public class ArcaneCircleDamage : MonoBehaviour
{
    [HideInInspector]
    public GameObject owner;
    [HideInInspector]
    public float damage;
    [HideInInspector]
    public float criticalChance;
    public float damageInterval = 1f;

    private readonly List<IDamageable> enemiesInRange = new();
    private float damageTimer = 0f;

    void Update()
    {
        if (enemiesInRange.Count > 0)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                DealDamageToAllEnemies();
                damageTimer = 0f;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") &&
            other.TryGetComponent<IDamageable>(out var damageable) &&
            !enemiesInRange.Contains(damageable))
        {
            enemiesInRange.Add(damageable);
            Debug.Log($"Enemy entered circle: {other.name}");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") &&
            other.TryGetComponent<IDamageable>(out var damageable))
        {
            enemiesInRange.Remove(damageable);
            Debug.Log($"Enemy left circle: {other.name}");
        }
    }

    void DealDamageToAllEnemies()
    {
        List<IDamageable> enemiesToDamage = new(enemiesInRange);

        foreach (IDamageable enemy in enemiesToDamage)
        {
            if (enemy != null)
            {
                bool isCritical = Random.Range(0f, 1f) < criticalChance;
                float dmg = enemy.ReceiveDamage(new DamageInfo
                {
                    Attacker = owner,
                    BaseAmount = damage,
                    IsCritical = isCritical
                });
                StatsComponent.Get(owner).currentStats.health += dmg * StatsComponent.Get(owner).currentStats.lifeSteal;
            }
            else
            {
                enemiesInRange.Remove(enemy);
            }
        }
    }
}