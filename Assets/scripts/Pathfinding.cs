using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour
{
    //Waypoints
    Waypoint waypoint;

    public float rotationSpeed = 4f;
    public float walkingSpeed = 2f;
    public Vector3 currentWaypoint;
    public int waypointIndex = 0;

    public Transform seeker;
    public Transform target;

    private bool drawingPath = false;
    private bool startFollowingPath = false;

    Grid grid;

    void Awake()
    {
        grid = GetComponent<Grid>();
    }

    void Start()
    {
        target = Waypoint.waypoints[waypointIndex];
        FindPath(seeker.position, target.position);
    }

    void Update()
    {
        if(startFollowingPath)
        {
            FollowThePath();
        }
    }

    public void FindPath(Vector3 startPos, Vector3 targetPos)
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
                }
            }

            reachable.Remove(node);
            explored.Add(node);


            if (node == targetNode)
            {
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
        startFollowingPath = true;
    }

    void FollowThePath()
    {
        //Rotate to waypoint
        float t = 0f;
        float time = 20f;

        for (int i = 0; i < grid.path.Count; i++)
        {
            while (t < 1)
            {
                t += Time.deltaTime / time;

                //Debug.Log(grid.path[i].worldPosition);
                seeker.transform.LookAt(grid.path[i].worldPosition);
                //float distanceToNextNode = Vector3.Distance(seeker.transform.position, grid.path[i].worldPosition);
                seeker.transform.position = Vector3.Slerp(seeker.transform.position, grid.path[i].worldPosition, time);
                //Vector3 directionToNextNodePoint = grid.path[i].worldPosition - seeker.transform.position;
                //seeker.transform.rotation = Quaternion.Slerp(seeker.transform.rotation, Quaternion.LookRotation(directionToNextNodePoint), rotationSpeed);
                //seeker.transform.position = Vector3.MoveTowards(seeker.transform.position, grid.path[i].worldPosition, Time.deltaTime / walkingSpeed);
                Debug.Log(seeker.transform.position);
            }

            t = 0;
        }

        if (Vector3.Distance(Waypoint.waypoints[waypointIndex].transform.position, transform.position) < 3.0f)
        {
            Debug.Log("success!");
            waypointIndex++;
            Debug.Log(waypointIndex);

            if (waypointIndex >= Waypoint.waypoints.Length)
            {
                Debug.Log("ranOut");
                waypointIndex = 0;
            }
        }
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
