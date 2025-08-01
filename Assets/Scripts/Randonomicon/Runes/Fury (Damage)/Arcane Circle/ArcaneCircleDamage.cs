using UnityEngine;
using System.Collections.Generic;

public class ArcaneCircleDamage : MonoBehaviour
{
    public GameObject owner;
    public float damage;
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

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IDamageable>(out var damageable) && !enemiesInRange.Contains(damageable))
        {
            enemiesInRange.Add(damageable);
            Debug.Log($"Enemy entered circle: {other.name}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IDamageable>(out var damageable))
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
                enemy.ReceiveDamage(new DamageInfo { Attacker = owner, BaseAmount = damage, IsCritical = isCritical });
            }
            else
            {
                enemiesInRange.Remove(enemy);
            }
        }
    }
}