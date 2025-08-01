using UnityEngine;
using System.Collections.Generic;

public class FireballDamage : MonoBehaviour
{
    [HideInInspector]
    public GameObject owner;
    [HideInInspector]
    public float damage;
    [HideInInspector]
    public float criticalChance;
    [HideInInspector]
    public float velocity = 2f;
    [HideInInspector]
    public float damageInterval = 1f;
    [HideInInspector]
    public float lifetime = 10f;

    private Transform target;
    private Vector2 currentDirection = Vector2.zero;
    private readonly Dictionary<IDamageable, float> enemyTimers = new();
    private float destroyTimer = 0f;

    void Start()
    {
        FindRandomTarget();
        destroyTimer = lifetime;
    }

    void Update()
    {
        destroyTimer -= Time.deltaTime;
        if (destroyTimer <= 0f)
        {
            DestroyFireball();
            return;
        }

        MoveTowardsTarget();
        UpdateDamageTimers();
    }

    void FindRandomTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length > 0)
        {
            // Find nearest enemies first
            List<GameObject> nearestEnemies = new();
            float minDistance = float.MaxValue;

            // Find the minimum distance
            foreach (GameObject enemy in enemies)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }

            // Add all enemies at minimum distance (in case of ties)
            foreach (GameObject enemy in enemies)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (Mathf.Approximately(distance, minDistance))
                {
                    nearestEnemies.Add(enemy);
                }
            }

            // Pick random from nearest enemies
            GameObject randomNearestEnemy = nearestEnemies[Random.Range(0, nearestEnemies.Count)];
            target = randomNearestEnemy.transform;
        }
        else
        {
            // No enemies found, move in random direction
            currentDirection = Random.insideUnitCircle.normalized;
            target = null;
        }
    }

    void MoveTowardsTarget()
    {
        if (target == null)
        {
            // No target, continue in current direction
            if (currentDirection == Vector2.zero)
            {
                FindRandomTarget();
            }
            else
            {
                transform.position += (Vector3)(currentDirection * velocity * Time.deltaTime);
            }
            return;
        }

        // Check if target still exists
        if (target.gameObject == null)
        {
            FindRandomTarget();
            return;
        }

        // Calculate direction to target
        Vector2 direction = (target.position - transform.position).normalized;
        currentDirection = direction;

        // Move towards target
        transform.position += (Vector3)(direction * velocity * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && other.TryGetComponent<IDamageable>(out var damageable))
        {
            if (!enemyTimers.ContainsKey(damageable))
            {
                enemyTimers.Add(damageable, 0f);
            }
        }
        else if(!other.CompareTag("Player"))
        {
            DestroyFireball();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && other.TryGetComponent<IDamageable>(out var damageable))
        {
            enemyTimers.Remove(damageable);
        }
    }

    void UpdateDamageTimers()
    {
        List<IDamageable> enemiesToDamage = new();
        List<IDamageable> enemiesToRemove = new();

        // Create a copy of the keys to avoid modification during iteration
        List<IDamageable> enemyKeys = new(enemyTimers.Keys);

        foreach (IDamageable enemy in enemyKeys)
        {
            if (enemy == null)
            {
                enemiesToRemove.Add(enemy);
                continue;
            }

            float timer = enemyTimers[enemy];
            timer += Time.deltaTime;
            enemyTimers[enemy] = timer;

            if (timer >= damageInterval)
            {
                enemiesToDamage.Add(enemy);
                enemyTimers[enemy] = 0f;
            }
        }

        foreach (var enemy in enemiesToRemove)
        {
            enemyTimers.Remove(enemy);
        }

        foreach (var enemy in enemiesToDamage)
        {
            DealDamageToEnemy(enemy);
        }
    }

    void DealDamageToEnemy(IDamageable enemy)
    {
        bool isCritical = Random.Range(0f, 1f) < criticalChance;

        enemy.ReceiveDamage(new DamageInfo
        {
            Attacker = owner,
            BaseAmount = damage,
            IsCritical = isCritical
        });
    }

    private void OnBecameInvisible()
    {
        DestroyFireball();
    }

    void DestroyFireball()
    {
        Destroy(gameObject);
    }
}