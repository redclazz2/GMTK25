using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Coroutine hitEffectCoroutine;
    private Vector3 originalScale; // Store the original scale once
    private Color originalColor;   // Store the original color once

    protected StatsComponent stats;
    protected Transform player;
    private List<Vector2> currentPath;
    private int pathIndex;
    private float pathUpdateTimer = 0f;

    [Header("Movement")]
    public float waypointReachDistance = 0.2f;
    public float rotationSpeed = 10f;
    public bool smoothRotation = true;
    public bool predictiveMovement = true;

    [Header("Performance")]
    [Tooltip("How often to update path (seconds). Higher = better performance")]
    public float pathUpdateInterval = 1f;
    [Tooltip("Distance from player before enemy stops updating (0 = always update)")]
    public float maxUpdateDistance = 10f;
    [Tooltip("Use less frequent updates when far from player")]
    public bool useDistanceBasedUpdates = false;

    [Header("Debug")]
    public bool showDebugInfo = false;
    public bool showPathGizmos = true;

    private Vector2 lastPlayerPosition;
    private Vector2 cachedPosition;
    private float cachedMoveSpeed;
    private bool hasValidPath;

    private float baseUpdateInterval;
    private float lastDistanceToPlayer;

    protected virtual void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        stats = StatsComponent.Get(gameObject);
        stats.OnDie += Die;
        stats.OnHit += Hit;

        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Store original values once at start
        originalScale = transform.localScale;
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        baseUpdateInterval = pathUpdateInterval;
        cachedPosition = transform.position;
        lastPlayerPosition = player != null ? (Vector2)player.position : Vector2.zero;

        UpdatePath();
    }

    void Hit()
    {
        if (hitEffectCoroutine != null)
            StopCoroutine(hitEffectCoroutine);

        // Always reset to original values before starting new effect
        transform.localScale = originalScale;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        hitEffectCoroutine = StartCoroutine(HitEffect());
    }

    IEnumerator HitEffect(float duration = 0.1f, float scaleFactor = 1.15f)
    {
        if (spriteRenderer == null)
            yield break;

        Color hitColor = new(1f, 0.5f, 0.5f);

        transform.localScale = new Vector3(originalScale.x * scaleFactor, originalScale.y * (2f - scaleFactor), originalScale.z);
        spriteRenderer.color = hitColor;

        float timer = 0f;
        while (timer < duration)
        {
            float t = timer / duration;
            spriteRenderer.color = Color.Lerp(hitColor, originalColor, t);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
        spriteRenderer.color = originalColor;
        hitEffectCoroutine = null;
    }

    protected virtual void Update()
    {
        if (player == null || stats == null)
            return;

        cachedPosition = transform.position;
        cachedMoveSpeed = stats.currentStats.moveSpeed;

        if (useDistanceBasedUpdates && maxUpdateDistance > 0)
        {
            lastDistanceToPlayer = Vector2.Distance(cachedPosition, player.position);
            if (lastDistanceToPlayer > maxUpdateDistance)
                return;

            pathUpdateInterval = baseUpdateInterval * (1f + lastDistanceToPlayer / 20f);
        }

        pathUpdateTimer += Time.deltaTime;
        if (pathUpdateTimer >= pathUpdateInterval)
        {
            Vector2 currentPlayerPos = player.position;
            if (Vector2.Distance(lastPlayerPosition, currentPlayerPos) > 1f || !hasValidPath)
            {
                UpdatePath();
                lastPlayerPosition = currentPlayerPos;
            }
            pathUpdateTimer = 0f;
        }

        if (hasValidPath)
        {
            FollowPath();
        }
    }

    void UpdatePath()
    {
        if (PathFinder.Instance == null) return;

        List<Vector2> newPath = PathFinder.Instance.GetPathFrom(cachedPosition);

        if (newPath != null && newPath.Count > 0)
        {
            if (currentPath == null || PathSignificantlyChanged(newPath))
            {
                currentPath = newPath;
                pathIndex = 0;
                hasValidPath = true;
            }
        }
        else
        {
            hasValidPath = false;
        }
    }

    bool PathSignificantlyChanged(List<Vector2> newPath)
    {
        if (currentPath == null || currentPath.Count != newPath.Count)
            return true;

        int checkCount = Mathf.Min(3, currentPath.Count, newPath.Count);
        for (int i = 0; i < checkCount; i++)
        {
            if (Vector2.Distance(currentPath[i], newPath[i]) > 0.5f)
                return true;
        }

        return false;
    }

    void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0 || pathIndex >= currentPath.Count)
            return;

        Vector2 targetWaypoint = currentPath[pathIndex];
        Vector2 lookAheadTarget = targetWaypoint;

        if (predictiveMovement && pathIndex + 1 < currentPath.Count)
        {
            float distanceToCurrentWaypoint = Vector2.Distance(cachedPosition, targetWaypoint);
            if (distanceToCurrentWaypoint < waypointReachDistance * 2f)
            {
                Vector2 nextWaypoint = currentPath[pathIndex + 1];
                float blendFactor = 1f - (distanceToCurrentWaypoint / (waypointReachDistance * 2f));
                lookAheadTarget = Vector2.Lerp(targetWaypoint, nextWaypoint, blendFactor * 0.5f);
            }
        }

        float distanceToTarget = Vector2.Distance(cachedPosition, targetWaypoint);
        if (distanceToTarget <= waypointReachDistance)
        {
            pathIndex++;
            if (pathIndex >= currentPath.Count)
            {
                hasValidPath = false;
                return;
            }
        }

        Vector2 moveDirection = (lookAheadTarget - cachedPosition).normalized;
        float moveDistance = cachedMoveSpeed * Time.deltaTime;
        Vector2 newPosition = cachedPosition + moveDirection * moveDistance;
        transform.position = newPosition;

        /*if (smoothRotation && moveDirection.sqrMagnitude > 0.01f)
        {
            float targetAngle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            float currentAngle = transform.eulerAngles.z;
            float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);
            float rotationStep = rotationSpeed * Time.deltaTime;
            angleDiff = Mathf.Clamp(angleDiff, -rotationStep, rotationStep);
            float newAngle = currentAngle + angleDiff;
            transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }*/
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    public void GetPerformanceInfo(out float updateInterval, out float distanceToPlayer)
    {
        updateInterval = pathUpdateInterval;
        distanceToPlayer = lastDistanceToPlayer;
    }

    void OnDrawGizmos()
    {
        if (!showPathGizmos) return;

        Vector2 currentPos = transform.position;

        if (currentPath != null && currentPath.Count > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < pathIndex && i < currentPath.Count; i++)
            {
                Gizmos.DrawSphere(currentPath[i], 0.08f);
            }

            Gizmos.color = Color.yellow;
            Vector2 prevPos = currentPos;

            for (int i = pathIndex; i < currentPath.Count; i++)
            {
                if (i == pathIndex)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(currentPath[i], 0.12f);
                    Gizmos.color = Color.yellow;
                }
                else
                {
                    Gizmos.DrawSphere(currentPath[i], 0.06f);
                }

                Gizmos.DrawLine(prevPos, currentPath[i]);
                prevPos = currentPath[i];
            }

            if (predictiveMovement && pathIndex < currentPath.Count)
            {
                Vector2 targetWaypoint = currentPath[pathIndex];
                if (pathIndex + 1 < currentPath.Count)
                {
                    float distanceToCurrentWaypoint = Vector2.Distance(currentPos, targetWaypoint);
                    if (distanceToCurrentWaypoint < waypointReachDistance * 2f)
                    {
                        Vector2 nextWaypoint = currentPath[pathIndex + 1];
                        float blendFactor = 1f - (distanceToCurrentWaypoint / (waypointReachDistance * 2f));
                        Vector2 lookAheadTarget = Vector2.Lerp(targetWaypoint, nextWaypoint, blendFactor * 0.5f);

                        Gizmos.color = Color.cyan;
                        Gizmos.DrawSphere(lookAheadTarget, 0.08f);
                        Gizmos.DrawLine(currentPos, lookAheadTarget);
                    }
                }
            }
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(currentPos, 0.1f);

        if (smoothRotation)
        {
            Vector2 facingDir = new Vector2(Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad),
                                            Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad));
            Gizmos.color = Color.white;
            Gizmos.DrawRay(currentPos, facingDir * 0.5f);
        }

        if (useDistanceBasedUpdates && maxUpdateDistance > 0)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
            Gizmos.DrawWireSphere(currentPos, maxUpdateDistance);
        }
    }
}