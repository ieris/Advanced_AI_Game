using UnityEngine;
using System.Collections.Generic;

//Represents the position of each space in the game
public class Node
{
    //Keep track of previous nodes looked at and adjacent nodes
    public List<Node> adjacent = new List<Node>();
    public Node previous;
    public Vector3 worldPosition;

    public bool walkable;
    public int graph_x;
    public int graph_y;

    //Store distance and heuristic values
    public int g, h;
    public int f
    {
        get
        {
            return g + h;
        }
    }

    //Node constructor
    public Node(bool _walkable, Vector3 _worldPosition, int _graph_x, int _graph_y)
    {
        graph_x = _graph_x;
        graph_y = _graph_y;
        walkable = _walkable;
        worldPosition = _worldPosition;
    }


    //Allow to reset a node
    public void Clear()
    {
        previous = null;
    }
}