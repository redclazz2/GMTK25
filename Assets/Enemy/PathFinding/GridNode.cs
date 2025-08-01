using UnityEngine;

public class GridNode
{
    public Vector2Int position;
    public Vector2 worldPosition;
    public bool walkable;
    public float cost = float.MaxValue;
    public GridNode cameFrom;
    public bool inClosedSet = false;
}
