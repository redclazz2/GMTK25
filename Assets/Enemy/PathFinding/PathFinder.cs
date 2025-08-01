using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    public static PathFinder Instance { get; private set; }
    public GridManager grid;
    public Transform player;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate PathFinder in scene. Destroying extra.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        if (player != null)
        {
            RunReverseAStar(grid.WorldToGrid(player.position));
        }
    }

    void RunReverseAStar(Vector2Int start)
    {
        // Reset all nodes
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

            foreach (var neighbor in GetNeighbors(current))
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
        return 1f;
    }

    public List<Vector2> GetPathFrom(Vector2 worldPos)
    {
        Vector2Int gridPos = grid.WorldToGrid(worldPos);
        if (!grid.nodes.TryGetValue(gridPos, out var startNode))
            return new List<Vector2>();

        // If no path to player (cost is still MaxValue), return empty
        if (startNode.cost == float.MaxValue)
            return new List<Vector2>();

        List<Vector2> path = new();
        var current = startNode;

        // In reverse A*, cameFrom points toward the start (player)
        // So we follow cameFrom to get closer to the player
        while (current != null && current.cameFrom != null)
        {
            current = current.cameFrom;
            path.Add(grid.GridToWorld(current.position));
        }

        // Now path goes from enemy toward player
        // First waypoint is the next step, last waypoint is near the player

        // Debug output
        Debug.Log($"Generated path from {worldPos} to player with {path.Count} waypoints:");
        for (int i = 0; i < path.Count; i++)
        {
            Debug.Log($"Waypoint {i}: {path[i]}");
        }

        return path;
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
}

// Same Priority Queue implementation
public class PriorityQueue<T>
{
    private List<(T item, float priority)> elements = new();

    public int Count => elements.Count;

    public void Enqueue(T item, float priority)
    {
        elements.Add((item, priority));
        int childIndex = elements.Count - 1;
        while (childIndex > 0)
        {
            int parentIndex = (childIndex - 1) / 2;
            if (elements[childIndex].priority >= elements[parentIndex].priority)
                break;

            var temp = elements[childIndex];
            elements[childIndex] = elements[parentIndex];
            elements[parentIndex] = temp;

            childIndex = parentIndex;
        }
    }

    public T Dequeue()
    {
        if (elements.Count == 0)
            throw new System.InvalidOperationException("Queue is empty");

        var result = elements[0].item;
        elements[0] = elements[elements.Count - 1];
        elements.RemoveAt(elements.Count - 1);

        if (elements.Count > 0)
        {
            int parentIndex = 0;
            while (true)
            {
                int leftChild = 2 * parentIndex + 1;
                int rightChild = 2 * parentIndex + 2;
                int smallest = parentIndex;

                if (leftChild < elements.Count && elements[leftChild].priority < elements[smallest].priority)
                    smallest = leftChild;

                if (rightChild < elements.Count && elements[rightChild].priority < elements[smallest].priority)
                    smallest = rightChild;

                if (smallest == parentIndex)
                    break;

                var temp = elements[parentIndex];
                elements[parentIndex] = elements[smallest];
                elements[smallest] = temp;

                parentIndex = smallest;
            }
        }

        return result;
    }
}