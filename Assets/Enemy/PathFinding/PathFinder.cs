using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    public static PathFinder Instance { get; private set; }
    public GridManager grid;
    public Transform player;

    [Header("Smoothing")]
    public bool enablePathSmoothing = true;
    public bool enableDiagonalMovement = true;
    public float cornerCuttingDistance = 0.3f;

    [Header("Performance")]
    public float updateInterval = 0.1f;

    private Vector2Int lastPlayerGridPos = Vector2Int.one * int.MinValue;
    private float lastUpdateTime;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        if (player != null && Time.time - lastUpdateTime >= updateInterval)
        {
            Vector2Int currentGridPos = grid.WorldToGrid(player.position);
            if (currentGridPos != lastPlayerGridPos)
            {
                RunReverseAStar(currentGridPos);
                lastPlayerGridPos = currentGridPos;
                lastUpdateTime = Time.time;
            }
        }
    }

    void RunReverseAStar(Vector2Int start)
    {
        foreach (var node in grid.nodes.Values)
        {
            node.cost = float.MaxValue;
            node.cameFrom = null;
            node.inClosedSet = false;
        }

        if (!grid.nodes.TryGetValue(start, out var startNode))
            return;

        startNode.cost = 0;
        PriorityQueue<GridNode> open = new();
        HashSet<GridNode> openSet = new();

        open.Enqueue(startNode, 0);
        openSet.Add(startNode);

        while (open.Count > 0)
        {
            GridNode current = open.Dequeue();
            openSet.Remove(current);
            current.inClosedSet = true;

            foreach (var neighbor in enableDiagonalMovement ? GetNeighborsWithDiagonals(current) : GetNeighbors(current))
            {
                if (neighbor.inClosedSet) continue;

                float newCost = current.cost + GetMovementCost(current, neighbor);

                if (newCost < neighbor.cost)
                {
                    neighbor.cost = newCost;
                    neighbor.cameFrom = current;

                    float fScore = newCost + GetHeuristic(neighbor.position, start);

                    if (!openSet.Contains(neighbor))
                    {
                        open.Enqueue(neighbor, fScore);
                        openSet.Add(neighbor);
                    }
                }
            }
        }
    }

    float GetHeuristic(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
    }

    float GetMovementCost(GridNode from, GridNode to)
    {
        Vector2Int diff = to.position - from.position;
        if (Mathf.Abs(diff.x) == 1 && Mathf.Abs(diff.y) == 1)
            return 1.414f;
        return 1f;
    }

    public List<Vector2> GetPathFrom(Vector2 worldPos)
    {
        Vector2Int gridPos = grid.WorldToGrid(worldPos);

        if (!grid.nodes.TryGetValue(gridPos, out var startNode) || !startNode.walkable)
        {
            gridPos = FindNearestWalkableTile(gridPos);
            if (gridPos == Vector2Int.one * int.MinValue || !grid.nodes.TryGetValue(gridPos, out startNode))
                return new List<Vector2>();
        }

        if (startNode.cost == float.MaxValue)
            return new List<Vector2>();

        List<Vector2> rawPath = new();
        var current = startNode;

        while (current != null && current.cameFrom != null)
        {
            current = current.cameFrom;
            rawPath.Add(grid.GridToWorld(current.position));
        }

        if (enablePathSmoothing && rawPath.Count > 0)
        {
            List<Vector2> smoothedPath = SmoothPath(worldPos, rawPath);
            return smoothedPath;
        }

        return rawPath;
    }

    List<Vector2> SmoothPath(Vector2 startPos, List<Vector2> originalPath)
    {
        if (originalPath.Count <= 1) return originalPath;

        List<Vector2> smoothedPath = new();
        Vector2 currentPos = startPos;
        int pathIndex = 0;

        while (pathIndex < originalPath.Count)
        {
            int furthestVisibleIndex = pathIndex;

            for (int i = pathIndex + 1; i < originalPath.Count; i++)
            {
                if (HasClearLineOfSight(currentPos, originalPath[i]))
                    furthestVisibleIndex = i;
                else
                    break;
            }

            smoothedPath.Add(originalPath[furthestVisibleIndex]);
            currentPos = originalPath[furthestVisibleIndex];
            pathIndex = furthestVisibleIndex + 1;
        }

        return smoothedPath;
    }

    bool HasClearLineOfSight(Vector2 from, Vector2 to)
    {
        float distance = Vector2.Distance(from, to);
        Vector2 direction = (to - from).normalized;
        RaycastHit2D hit = Physics2D.Raycast(from, direction, distance, grid.terrainMask);
        return hit.collider == null;
    }

    List<Vector2> GetCornerCutPath(List<Vector2> originalPath)
    {
        if (originalPath.Count <= 2) return originalPath;

        List<Vector2> cuttingPath = new();
        cuttingPath.Add(originalPath[0]);

        for (int i = 1; i < originalPath.Count - 1; i++)
        {
            Vector2 prev = originalPath[i - 1];
            Vector2 current = originalPath[i];
            Vector2 next = originalPath[i + 1];

            Vector2 dir1 = (current - prev).normalized;
            Vector2 dir2 = (next - current).normalized;

            float angle = Vector2.Angle(dir1, dir2);

            if (angle > 45f)
            {
                Vector2 cutPoint = current + (prev - current).normalized * cornerCuttingDistance +
                                            (next - current).normalized * cornerCuttingDistance;
                cuttingPath.Add(cutPoint);
            }
            else
            {
                cuttingPath.Add(current);
            }
        }

        cuttingPath.Add(originalPath[originalPath.Count - 1]);
        return cuttingPath;
    }

    List<GridNode> GetNeighbors(GridNode node)
    {
        List<GridNode> neighbors = new();
        Vector2Int[] directions = {
            Vector2Int.up, Vector2Int.down,
            Vector2Int.left, Vector2Int.right
        };

        foreach (var dir in directions)
        {
            Vector2Int pos = node.position + dir;
            if (grid.nodes.TryGetValue(pos, out GridNode neighbor) && neighbor.walkable)
                neighbors.Add(neighbor);
        }

        return neighbors;
    }

    Vector2Int FindNearestWalkableTile(Vector2Int startPos)
    {
        for (int radius = 1; radius <= 10; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius) continue;

                    Vector2Int checkPos = startPos + new Vector2Int(x, y);

                    if (grid.nodes.TryGetValue(checkPos, out var node) && node.walkable)
                        return checkPos;
                }
            }
        }

        return Vector2Int.one * int.MinValue;
    }

    List<GridNode> GetNeighborsWithDiagonals(GridNode node)
    {
        List<GridNode> neighbors = new();
        Vector2Int[] directions = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            new Vector2Int(1, 1), new Vector2Int(-1, 1),
            new Vector2Int(1, -1), new Vector2Int(-1, -1)
        };

        foreach (var dir in directions)
        {
            Vector2Int pos = node.position + dir;
            if (grid.nodes.TryGetValue(pos, out GridNode neighbor) && neighbor.walkable)
            {
                if (Mathf.Abs(dir.x) == 1 && Mathf.Abs(dir.y) == 1)
                {
                    bool horizontalClear = grid.nodes.TryGetValue(node.position + new Vector2Int(dir.x, 0), out var h) && h.walkable;
                    bool verticalClear = grid.nodes.TryGetValue(node.position + new Vector2Int(0, dir.y), out var v) && v.walkable;

                    if (horizontalClear && verticalClear)
                        neighbors.Add(neighbor);
                }
                else
                {
                    neighbors.Add(neighbor);
                }
            }
        }

        return neighbors;
    }
}