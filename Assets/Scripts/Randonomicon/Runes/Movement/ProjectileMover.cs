using UnityEngine;

public class ProjectileMover : MonoBehaviour
{
    public float speed = 5f;
    public float detectionRange = 8f;
    public float homingStrength = 2f;

    private Vector2 direction;
    private Camera mainCamera;
    private float lifetime = 10f;
    private Transform targetEnemy;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir;
    }

    private void Update()
    {
        // Find enemy target in range
        FindNearestEnemy();

        // Update direction based on target
        UpdateDirection();

        // Move projectile
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        // Destroy if outside camera or after lifetime
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
        bool isOutside = viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1;
        lifetime -= Time.deltaTime;

        if (isOutside || lifetime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void FindNearestEnemy()
    {
        // Clear target if it's destroyed or too far
        if (targetEnemy == null || Vector2.Distance(transform.position, targetEnemy.position) > detectionRange)
        {
            targetEnemy = null;
        }

        // Find new target if we don't have one
        if (targetEnemy == null)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            float closestDistance = detectionRange;
            Transform closestEnemy = null;

            foreach (GameObject enemy in enemies)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy.transform;
                }
            }

            targetEnemy = closestEnemy;
        }
    }

    private void UpdateDirection()
    {
        if (targetEnemy != null)
        {
            // Calculate direction towards enemy
            Vector2 targetDirection = ((Vector2)targetEnemy.position - (Vector2)transform.position).normalized;

            // Smoothly adjust direction towards target (homing behavior)
            direction = Vector2.Lerp(direction, targetDirection, homingStrength * Time.deltaTime).normalized;
        }
        // If no target, keep current direction (random shooting continues)
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            StatsComponent.Get(collision.gameObject).ApplyModifiers(new StatsBundle { health = -5 });
            Debug.Log("Enemy hit!");
            Destroy(gameObject);
        }
    }
}