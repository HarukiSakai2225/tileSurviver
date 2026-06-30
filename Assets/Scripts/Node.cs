using UnityEngine;
public class Node
{
    public Vector2Int GridPosition;
    public bool IsWalkable;
    public Node Parent;
    public int CCost;
    public int PCost;
    public int HCost;
    public int SCost;

    public Node(Vector2Int gridPosition, bool isWalkable, int pCost = 0)
    {
        GridPosition = gridPosition;
        IsWalkable = isWalkable;
        PCost = pCost;
    }
}
