using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Vector2Int gridSize = new Vector2Int(50, 50);
    public float cellSize = 1f;
    public LayerMask terrainMask;

    public Dictionary<Vector2Int, GridNode> nodes = new();

    private Vector2 GridOriginOffset => new Vector2(gridSize.x, gridSize.y) * cellSize * 0.5f;

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
                bool isWalkable = !Physics2D.OverlapCircle(worldPos, 0.6f, terrainMask);

                nodes[new Vector2Int(x, y)] = new GridNode
                {
                    position = new Vector2Int(x, y),
                    worldPosition = worldPos,
                    walkable = isWalkable
                };
            }
        }
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
            Gizmos.color = node.walkable ? Color.gray : Color.red;
            Gizmos.DrawWireCube(node.worldPosition, Vector3.one * cellSize * 0.9f);
        }

        // Optional: draw world origin
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }

}
