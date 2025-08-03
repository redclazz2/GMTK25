using UnityEngine;

public class BasicDamage : MonoBehaviour
{
    [HideInInspector] public GameObject owner;
    [HideInInspector] public float damage;
    [HideInInspector] public float criticalChance;
    public float velocity = 4f;
    public float lifetime = 3f;

    private Vector2 direction;

    void Start()
    {
        direction = GetDirectionToNearestEnemy();
        SetRotation(direction);
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += (Vector3)(velocity * Time.deltaTime * direction);
    }

    private Vector2 GetDirectionToNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0)
            return Random.insideUnitCircle.normalized;

        GameObject nearest = null;
        float shortestDistance = float.MaxValue;

        foreach (var enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < shortestDistance)
            {
                shortestDistance = dist;
                nearest = enemy;
            }
        }

        return (nearest.transform.position - transform.position).normalized;
    }

    private void SetRotation(Vector2 dir)
    {
        if (dir != Vector2.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && other.TryGetComponent<IDamageable>(out var damageable))
        {
            bool isCritical = Random.Range(0f, 1f) < criticalChance;

            float dealtDamage = damageable.ReceiveDamage(new DamageInfo
            {
                Attacker = owner,
                BaseAmount = damage * 1.5f,
                IsCritical = isCritical
            });

            StatsComponent.Get(owner).currentStats.health += dealtDamage * StatsComponent.Get(owner).currentStats.lifeSteal;
            Destroy(gameObject);
        }
    }
}
