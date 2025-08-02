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
    public float velocity = 4f;
    public float damageInterval = 0.5f;
    [SerializeField]
    private float lifetime = 3f;

    private Vector2 fixedDirection = Vector2.zero;
    private readonly Dictionary<IDamageable, float> enemyTimers = new();
    private float destroyTimer = 0f;

    void Start()
    {
        SetDirectionToNearestEnemy();
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

        MoveInFixedDirection();
        UpdateDamageTimers();
    }

    void SetDirectionToNearestEnemy()
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

            // Pick random from nearest enemies and set direction
            GameObject randomNearestEnemy = nearestEnemies[Random.Range(0, nearestEnemies.Count)];
            fixedDirection = (randomNearestEnemy.transform.position - transform.position).normalized;
        }
        else
        {
            // No enemies found, move in random direction
            fixedDirection = Random.insideUnitCircle.normalized;
        }
    }

    void MoveInFixedDirection()
    {
        // Always move in the fixed direction set at start
        transform.position += (Vector3)(Time.deltaTime * velocity * fixedDirection);
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
        else if (!other.CompareTag("Player"))
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
            BaseAmount = damage * 1.5f,
            IsCritical = isCritical
        });
    }

    void DestroyFireball()
    {
        Destroy(gameObject);
    }
}