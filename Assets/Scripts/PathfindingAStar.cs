using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class PathfindingAStar : MonoBehaviour
{
    public Transform seeker;
    public Transform target;
    
    // Using a list to optimise the search, but aiming to use a heap instead
    private Heap<NodeAStar> _openSet;
    // Using hashes to optimise the search
    private HashSet<NodeAStar> _closedSet = new HashSet<NodeAStar>();
    
    GridAStar _grid;
    private bool _isInitialized = false;
    
    private void Awake()
    {
        _grid = GetComponent<GridAStar>();
    }

    private void Start()
    {
        // Initialise the heap after the grid has been created in Start
        InitialisePathfinding();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && _isInitialized)
        {
            FindPath(seeker.position, target.position);
        }
    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
        StartCoroutine(FindPath(startPos, targetPos));
    }
    
    private void InitialisePathfinding()
    {
        // Make sure _grid is initialised
        if (_grid.MaxSize > 0)
        {
            _openSet = new Heap<NodeAStar>(_grid.MaxSize);
            _isInitialized = true;
            Debug.Log($"Pathfinding initialized with heap size: {_grid.MaxSize}");
        }
        else
        {
            // _grid is not ready yet, try again later
            Debug.LogWarning("Grid not initialized yet. Will retry initialization later.");
            Invoke("InitialisePathfinding", 0.5f);
        }
    }

    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        // Safety check
        if (!_isInitialized || _openSet == null)
        {
            Debug.LogError("Pathfinding system not initialized yet. Cannot find path.");
            yield return null;
        }

        Stopwatch sw = new Stopwatch();
        sw.Start();
        
        // Clear collections instead of creating new ones
        if (_openSet != null)
        {
            _openSet.Clear();
            _closedSet.Clear();

            NodeAStar startNode = _grid.GetNodeFromWorldPoint(startPos);
            NodeAStar targetNode = _grid.GetNodeFromWorldPoint(targetPos);

            // Set initial G cost for start node to prevent potential issues
            startNode.GCost = 0;

            _openSet.Add(startNode);

            while (_openSet.Count > 0)
            {
                NodeAStar currentNode = _openSet.RemoveFirst();
                _closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    sw.Stop();
                    Debug.Log("Path found " + sw.ElapsedMilliseconds + "ms");
                    RetracePath(startNode, targetNode);
                    yield return null;
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
    }

    // Retrace the path after finding the end target node
    void RetracePath(NodeAStar startNode, NodeAStar endNode)
    {
        List<NodeAStar> path = new List<NodeAStar>();
        NodeAStar currentNode = endNode;
        
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.Parent;
        }
        path.Add(startNode); // This line includes the start node, can be removed if not needed
        path.Reverse();
        _grid.Path = path;
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
