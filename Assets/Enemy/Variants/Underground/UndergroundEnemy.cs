using UnityEngine;

public class UndergroundEnemy : Enemy
{
    [Header("Hybrid Behavior")]
    [Tooltip("Distance at which enemy switches to pathfinding mode")]
    public float pathfindingRange = 3f;

    [Tooltip("Sprite used when moving directly toward player")]
    public Sprite directMovementSprite;

    [Tooltip("Sprite used when using pathfinding")]
    public Sprite pathfindingSprite;

    [Header("Direct Movement")]
    [Tooltip("Speed multiplier when moving directly (can be different from pathfinding speed)")]
    public float directMovementSpeedMultiplier = 1.2f;
    private float pathfindingEnteredTime;
    public float directModeCooldown = 2f;
    private bool isUsingPathfinding = false;

    protected override void Start()
    {
        base.Start();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            return;

        if (directMovementSprite != null)
        {
            spriteRenderer.sprite = directMovementSprite;
        }

        spriteRenderer.enabled = true;
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        stats.SetInvulnerable(true);
    }

    protected override void Update()
    {
        if (player == null || stats == null)
            return;

        FlipSprite();

        if (Time.time % 0.5f < Time.deltaTime && spriteRenderer != null && spriteRenderer.color != Color.white)
        {
            spriteRenderer.color = Color.white;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool wantsToUsePathfinding = distanceToPlayer <= pathfindingRange;

        if (wantsToUsePathfinding && !isUsingPathfinding)
        {
            SwitchMode(true);
        }
        else if (!wantsToUsePathfinding && isUsingPathfinding)
        {
            if (Time.time - pathfindingEnteredTime >= directModeCooldown)
            {
                SwitchMode(false);
            }
        }

        if (isUsingPathfinding)
        {
            base.Update();
        }
        else
        {
            MoveDirectlyToPlayer();
        }
    }

    private void SwitchMode(bool usePathfinding)
    {
        isUsingPathfinding = usePathfinding;

        if (usePathfinding)
        {
            pathfindingEnteredTime = Time.time;
        }

        if (spriteRenderer != null)
        {
            if (usePathfinding && pathfindingSprite != null)
            {
                stats.SetInvulnerable(false);
                spriteRenderer.sprite = pathfindingSprite;
            }
            else if (!usePathfinding && directMovementSprite != null)
            {
                stats.SetInvulnerable(true);
                spriteRenderer.sprite = directMovementSprite;
            }
        }

        int enemyLayer = gameObject.layer;
        int terrainLayer = LayerMask.NameToLayer("Terrain");
        Physics2D.IgnoreLayerCollision(enemyLayer, terrainLayer, !usePathfinding);
    }


    private void MoveDirectlyToPlayer()
    {
        Vector2 currentPos = transform.position;
        Vector2 targetPos = player.position;

        Vector2 direction = (targetPos - currentPos).normalized;
        float moveSpeed = stats.currentStats.moveSpeed * directMovementSpeedMultiplier;
        float moveDistance = moveSpeed * Time.deltaTime;

        Vector2 newPosition = currentPos + direction * moveDistance;
        transform.position = newPosition;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (!showPathGizmos) return;

        Vector2 currentPos = transform.position;

        if (isUsingPathfinding)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        }
        else
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        }

        Gizmos.DrawWireSphere(currentPos, pathfindingRange);

        if (!isUsingPathfinding && player != null)
        {
            Gizmos.color = Color.magenta;
            Vector2 direction = ((Vector2)player.position - currentPos).normalized;
            Gizmos.DrawRay(currentPos, direction * 1f);
        }
    }
}
