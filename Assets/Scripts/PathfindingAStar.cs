using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingAStar : MonoBehaviour
{
    PathRequestManager _pathRequestManager;
    GridAStar _grid;
    
    // Using a list to optimise the search, but aiming to use a heap instead
    private Heap<NodeAStar> _openSet;
    // Using hashes to optimise the search
    private HashSet<NodeAStar> _closedSet = new HashSet<NodeAStar>();
    
    private void Awake()
    {
        _pathRequestManager = GetComponent<PathRequestManager>();
        _grid = GetComponent<GridAStar>();
        _openSet = new Heap<NodeAStar>(_grid.MaxSize);
    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
        StartCoroutine(FindPath(startPos, targetPos));
    }

    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Vector3[] waypoints = Array.Empty<Vector3>();
        bool pathSuccess = false;
        
        // Clear collections instead of creating new ones
        _openSet.Clear();
        _closedSet.Clear();

        NodeAStar startNode = _grid.GetNodeFromWorldPoint(startPos);
        NodeAStar targetNode = _grid.GetNodeFromWorldPoint(targetPos);

        // Check if startNode and targetNode are walkable before continuing
        if (startNode.Walkable && targetNode.Walkable)
        {
            // Set initial G cost for start node
            startNode.GCost = 0;
            _openSet.Add(startNode);

            while (_openSet.Count > 0)
            {
                NodeAStar currentNode = _openSet.RemoveFirst();
                _closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    pathSuccess = true;
                    break;
                }

                // Check all neighbours of the currentNode and add them to the openSet if walkable and not in the _closedSet
                foreach (var neighbour in _grid.GetNeighbours(currentNode))
                {
                    if (!neighbour.Walkable || _closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentNode.GCost + GetDistance(currentNode, neighbour);
                    if (newMovementCostToNeighbour < neighbour.GCost || !_openSet.Contains(neighbour))
                    {
                        neighbour.GCost = newMovementCostToNeighbour;
                        neighbour.HCost = GetDistance(neighbour, targetNode);
                        neighbour.Parent = currentNode;

                        if (!_openSet.Contains(neighbour))
                        {
                            _openSet.Add(neighbour);
                        }
                        else
                        {
                            // If already in open set, update its position in the heap
                            _openSet.UpdateItem(neighbour);
                        }
                    }
                }
            }
        }
        yield return null;
        if (pathSuccess)
        {
            waypoints = RetracePath(startNode, targetNode);
        }
        _pathRequestManager.FinishedProcessingPath(waypoints, pathSuccess);
    }

    // Retrace the path after finding the end target node
    Vector3[] RetracePath(NodeAStar startNode, NodeAStar endNode)
    {
        List<NodeAStar> path = new List<NodeAStar>();
        NodeAStar currentNode = endNode;
        
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.Parent;
        }

        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
    }

    Vector3[] SimplifyPath(List<NodeAStar> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i-1].GridX - path[i].GridX, path[i-1].GridY - path[i].GridY);
            if (directionNew != directionOld)
            {
                waypoints.Add(path[i].WorldPosition);
            }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
    }
    
    // Calculate the distance between 2 nodes
    int GetDistance(NodeAStar nodeA, NodeAStar nodeB)
    {
        int distanceX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
        int distanceY = Mathf.Abs(nodeA.GridY - nodeB.GridY);
        
        if (distanceX > distanceY)
            return 14 * distanceY + 10 * (distanceX - distanceY);
        return 14 * distanceX + 10 * (distanceY - distanceX);
        
    }
}
