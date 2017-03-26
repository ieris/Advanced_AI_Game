using UnityEngine;
using System.Collections.Generic;
using System;

public class Guard_Movement : MonoBehaviour
{
    //Pathfinding start
    public Map map;
    public List<Node> reachable; //Open
    public List<Node> explored;  //Closed
    public List<Node> path;

    //Tracks how many iterations have been completed for debuging purposes
    public int iterations;
    public bool finished;

    //Waypoints
    Waypoint waypoint;

    public float walkingSpeed = 10f;
    public float rotationSpeed = 2f;
    private Vector3 currentWaypoint;
    private int waypointIndex = 0;

    public Transform start;
    public Transform target;

    //Constructor : takes a graph
    public Guard_Movement(Map map)
    {
        this.map = map;
    }

    void Awake()
    {
        map = GetComponent<Map>();
    }

    void Start()
    {
        //start = astar.start;
        target = Waypoint.waypoints[0];
    }

    void Update()
    {
        //Debug.Log("start position: " + start.position);
        //Debug.Log("target position: " + target.transform.position);

        if (Vector3.Distance(start.position, target.position) <= 0.2f)
        {
            waypointIndex++;
            target = Waypoint.waypoints[waypointIndex];

            if (waypointIndex >= Waypoint.waypoints.Length)
            {
                waypointIndex = 0;
            }
        }

        Vector3 directionToWayPoint = Waypoint.waypoints[waypointIndex].transform.position - transform.position;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionToWayPoint), rotationSpeed * Time.deltaTime);
        //start.transform.Translate(0, 0, Time.deltaTime * walkingSpeed);
        Step(start.position, target.position);
        //transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, walkingSpeed * Time.deltaTime);
    }

    //Create the search method which takes in a start and target node
    //Checks possible moves that can be made
    public void Step(Vector3 startPosition, Vector3 targetPosition)
    {
        Vector3[] waypoint = new Vector3[0];
        reachable = new List<Node>();
        explored = new List<Node>();
        path = new List<Node>();

        //Vector3 startWorldPos = graph.NodeFromWorldPoint(startPosition);
        //Vector3 targetWorldPos = graph.NodeFromWorldPoint(targetPosition);

        Node startNode = map.NodeFromWorldPoint(startPosition);
        Node targetNode = map.NodeFromWorldPoint(targetPosition);

        //Debug.Log("star node position world point " + startNode.worldPosition);
        //Debug.Log("target node position world point " + targetNode.worldPosition);

        //Add the start node to the reachable/open list
        reachable.Add(startNode);

        //Create the explored/closed list and path list        
        iterations = 0;

        //Clear the graph in case we have ran this previously
        /*for (var i = 0; i < graph.nodes.Length; i++)
        {
            graph.nodes[i].Clear();
        }*/

        if (path.Count > 0)
        {
            return;
        }

        //Check if we ran out of options
        if (reachable.Count == 0)
        {
            finished = true;
            return;
        }

        //Track number of iterations for performance purposes
        iterations++;

        //Pick a node to start the search from
        var currentNode = ChooseNode();

        //Check if the node is the target node
        //Add the node to the path and set the node as the previous node
        if (currentNode == targetNode)
        {
            while (currentNode != null)
            {
                path.Insert(0, currentNode);
                currentNode = currentNode.previous;
            }
            //RetracePath(startNode, targetNode);
            finished = true;
            return;
        }

        //Remove the current node from the open list
        //Add it in the closed list
        reachable.Remove(currentNode);
        explored.Add(currentNode);


        //Iterate through adjacent nodes
        //For all values, add adjacent nodes
        for (var i = 0; i < currentNode.adjacent.Count; i++)
        {
            AddAdjacent(currentNode, currentNode.adjacent[i]);
        }

        foreach (Node adjacent in currentNode.adjacent)
        {
            if (explored.Contains(adjacent))
            {
                continue;
            }

            int newCostToNeighbour = currentNode.g + calculateDistance(currentNode, adjacent);
            if (newCostToNeighbour < adjacent.g || !reachable.Contains(adjacent))
            {
                adjacent.g = newCostToNeighbour;

                Debug.Log("adjacent.g : " + adjacent.g);
                adjacent.h = calculateDistance(adjacent, targetNode);
                adjacent.previous = currentNode;

                if (!reachable.Contains(adjacent))
                    reachable.Add(adjacent);
            }
        }

        waypoint = RetracePath(startNode, targetNode);
        start.transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, walkingSpeed * Time.deltaTime);
    }

    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.previous;
        }
        //path.Reverse();
        //map.path = path;
        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;

    }

    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].graph_x - path[i].graph_y, path[i - 1].graph_y - path[i].graph_y);
            if (directionNew != directionOld)
            {
                //waypoints.Add(path[i].worldPosition);
                waypoints.Add(path[i].worldPosition);
            }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
    }

    //Loops through all adjacent nodes and finds next
    //available options. Makes that node available (open List)
    //and creates connection at previous node.
    public void AddAdjacent(Node node, Node adjacent)
    {
        //If found, we return the node and we have found a new path
        if (FindNode(adjacent, explored) || FindNode(adjacent, reachable))
        {
            return;
        }

        //Set the previous node from adj to current node
        //Add adj node to open list
        adjacent.previous = node;
        reachable.Add(adjacent);
    }

    //Finds a node in the list
    public bool FindNode(Node node, List<Node> list)
    {
        return getNodeIndex(node, list) >= 0;
    }

    //Tests is node is in the list
    public int getNodeIndex(Node node, List<Node> list)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (node == list[i])
            {
                return i;
            }
        }

        return -1;
    }

    int calculateDistance(Node a, Node b)
    {
        int distance_x = Mathf.Abs(a.graph_x - b.graph_x);
        int distance_y = Mathf.Abs(a.graph_y - b.graph_y);

        if (distance_x > distance_y)
            return 14 * distance_y + 10 * (distance_x - distance_y);
        return 14 * distance_x + 10 * (distance_y - distance_x);
    }

    public Node ChooseNode()
    {
        var currentNode = new Node();

        //Choose node with the lowest f score and check the lowest h also
        while (reachable.Count > 0)
        {
            currentNode = reachable[0];
            for (int i = 0; i < reachable.Count; i++)
            {
                if (reachable[i].f < currentNode.f || reachable[i].f == currentNode.f && reachable[i].h < currentNode.h)
                {
                    if (reachable[i].h < currentNode.h)
                    {
                        currentNode = reachable[i];
                    }
                }
            }

            reachable.Remove(currentNode);
            explored.Add(currentNode);

            return currentNode;
        }

        return currentNode;
    }
}
