using System;
using UnityEngine;

public class NodeAStar : IHeapItem<NodeAStar>
{
    public bool Walkable;
    public Vector3 WorldPosition;
    public int GridX; 
    public int GridY;
    
    // Initialize GCost to int.MaxValue so any new path is better
    public int GCost = int.MaxValue;
    public int HCost = 0;
    public NodeAStar Parent;

    public NodeAStar(bool walkable, Vector3 worldPosition, int gridX, int gridY)
    {
        Walkable = walkable;
        WorldPosition = worldPosition;
        GridX = gridX;
        GridY = gridY;
    }

    public int FCost => GCost + HCost;

    public int HeapIndex { get; set; }

    public int CompareTo(NodeAStar nodeToCompare)
    {
        // Compare by FCost, then HCost, returns -1 if this is less than nodeToCompare, 1 if greater, 0 if equal
        int compare = FCost.CompareTo(nodeToCompare.FCost);
        if (compare == 0)
        {
            compare = HCost.CompareTo(nodeToCompare.HCost);
        }
        // Flip the sign if we're in descending order
        return -compare;
    }
}
