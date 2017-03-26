using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//A graph is used to link the nodes together
public class Map : MonoBehaviour
{
    public int rows = 0;
    public int columns = 0;
    public Node[] nodes;

    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    Node[,] grid;

    float nodeDiameter;
    public int gridSizeX, gridSizeY;

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        //CreateGrid();

    }
    //constructor: accepts 2d array
    public Map(int[,] grid)
    {
        rows = grid.GetLength(0);
        columns = grid.GetLength(1);

        //Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        //Create an empty node for each space in the graph
        nodes = new Node[grid.Length];

        //grid = new Node[gridSizeX, gridSizeY];
        for (var i = 0; i < grid.Length; i++)
        {
            //Vector3 worldPoint = worldBottomLeft + Vector3.right * (i * nodeDiameter + nodeRadius) + Vector3.forward * (j * nodeDiameter + nodeRadius);
            //bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));

            var node = new Node();
            node.label = i.ToString();
            nodes[i] = node;
        }

        //Fill the array with empty nodes and label each node with its position
        for (var r = 0; r < rows; r++)
        {
            for (var c = 0; c < columns; c++)
            {
                var node = nodes[columns * r + c];

                //Open node is represented by 0
                //Closed node is represented by 1
                if (grid[r, c] == 1)
                {
                    continue;
                }

                //Up
                //Add any node above the current one
                if (r > 0)
                {
                    node.adjacent.Add(nodes[columns * (r - 1) + c]);
                }

                //Right
                //Looks for any node to the right of the current node
                if (c < columns - 1)
                {
                    node.adjacent.Add(nodes[columns * r + c + 1]);
                }

                //Down
                //Looks for any nodes below the current one
                if (r < rows - 1)
                {
                    node.adjacent.Add(nodes[columns * (r + 1) + c]);
                }

                //Left
                //Looks for any nodes to the left of the current node
                if (c > 0)
                {
                    node.adjacent.Add(nodes[columns * r + c - 1]);
                }
            }
        }
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                grid[x, y] = new Node();
            }
        }
    }

    public List<Node> path;
    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = (n.walkable) ? Color.white : Color.red;
                if (path != null)
                    if (path.Contains(n))
                        Gizmos.color = Color.black;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .1f));
            }
        }
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {

        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        worldPosition.x = x;
        worldPosition.y = y;
        worldPosition.z = 1;

        var worldPositionNode = new Node();
        worldPositionNode.worldPosition = worldPosition;

        return worldPositionNode;
    }
}