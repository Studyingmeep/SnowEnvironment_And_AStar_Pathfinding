using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class GridAStar : MonoBehaviour
{
    public bool displayGridGizmos;
    public LayerMask unwalkableMask;

    private LayerMask _walkableMask;
    // Recommend keeping this less than 100x100
    public Vector2 gridWorldSize;
    private int _gridSizeX, _gridSizeY;
    public int obstacleProximityPenalty = 10;

    private int penaltyMin = int.MaxValue;
    private int penaltyMax = int.MinValue;
    
    public TerrainType[] walkableRegions;
    private Dictionary<int, int> _walkableRegionsDict = new Dictionary<int, int>();
    
    [Range(0.1f, 10)] // Value needs to be minimum 0.1f to avoid overflowing, floats are rounded to int during run-time.
    public float nodeRadius; 
    private float _nodeDiameter;
    
    private NodeAStar[,] _grid; // Our 2D Array (grid) of nodes

    void Awake()
    {
        _nodeDiameter = nodeRadius * 2;
        _gridSizeX = Mathf.RoundToInt(gridWorldSize.x / _nodeDiameter);
        _gridSizeY = Mathf.RoundToInt(gridWorldSize.y / _nodeDiameter);

        foreach (TerrainType region in walkableRegions)
        {
            _walkableMask.value |= region.terrainMask.value;
            _walkableRegionsDict.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty);
        }
        
        CreateGrid();
    }

    public int MaxSize => _gridSizeX * _gridSizeY;

    // Creating the initial grid of nodes, based on our NodeAStar class
    void CreateGrid()
    {
        _grid = new NodeAStar[_gridSizeX, _gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        // Loop through all nodes in the grid and create them, first the y-nodes, then the x-nodes
        for (int x = 0; x < _gridSizeX; x++)
        {
            for (int y = 0; y < _gridSizeY; y++)
            {
                // Calculate the world position of each node, and then check if it's walkable or not'
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * _nodeDiameter + nodeRadius) + Vector3.forward * (y * _nodeDiameter + nodeRadius);
                bool walkable = !Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask);
                
                int movementPenalty = 0;
                
                Ray ray = new Ray(worldPoint + Vector3.up * 250, Vector3.down);
                if (Physics.Raycast(ray, out var hit, 500, _walkableMask))
                {
                    _walkableRegionsDict.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                }

                if (!walkable)
                {
                    movementPenalty += obstacleProximityPenalty;
                }
                
                _grid[x, y] = new NodeAStar(walkable, worldPoint, x, y, movementPenalty);
            }
        }
        
        BlurPenaltyMap(3);
    }

    void BlurPenaltyMap(int blurSize)
    {
        int kernelSize = blurSize * 2 + 1;
        int kernalExtents = (kernelSize - 1) / 2;

        int[,] penaltiesHorizontalPass = new int[_gridSizeX, _gridSizeY];
        int[,] penaltiesVerticalPass = new int[_gridSizeX, _gridSizeY];

        for (int y = 0; y < _gridSizeY; y++)
        {
            for (int x = -kernalExtents; x <= kernalExtents; x++)
            {
                int sampleX = Mathf.Clamp(x, 0, kernalExtents);
                penaltiesHorizontalPass[0, y] += _grid[sampleX, y].movementPenalty;
            }

            for (int x = 1; x < _gridSizeX; x++)
            {
                int removeIndex = Mathf.Clamp(x - kernalExtents - 1, 0, _gridSizeX);
                int addIndex = Mathf.Clamp(x + kernalExtents, 0, _gridSizeX - 1);

                penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - _grid[removeIndex, y].movementPenalty + _grid[addIndex, y].movementPenalty;
            }
        }
        for (int x = 0; x < _gridSizeX; x++)
        {
            for (int y = -kernalExtents; y <= kernalExtents; y++)
            {
                int sampleY = Mathf.Clamp(y, 0, kernalExtents);
                penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
            }
            
            int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
            _grid[x,0].movementPenalty = blurredPenalty;

            for (int y = 1; y < _gridSizeY; y++)
            {
                int removeIndex = Mathf.Clamp(y - kernalExtents - 1, 0, _gridSizeY);
                int addIndex = Mathf.Clamp(y + kernalExtents, 0, _gridSizeY - 1);

                penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
                blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
                _grid[x,y].movementPenalty = blurredPenalty;

                if (blurredPenalty > penaltyMax)
                {
                    penaltyMax = blurredPenalty;
                }
                if (blurredPenalty < penaltyMin)
                {
                    penaltyMin = blurredPenalty;   
                }
            }
        }
}
    
    public List<NodeAStar> GetNeighbours(NodeAStar node)
    {
        List<NodeAStar> neighbours = new List<NodeAStar>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;
                
                int checkX = node.GridX + x;
                int checkY = node.GridY + y;
                
                if (checkX >= 0 && checkX < _gridSizeX && checkY >= 0 && checkY < _gridSizeY)
                {
                    neighbours.Add(_grid[checkX, checkY]);
                }
            }
        }
        
        return neighbours;
    }

    public NodeAStar GetNodeFromWorldPoint(Vector3 worldPosition)
    {
        // InverseLerp works great when we know the worldPosition but want to know the % (where it fits in the grid)
        // InverseLamp already clamps the values.
        float percentX = Mathf.InverseLerp(-gridWorldSize.x * 0.5f, gridWorldSize.x * 0.5f, worldPosition.x);
        float percentY = Mathf.InverseLerp(-gridWorldSize.y * 0.5f, gridWorldSize.y * 0.5f, worldPosition.z); // Note: Using z for Y in world space

        // Then we use the percentages to get the x and y values of the node, clamping again to avoid edge cases
        int x = Mathf.Clamp(Mathf.FloorToInt(percentX * _gridSizeX), 0, _gridSizeX - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(percentY * _gridSizeY), 0, _gridSizeY - 1);

        return _grid[x, y];
    }
    
    // Debug function to draw the grid in the scene view
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
        
        if (_grid != null && displayGridGizmos)
        {
            foreach (NodeAStar n in _grid)
            {
                Gizmos.color = Color.Lerp(Color.white, Color.black,
                    Mathf.InverseLerp(penaltyMin, penaltyMax, n.movementPenalty));
                
                Gizmos.color = n.Walkable ? Gizmos.color : Color.red;
                Gizmos.DrawCube(n.WorldPosition, Vector3.one * (_nodeDiameter));
            }
        };
    }
}

[System.Serializable]
public class TerrainType
{
    public LayerMask terrainMask;
    public int terrainPenalty;
}
