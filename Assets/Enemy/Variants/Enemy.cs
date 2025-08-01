using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    protected StatsComponent stats;
    private Transform player;
    private List<Vector2> currentPath;
    private int pathIndex;
    private float pathUpdateTimer = 0f;
    private const float pathUpdateInterval = 2f;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool showPathGizmos = true;
    
    [Header("Movement Settings")]
    public float waypointReachDistance = 0.3f;

    protected virtual void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        stats = StatsComponent.Get(gameObject);
        stats.OnDie += Die;
        UpdatePath();
    }

    protected virtual void Update()
    {
        if (player == null || stats == null)
            return;

        pathUpdateTimer += Time.deltaTime;
        if (pathUpdateTimer >= pathUpdateInterval)
        {
            UpdatePath();
            pathUpdateTimer = 0f;
        }

        FollowPath();
    }

    void UpdatePath()
    {
        if (PathFinder.Instance == null) return;

        Vector2 start = transform.position;
        List<Vector2> newPath = PathFinder.Instance.GetPathFrom(start);
        
        if (newPath != null && newPath.Count > 0)
        {
            currentPath = newPath;
            pathIndex = 0;
            
            if (showDebugInfo)
            {
                Debug.Log($"{name}: New path with {currentPath.Count} waypoints");
                Debug.Log($"{name}: Enemy position: {start}");
                
                // Print ALL waypoints to see what's in the path
                for (int i = 0; i < currentPath.Count; i++)
                {
                    Debug.Log($"{name}: Waypoint {i}: {currentPath[i]}");
                }
                
                Debug.Log($"{name}: Player position: {(Vector2)player.position}");
                
                // Check if path makes sense
                if (currentPath.Count > 0)
                {
                    float distToFirst = Vector2.Distance(start, currentPath[0]);
                    float distToLast = Vector2.Distance(currentPath[currentPath.Count-1], player.position);
                    Debug.Log($"{name}: Distance to first waypoint: {distToFirst:F2}");
                    Debug.Log($"{name}: Distance from last waypoint to player: {distToLast:F2}");
                }
            }
        }
        else
        {
            if (showDebugInfo) Debug.LogWarning($"{name}: No path found");
        }
    }

    void FollowPath()
    {
        // No path - stop moving
        if (currentPath == null || currentPath.Count == 0)
        {
            if (showDebugInfo) Debug.LogWarning($"{name}: No path to follow");
            return;
        }
        
        // Reached end of path
        if (pathIndex >= currentPath.Count)
        {
            if (showDebugInfo) Debug.Log($"{name}: Completed path");
            return;
        }

        Vector2 currentPos = transform.position;
        
        // CRITICAL: Use the CURRENT waypoint (pathIndex), NOT the final waypoint!
        Vector2 targetWaypoint = currentPath[pathIndex];
        
        // DOUBLE CHECK: Make sure we're not accidentally using the wrong waypoint
        if (showDebugInfo && Time.frameCount % 60 == 0)
        {
            Debug.Log($"{name}: Target is waypoint {pathIndex}: {targetWaypoint}");
            Debug.Log($"{name}: NOT the final waypoint {currentPath.Count-1}: {currentPath[currentPath.Count-1]}");
            Debug.Log($"{name}: NOT the player position: {(Vector2)player.position}");
        }
        
        // NEVER use player position for movement!
        // Vector2 playerPos = (Vector2)player.position; // DON'T DO THIS
        
        float distanceToWaypoint = Vector2.Distance(currentPos, targetWaypoint);
        
        if (showDebugInfo && Time.frameCount % 60 == 0)
        {
            Debug.Log($"{name}: Following waypoint {pathIndex}/{currentPath.Count} at {targetWaypoint}");
            Debug.Log($"{name}: Distance to waypoint: {distanceToWaypoint:F2}");
            Debug.Log($"{name}: Current position: {currentPos}");
        }
        
        // Check if we've reached the current waypoint
        if (distanceToWaypoint <= waypointReachDistance)
        {
            pathIndex++;
            if (showDebugInfo) 
                Debug.Log($"{name}: ✓ Reached waypoint {pathIndex - 1}! Moving to {pathIndex}");
            
            // If we've completed the path, stop
            if (pathIndex >= currentPath.Count)
            {
                if (showDebugInfo) Debug.Log($"{name}: ✓ Reached final destination!");
                return;
            }
            
            // Update target to next waypoint
            targetWaypoint = currentPath[pathIndex];
        }
        
        // Move toward the CURRENT WAYPOINT (not the player!)
        Vector2 direction = (targetWaypoint - currentPos).normalized;
        float moveDistance = stats.currentStats.moveSpeed * Time.deltaTime;
        
        // Apply movement
        Vector2 newPosition = Vector2.MoveTowards(currentPos, targetWaypoint, moveDistance);
        transform.position = newPosition;
        
        // Debug movement
        if (showDebugInfo && Time.frameCount % 60 == 0)
        {
            Debug.Log($"{name}: Moving {direction * moveDistance} toward waypoint {pathIndex}");
        }
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
    
    void OnDrawGizmos()
    {
        if (!showPathGizmos) return;
        
        Vector2 currentPos = transform.position;
        
        // Draw the path with waypoint numbers
        if (currentPath != null && currentPath.Count > 0)
        {
            // Draw completed portion in green
            Gizmos.color = Color.green;
            for (int i = 0; i < pathIndex && i < currentPath.Count; i++)
            {
                Gizmos.DrawSphere(currentPath[i], 0.08f);
                if (i > 0)
                    Gizmos.DrawLine(currentPath[i-1], currentPath[i]);
            }
            
            // Draw remaining path in yellow
            Gizmos.color = Color.yellow;
            Vector2 prevPos = pathIndex > 0 ? currentPath[pathIndex - 1] : currentPos;
            
            for (int i = pathIndex; i < currentPath.Count; i++)
            {
                if (i == pathIndex)
                {
                    // Current target waypoint in RED
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(currentPath[i], 0.15f);
                    
                    // Show reach distance
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(currentPath[i], waypointReachDistance);
                    
                    // Draw movement line
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(currentPos, currentPath[i]);
                    
                    Gizmos.color = Color.yellow;
                }
                else
                {
                    Gizmos.DrawSphere(currentPath[i], 0.08f);
                }
                
                // Draw connections
                Gizmos.DrawLine(prevPos, currentPath[i]);
                prevPos = currentPath[i];
            }
            
            // Draw waypoint numbers
            #if UNITY_EDITOR
            for (int i = 0; i < currentPath.Count; i++)
            {
                UnityEditor.Handles.color = i == pathIndex ? Color.red : Color.white;
                UnityEditor.Handles.Label(currentPath[i] + Vector2.up * 0.2f, i.ToString());
            }
            #endif
        }
        
        // Draw enemy
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(currentPos, 0.12f);
        
        // Show what direction the enemy is facing
        if (currentPath != null && pathIndex < currentPath.Count)
        {
            Vector2 facingDirection = (currentPath[pathIndex] - currentPos).normalized;
            Gizmos.color = Color.white;
            Gizmos.DrawRay(currentPos, facingDirection * 0.5f);
        }
    }
}