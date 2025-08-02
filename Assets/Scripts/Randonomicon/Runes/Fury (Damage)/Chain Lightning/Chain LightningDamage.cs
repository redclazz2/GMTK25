using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ChainLightningDamage : MonoBehaviour
{
    [HideInInspector] public GameObject owner;
    [HideInInspector] public float damage;
    [HideInInspector] public float criticalChance;
    [SerializeField] private float velocity = 5f;
    [SerializeField] private int maxBounces = 6;
    [SerializeField] private float impactRadius = 1f;
    [SerializeField] private float bounceRadius = 4f; // Radius to search for next bounce target
    [SerializeField] private float maxLifetime = 4f; // Maximum lifetime to prevent stuck projectiles

    private Transform currentTarget;
    private Vector2 currentDirection;
    private int bouncesRemaining;
    private readonly List<IDamageable> hitEnemies = new();
    private float totalLifetime = 0f;
    private float noTargetTimer = 0f;
    private const float NO_TARGET_LIFETIME = 2f;
    private Vector2 lastHitPosition;

    void Start()
    {
        bouncesRemaining = maxBounces;
        AcquireInitialTarget();
    }

    void Update()
    {
        totalLifetime += Time.deltaTime;

        // Safety check: destroy if alive too long
        if (totalLifetime >= maxLifetime)
        {
            DestroyProjectile();
            return;
        }

        // Handle case where no target is found (initial or bounce)
        if (currentTarget == null)
        {
            noTargetTimer += Time.deltaTime;
            if (noTargetTimer >= NO_TARGET_LIFETIME)
            {
                DestroyProjectile();
                return;
            }
        }
        else
        {
            // Reset timer when we have a target
            noTargetTimer = 0f;
        }

        MoveTowardsTarget();
    }

    private void AcquireInitialTarget()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0)
        {
            currentDirection = Random.insideUnitCircle.normalized;
            currentTarget = null;
            return;
        }

        // Pick from nearest 3 enemies (no radius restriction for initial target)
        var nearest3 = enemies
            .OrderBy(e => Vector2.Distance(transform.position, e.transform.position))
            .Take(3)
            .ToArray();

        var choice = nearest3[Random.Range(0, nearest3.Length)];
        currentTarget = choice.transform;
    }

    private void AcquireNextBounceTarget()
    {
        // Find all enemies within bounce radius that haven't been hit
        var candidates = GameObject.FindGameObjectsWithTag("Enemy")
            .Where(e =>
            {
                // Check if within bounce radius from last hit position
                if (Vector2.Distance(lastHitPosition, e.transform.position) > bounceRadius)
                    return false;

                // Check if it has IDamageable component and hasn't been hit
                var dmg = e.GetComponent<IDamageable>();
                return dmg != null && !hitEnemies.Contains(dmg);
            })
            .Select(e => e.transform)
            .ToList();

        if (candidates.Count == 0)
        {
            // No valid targets in bounce radius
            DestroyProjectile();
            return;
        }

        // Choose the nearest valid target
        currentTarget = candidates
            .OrderBy(t => Vector2.Distance(lastHitPosition, t.position))
            .First();

        currentDirection = Vector2.zero;
    }

    private void MoveTowardsTarget()
    {
        if (currentTarget == null)
        {
            // Moving without target (initial spawn with no enemies)
            transform.position += (Vector3)(currentDirection * velocity * Time.deltaTime);
            return;
        }

        // Check if target still exists
        if (currentTarget.gameObject == null)
        {
            if (bouncesRemaining > 0)
                AcquireNextBounceTarget();
            else
                DestroyProjectile();
            return;
        }

        // Move towards target
        Vector2 pos2D = transform.position;
        Vector2 tgt2D = currentTarget.position;
        Vector2 dir = (tgt2D - pos2D).normalized;
        currentDirection = dir;
        transform.position += (Vector3)(dir * velocity * Time.deltaTime);

        // Check for impact
        float dist = Vector2.Distance(transform.position, currentTarget.position);
        if (dist <= impactRadius)
            ProcessHitOn(currentTarget);
    }

    private void ProcessHitOn(Transform targetT)
    {
        IDamageable dmgable = targetT.GetComponent<IDamageable>();
        if (dmgable == null || hitEnemies.Contains(dmgable))
        {
            // Target is invalid, try to find another target or destroy
            if (bouncesRemaining > 0)
                AcquireNextBounceTarget();
            else
                DestroyProjectile();
            return;
        }

        lastHitPosition = transform.position;
        hitEnemies.Add(dmgable);

        // Deal damage
        bool isCrit = Random.value < criticalChance;
        float dmg = dmgable.ReceiveDamage(new DamageInfo
        {
            Attacker = owner,
            BaseAmount = damage * 0.4f,
            IsCritical = isCrit
        });
        StatsComponent.Get(owner).currentStats.health += dmg * StatsComponent.Get(owner).currentStats.lifeSteal;
        currentTarget = null;

        // Look for next bounce target
        if (bouncesRemaining > 0)
        {
            bouncesRemaining--;
            AcquireNextBounceTarget();
        }
        else
        {
            DestroyProjectile();
        }
    }

    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}