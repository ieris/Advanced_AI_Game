using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour
{
    //Decision Making
    DecisionMaking dm;

    private float time = 1f;
    private int i = 0;

    //Waypoints
    Waypoint waypoint;

    public float rotationSpeed = 1f;
    public float walkingSpeed = 1.5f;
    public Vector3 currentWaypoint;
    public int waypointIndex = 0;

    public Transform seeker;
    public Transform target;
    public Transform player;

    public static bool startFollowingPath = false;

    public Grid grid;

    void Awake()
    {
        grid = GetComponent<Grid>();
        dm = GetComponent<DecisionMaking>();
    }

    void Start()
    {
        dm = new DecisionMaking();
        target = Waypoint.waypoints[waypointIndex];
        FindPath(seeker.position, target.position);
        
    }

    void Update()
    {
        if (startFollowingPath == true && DecisionMaking.aiState == DecisionMaking.States.Wander)
        {
            if (i == grid.path.Count)
            {
                i = 0;
                waypointIndex++;
                target = Waypoint.waypoints[waypointIndex];
                FindPath(seeker.position, target.position);
                //StartCoroutine("FollowThePath");
            }
            if (waypointIndex == Waypoint.waypoints.Length)
            {
                Debug.Log("ranOut");
                waypointIndex = 0;
                target = Waypoint.waypoints[waypointIndex];
                FindPath(seeker.position, target.position);
                //StopCoroutine("FollowThePath");
            }

            seeker.transform.LookAt(grid.path[i].worldPosition);
            seeker.transform.position = Vector3.MoveTowards(seeker.transform.position, grid.path[i].worldPosition, walkingSpeed * Time.deltaTime);
            if (Vector3.Distance(grid.path[i].worldPosition, seeker.transform.position) <= 0.1f)
            {
                i++;
            }

        }
        else if (startFollowingPath == false && DecisionMaking.aiState == DecisionMaking.States.Seek)
        {
            //target = player.transform;
            //FindPath(seeker.position, target.position);
            startFollowingPath = true;
        }
        else if (startFollowingPath == true && DecisionMaking.aiState == DecisionMaking.States.Seek)
        {
            Debug.Log("walk towards player");

            //Not in range
            if (!(Vector3.Distance(seeker.transform.position, player.transform.position) <= 4f))
            {
                seeker.transform.LookAt(player.position);
                seeker.transform.position = Vector3.MoveTowards(seeker.transform.position, player.position, walkingSpeed * Time.deltaTime);
            }
            //In range
            else
            {
                Debug.Log("in range: attack!");
                DecisionMaking.aiState = DecisionMaking.States.Attack;
                startFollowingPath = false;
            }
        }
        else if (startFollowingPath == false && DecisionMaking.aiState == DecisionMaking.States.Attack)
        {
            
            //He is now out of attack range, get in range again
            if (!(Vector3.Distance(seeker.transform.position, player.transform.position) <= 4f))
            {
                Debug.Log("He is now out of range, get in range again");

                //Make the AI pause before it gets close to the player again
                if (time >= 0)
                {
                    Debug.Log(time);
                    time -= Time.deltaTime;
                }
                //Get in range to the player
                else
                {                         
                    DecisionMaking.aiState = DecisionMaking.States.Seek;
                    startFollowingPath = true;
                    time = 1f;
                }                              
            }

            if(Vector3.Distance(seeker.transform.position, player.transform.position) > dm.visionRadius)
            {
                Debug.Log("I lost him!");
                //startFollowingPath = true;
                DecisionMaking.aiState = DecisionMaking.States.Search;
            }
        }
    }

    public void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Debug.Log("findpath");
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
                break;
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

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.graph_x - nodeB.graph_x);
        int dstY = Mathf.Abs(nodeA.graph_y - nodeB.graph_y);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
}
