using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour
{
    //Waypoints
    Waypoint waypoint;

    public float walkingSpeed = 10f;
    private Vector3 currentWaypoint;
    private int waypointIndex = 0;


    public Transform seeker;
    public Transform target;

    Grid grid;

    void Awake()
    {
        grid = GetComponent<Grid>();       
    }

    void Start()
    {
        target = Waypoint.waypoints[waypointIndex];
    }

    void Update()
    {
        FindPath(seeker.position, target.position);
    }

    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        List<Node> reachable; //Open
        List<Node> explored;  //Closed
        List<Node> path;

        reachable = new List<Node>();
        explored = new List<Node>();
        path = new List<Node>();

        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        reachable = new List<Node>();
        reachable.Add(startNode);

        while (reachable.Count > 0)
        {
            Node node = reachable[0];
            for (int i = 1; i < reachable.Count; i++)
            {
                if (reachable[i].f < node.f || reachable[i].f == node.f)
                {
                    if (reachable[i].h < node.h)
                        node = reachable[i];
                        Vector3 directionToWaypoint = (node.worldPosition - seeker.transform.position);
                        seeker.transform.Translate(directionToWaypoint * Time.deltaTime * 2f);

                        //The addition
                        /*for (int j = 0; j < path.Count; j++)
                        {
                            Debug.Log("hello " + j);
                            Vector3 directionToNextTile = (path[j].worldPosition - path[j].previous.worldPosition);
                            path[j].worldPosition = (directionToNextTile * Time.deltaTime);
                        }*/
                }
            }

            reachable.Remove(node);
            explored.Add(node);


            if (node == targetNode)
            {
                //Waypoint
                
                if (Vector3.Distance(seeker.transform.position, target.transform.position) <= 5f)
                {
                    //The addition
                    for(int i = 0; i < path.Count; i++)
                    {
                        Vector3 directionToNextTile = (path[i].worldPosition - path[i].previous.worldPosition);
                        path[i].worldPosition = (directionToNextTile * Time.deltaTime);
                    }                    

                    if (waypointIndex >= Waypoint.waypoints.Length)
                    {
                        waypointIndex = 0;
                    }

                    Debug.Log("waypoint index: " + waypointIndex);
                    waypointIndex++;
                    target = Waypoint.waypoints[waypointIndex];                                       
                }
                //End

                RetracePath(startNode, targetNode);
                return;
            }

            foreach (Node neighbour in grid.AddAdjacent(node))
            {
                if (!neighbour.walkable || explored.Contains(neighbour))
                {
                    continue;
                }

                int newCostToNeighbour = node.g + GetDistance(node, neighbour);
                if (newCostToNeighbour < neighbour.g || !reachable.Contains(neighbour))
                {
                    neighbour.g = newCostToNeighbour;
                    neighbour.h = GetDistance(neighbour, targetNode);
                    neighbour.previous = node;

                    if (!reachable.Contains(neighbour))
                        reachable.Add(neighbour);
                }
            }
        }
    }


    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {           
            path.Add(currentNode);
            currentNode = currentNode.previous;
        }

        path.Reverse();
        grid.path = path;
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.graph_x - nodeB.graph_x);
        int dstY = Mathf.Abs(nodeA.graph_y - nodeB.graph_y);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
}
