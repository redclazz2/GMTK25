using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Vector2Int gridSize = new Vector2Int(50, 50);
    public float cellSize = 1f;
    public LayerMask terrainMask;

    [Header("Precision Settings")]
    [Tooltip("How to check if cells are walkable")]
    public WalkabilityCheckType checkType = WalkabilityCheckType.MultiplePoints;
    [Tooltip("Radius for circle/square checks")]
    public float checkRadius = 0.4f;
    [Tooltip("Points to check per cell (for MultiplePoints mode)")]
    public int checksPerCell = 9;
    [Tooltip("Percentage of points that must be walkable for cell to be walkable")]
    [Range(0f, 1f)]
    public float walkableThreshold = 0.5f;

    public Dictionary<Vector2Int, GridNode> nodes = new();
    private Vector2 GridOriginOffset => new Vector2(gridSize.x, gridSize.y) * cellSize * 0.5f;

    public enum WalkabilityCheckType
    {
        SinglePoint,      // Just center point
        CircleCheck,      // Single circle (original method)
        SquareCheck,      // Box overlap
        MultiplePoints,   // Multiple point checks within cell
        EdgeSampling      // Check points around the edges
    }

    void Awake()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2 worldPos = GridToWorld(new Vector2Int(x, y));
                bool isWalkable = IsPositionWalkable(worldPos);

                nodes[new Vector2Int(x, y)] = new GridNode
                {
                    position = new Vector2Int(x, y),
                    worldPosition = worldPos,
                    walkable = isWalkable
                };
            }
        }
    }

    bool IsPositionWalkable(Vector2 centerPos)
    {
        switch (checkType)
        {
            case WalkabilityCheckType.SinglePoint:
                return !Physics2D.OverlapPoint(centerPos, terrainMask);

            case WalkabilityCheckType.CircleCheck:
                ContactFilter2D filter = new ContactFilter2D();
                filter.SetLayerMask(terrainMask);
                filter.useTriggers = false;

                Collider2D[] results = new Collider2D[1];
                int hitCount = Physics2D.OverlapCircle(centerPos, checkRadius, filter, results);

                return hitCount == 0;

            case WalkabilityCheckType.SquareCheck:
                Vector2 boxSize = Vector2.one * (checkRadius * 2f);
                return !Physics2D.OverlapBox(centerPos, boxSize, 0f, terrainMask);

            case WalkabilityCheckType.MultiplePoints:
                return CheckMultiplePoints(centerPos);

            case WalkabilityCheckType.EdgeSampling:
                return CheckEdgePoints(centerPos);

            default:
                return !Physics2D.OverlapCircle(centerPos, checkRadius, terrainMask);
        }
    }

    bool CheckMultiplePoints(Vector2 centerPos)
    {
        int walkablePoints = 0;
        int totalPoints = checksPerCell;

        // Create a grid of points within the cell
        int pointsPerSide = Mathf.CeilToInt(Mathf.Sqrt(checksPerCell));
        float stepSize = (cellSize * 0.8f) / (pointsPerSide - 1); // 0.8f to stay within cell bounds
        Vector2 startOffset = Vector2.one * (cellSize * 0.4f); // Start from corner

        for (int i = 0; i < pointsPerSide; i++)
        {
            for (int j = 0; j < pointsPerSide; j++)
            {
                if (walkablePoints + (totalPoints - (i * pointsPerSide + j)) < totalPoints * walkableThreshold)
                {
                    // Early exit if impossible to reach threshold
                    return false;
                }

                Vector2 checkPos = centerPos - startOffset + new Vector2(i * stepSize, j * stepSize);

                if (!Physics2D.OverlapPoint(checkPos, terrainMask))
                {
                    walkablePoints++;
                }

                // Early exit if we've already met the threshold
                if (walkablePoints >= totalPoints * walkableThreshold)
                {
                    return true;
                }
            }
        }

        return (float)walkablePoints / totalPoints >= walkableThreshold;
    }

    bool CheckEdgePoints(Vector2 centerPos)
    {
        int walkablePoints = 0;
        int totalPoints = checksPerCell;

        // Check points around the perimeter of the cell
        for (int i = 0; i < totalPoints; i++)
        {
            float angle = (float)i / totalPoints * 2f * Mathf.PI;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * checkRadius;
            Vector2 checkPos = centerPos + offset;

            if (!Physics2D.OverlapPoint(checkPos, terrainMask))
            {
                walkablePoints++;
            }
        }

        return (float)walkablePoints / totalPoints >= walkableThreshold;
    }

    public Vector2 GridToWorld(Vector2Int gridPos)
    {
        return (Vector2)transform.position
            + new Vector2(gridPos.x, gridPos.y) * cellSize
            - GridOriginOffset;
    }

    public Vector2Int WorldToGrid(Vector2 worldPos)
    {
        Vector2 relative = worldPos - (Vector2)transform.position + GridOriginOffset;
        return new Vector2Int(
            Mathf.FloorToInt(relative.x / cellSize),
            Mathf.FloorToInt(relative.y / cellSize)
        );
    }

    void OnDrawGizmos()
    {
        if (gridSize.x <= 0 || gridSize.y <= 0 || nodes == null || nodes.Count == 0) return;

        foreach (var node in nodes.Values)
        {
            // Color based on walkability
            Gizmos.color = node.walkable ? Color.gray : Color.red;
            Gizmos.DrawWireCube(node.worldPosition, Vector3.one * cellSize * 0.9f);

            // Show check visualization for selected check type
            if (Application.isPlaying)
            {
                DrawCheckVisualization(node.worldPosition);
            }
        }

        // Draw world origin
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }

    void DrawCheckVisualization(Vector2 centerPos)
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);

        switch (checkType)
        {
            case WalkabilityCheckType.CircleCheck:
                Gizmos.DrawWireSphere(centerPos, checkRadius);
                break;

            case WalkabilityCheckType.SquareCheck:
                Vector3 boxSize = Vector3.one * (checkRadius * 2f);
                Gizmos.DrawWireCube(centerPos, boxSize);
                break;

            case WalkabilityCheckType.MultiplePoints:
                // Draw the grid of check points
                int pointsPerSide = Mathf.CeilToInt(Mathf.Sqrt(checksPerCell));
                float stepSize = (cellSize * 0.8f) / (pointsPerSide - 1);
                Vector2 startOffset = Vector2.one * (cellSize * 0.4f);

                for (int i = 0; i < pointsPerSide; i++)
                {
                    for (int j = 0; j < pointsPerSide; j++)
                    {
                        Vector2 checkPos = centerPos - startOffset + new Vector2(i * stepSize, j * stepSize);
                        Gizmos.DrawWireSphere(checkPos, 0.02f);
                    }
                }
                break;

            case WalkabilityCheckType.EdgeSampling:
                // Draw points around the edge
                for (int i = 0; i < checksPerCell; i++)
                {
                    float angle = (float)i / checksPerCell * 2f * Mathf.PI;
                    Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * checkRadius;
                    Vector2 checkPos = centerPos + offset;
                    Gizmos.DrawWireSphere(checkPos, 0.02f);
                }
                break;
        }
    }

    // Utility method to regenerate grid at runtime
    [ContextMenu("Regenerate Grid")]
    public void RegenerateGrid()
    {
        nodes.Clear();
        GenerateGrid();
    }
}