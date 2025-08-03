using UnityEngine;

public class RangedEnemy : Enemy
{
    [Header("Combat")]
    public float shootingRange = 5f;
    public float fireRate = 1f;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 10f;
    public Animator animator;

    private float lastShotTime;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
    }

    protected override void Update()
    {
        if (player == null || stats == null)
            return;

        FlipSprite();

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= shootingRange)
        {
            TryShoot();
            
        }
        else
        {
            base.Update();
        }
    }

    public void TryShoot()
    {
        if (Time.time - lastShotTime >= fireRate)
        {
            animator.SetBool("IsShooting", true);
            //Shoot();
            lastShotTime = Time.time;
        }
        else
        {
            //animator.SetBool("IsShooting", false);
        }
    }

    public void Shoot()
    {
        if (projectilePrefab == null) return;

        Vector2 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector2 direction = ((Vector2)player.position - spawnPos).normalized;

        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * projectileSpeed;
        }

        animator.SetBool("IsShooting", false);
    }
}