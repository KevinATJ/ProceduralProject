using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingGrid : MonoBehaviour
{
    public LayerMask obstacleMask;
    public Vector2 gridSize;
    public float nodeRadius;
    public bool displayGridGizmos;

    public Node[,] grid;
    private float nodeDiameter;
    public int gridCountX, gridCountY;

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridCountX = Mathf.RoundToInt(gridSize.x / nodeDiameter);
        gridCountY = Mathf.RoundToInt(gridSize.y / nodeDiameter);
        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new Node[gridCountX, gridCountY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridSize.x / 2 - Vector3.forward * gridSize.y / 2;

        for (int x = 0; x < gridCountX; x++)
        {
            for (int y = 0; y < gridCountY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) +
                                     Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool isObstacle = Physics.CheckSphere(worldPoint, nodeRadius, obstacleMask);
                grid[x, y] = new Node(worldPoint, x, y, isObstacle);
            }
        }
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridSize.x / 2) / gridSize.x;
        float percentY = (worldPosition.z + gridSize.y / 2) / gridSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridCountX - 1) * percentX);
        int y = Mathf.RoundToInt((gridCountY - 1) * percentY);
        return grid[x, y];
    }

    public Node GetRandomWalkableNode()
    {
        Node node;
        do
        {
            int x = Random.Range(0, gridCountX);
            int y = Random.Range(0, gridCountY);
            node = grid[x, y];
        } while (node.isObstacle);
        return node;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridSize.x, 1, gridSize.y));
        if (grid != null && displayGridGizmos)
        {
            foreach (Node node in grid)
            {
                Gizmos.color = (node.isObstacle) ? Color.red : Color.white;
                Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeDiameter - .1f));
            }
        }
    }
}

