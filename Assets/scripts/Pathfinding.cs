using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour
{
    //Waypoints
    Waypoint waypoint;

    public float rotationSpeed = 0.0002f;
    public float walkingSpeed = 0.0002f;
    public Vector3 currentWaypoint;
    public int waypointIndex = 0;


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

                        //Move the guard through the nodes along the path
                        //Vector3 directionToWaypoint = (node.worldPosition - seeker.transform.position);
                       // seeker.transform.Translate(directionToWaypoint * Time.deltaTime * 30f);

                    //Move the guard through the nodes along the path
                    //Vector3 directionToFurtherWaypoint = (node.adjacent[i].worldPosition - seeker.transform.position);
                    //seeker.transform.Translate(directionToFurtherWaypoint * Time.deltaTime * 0.1f);


                    //The addition
                    /*for (int j = 0; j < path.Count; j++)
                    {
                        Debug.Log("hello " + j);
                        Vector3 directionToNextTile = (path[j].worldPosition - path[j].previous.worldPosition);
                        path[j].worldPosition = (directionToNextTile * Time.deltaTime);
                    }*/
                }
            }

            //Vector3 directionToFurtherWaypoint = (node.worldPosition - seeker.transform.position);
            //seeker.transform.Translate(directionToFurtherWaypoint * Time.deltaTime * 30f);

            reachable.Remove(node);
            explored.Add(node);


            if (node == targetNode)
            {
                //waypointIndex++;
                //Debug.Log("guard position: " + seeker.transform.position);
                //Waypoint

                /*foreach(Node n in path)
                {
                    Vector3 directionToFurtherWaypoint = (n.worldPosition - seeker.transform.position);
                    seeker.transform.Translate(directionToFurtherWaypoint * Time.deltaTime * 0.1f);
                }*/

                //if (Vector3.Distance(seeker.transform.position, target.transform.position) < 1f)
                //{
                //The addition
                /*for(int i = 0; i < path.Count; i++)
                {
                    Vector3 directionToNextTile = (path[i].worldPosition - path[i].previous.worldPosition);
                    path[i].worldPosition = (directionToNextTile * Time.deltaTime * 1f);
                }*/

                /*if (waypointIndex >= Waypoint.waypoints.Length)
                {
                    waypointIndex = 0;
                }*/

                //Debug.Log("waypoint index: " + waypointIndex);
                //waypointIndex++;
                //target = Waypoint.waypoints[waypointIndex];                                       
                //}
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

        

        /*for (int i = 0; i < path.Count; i++)
        {
            directionToNextNode = (path[i].worldPosition - seeker.transform.position);
            seeker.transform.Translate(directionToNextNode * Time.deltaTime * 0.1f);
        }*/

        Node currentNode = endNode;

        while (currentNode != startNode)
        {        
            //if( currentNode.previous != null)
            //{
                path.Add(currentNode);
            //Vector3 directionToNextNode = (currentNode.worldPosition - currentNode.previous.worldPosition);
            //Debug.Log("next node is at: " + currentNode.worldPosition);
            //seeker.transform.position = Vector3.Lerp(seeker.transform.position, currentNode.worldPosition, Time.deltaTime * 0.01f);

                if (Vector3.Distance(Waypoint.waypoints[waypointIndex].transform.position, transform.position) < 3.0f)
                {
                    //Debug.Log("success!");
                    waypointIndex++;
                    Debug.Log(waypointIndex);

                    if (waypointIndex >= Waypoint.waypoints.Length)
                    {
                        Debug.Log("ranOut");
                        waypointIndex = 0;
                    }
                }

                //Rotate to waypoint
                Vector3 directionToNextNodePoint = currentNode.worldPosition - seeker.transform.position;
                seeker.transform.rotation = Quaternion.Slerp(seeker.transform.rotation, Quaternion.LookRotation(directionToNextNodePoint), rotationSpeed);
                seeker.transform.position = Vector3.MoveTowards(seeker.transform.position, currentNode.worldPosition, Time.deltaTime* walkingSpeed);

                currentNode = currentNode.previous;
            //}
            //else
            //{
                //Vector3 directionToNextNode = (currentNode.worldPosition - startNode.worldPosition);
                //Debug.Log("1 next node is at: " + currentNode.worldPosition);
                //seeker.transform.position = Vector3.Lerp(seeker.transform.position, currentNode.worldPosition, Time.deltaTime * 0.01f);
            //}
        }
        
        path.Reverse();       
        grid.path = path;

        /*foreach(Node n in path)
        {
            transform.position = Vector3.MoveTowards(transform.position, path[0].worldPosition, Time.deltaTime * 0.2f);
        }*/
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
