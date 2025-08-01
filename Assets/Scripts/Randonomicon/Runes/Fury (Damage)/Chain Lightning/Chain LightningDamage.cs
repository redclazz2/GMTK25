using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ChainLightningDamage : MonoBehaviour
{
    [HideInInspector] public GameObject owner;
    [HideInInspector] public float damage;
    [HideInInspector] public float criticalChance;
    [HideInInspector] public float velocity = 5f;
    [HideInInspector] public int maxBounces = 3;
    [Tooltip("How close we must be to count as a hit")]
    public float impactRadius = 0.5f;

    private Transform currentTarget;
    private Vector2 currentDirection;
    private int bouncesRemaining;
    private readonly List<IDamageable> hitEnemies = new();
    private bool isFirstSpawn = true;
    private float noEnemyTimer;
    private const float NO_ENEMY_LIFETIME = 3f;
    private Vector2 lastHitPosition;

    void Start()
    {
        bouncesRemaining = maxBounces;
        AcquireInitialTarget();
    }

    void Update()
    {
        if (isFirstSpawn && currentTarget == null)
        {
            noEnemyTimer += Time.deltaTime;
            if (noEnemyTimer >= NO_ENEMY_LIFETIME)
            {
                DestroyProjectile();
                return;
            }
        }

        MoveTowardsTarget();
    }

    private void AcquireInitialTarget()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0)
        {
            isFirstSpawn = true;
            currentDirection = Random.insideUnitCircle.normalized;
            currentTarget = null;
            return;
        }

        var nearest10 = enemies
            .OrderBy(e => Vector2.Distance(transform.position, e.transform.position))
            .Take(10)
            .ToArray();

        var choice = nearest10[Random.Range(0, nearest10.Length)];
        currentTarget = choice.transform;
        isFirstSpawn = false;
    }

    private void AcquireNextBounceTarget()
    {
        var candidates = GameObject.FindGameObjectsWithTag("Enemy")
            .Select(e => e.GetComponent<IDamageable>() is IDamageable dmg && !hitEnemies.Contains(dmg)
                         ? e.transform
                         : null)
            .Where(t => t != null)
            .ToList();

        if (candidates.Count == 0)
        {
            DestroyProjectile();
            return;
        }

        currentTarget = candidates
            .OrderBy(t => Vector2.Distance(lastHitPosition, t.position))
            .First();

        currentDirection = Vector2.zero;
    }

    private void MoveTowardsTarget()
    {
        if (currentTarget == null)
        {
            transform.position += (Vector3)(currentDirection * velocity * Time.deltaTime);
            return;
        }

        if (currentTarget.gameObject == null)
        {
            if (bouncesRemaining > 0)
                AcquireNextBounceTarget();
            else
                DestroyProjectile();
            return;
        }

        Vector2 pos2D = transform.position;
        Vector2 tgt2D = currentTarget.position;
        Vector2 dir = (tgt2D - pos2D).normalized;
        currentDirection = dir;
        transform.position += (Vector3)(dir * velocity * Time.deltaTime);

        float dist = Vector2.Distance(transform.position, currentTarget.position);
        if (dist <= impactRadius)
            ProcessHitOn(currentTarget);
    }

    private void ProcessHitOn(Transform targetT)
    {
        IDamageable dmgable = targetT.GetComponent<IDamageable>();
        if (dmgable == null || hitEnemies.Contains(dmgable))
            return;

        lastHitPosition = transform.position;
        hitEnemies.Add(dmgable);

        bool isCrit = Random.value < criticalChance;
        dmgable.ReceiveDamage(new DamageInfo
        {
            Attacker = owner,
            BaseAmount = damage,
            IsCritical = isCrit
        });

        currentTarget = null;

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