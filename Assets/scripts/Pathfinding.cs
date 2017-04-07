using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour
{
    //Decision Making
    DecisionMaking dm;
    StationaryGuard statGuard;

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
    public Transform stationaryGuard;

    public static bool startFollowingPath = false;

    public Grid grid;

    void Awake()
    {
        //References to external scripts
        grid = GetComponent<Grid>();
        dm = GetComponent<DecisionMaking>();
        statGuard = GetComponent<StationaryGuard>();
    }

    void Start()
    {
        dm = new DecisionMaking();
        statGuard = new StationaryGuard();
        target = Waypoint.waypoints[waypointIndex];
        FindPath(seeker.position, target.position);

    }

    void Update()
    {
        //If start following path is true, it runs the a* algorithm
        //Checks if guard wandering
        if (startFollowingPath == true && DecisionMaking.aiState == DecisionMaking.States.Wander)
        {
            //check if the path has been travelled, if yes set the target to the next waypoint
            if (i == grid.path.Count)
            {
                i = 0;
                waypointIndex++;
                target = Waypoint.waypoints[waypointIndex];
                FindPath(seeker.position, target.position);
                //StartCoroutine("FollowThePath");
            }
            //reset the waypoints to 0 and set the path to the beginning
            if (waypointIndex == Waypoint.waypoints.Length)
            {
                Debug.Log("ranOut");
                waypointIndex = 0;
                target = Waypoint.waypoints[waypointIndex];
                FindPath(seeker.position, target.position);
                //StopCoroutine("FollowThePath");
            }

            //move the guard to next node in the grid
            seeker.transform.LookAt(grid.path[i].worldPosition);
            seeker.transform.position = Vector3.MoveTowards(seeker.transform.position, new Vector3(grid.path[i].worldPosition.x, 0, grid.path[i].worldPosition.z), walkingSpeed * Time.deltaTime);
            //when the node is reached, to the next one
            if (Vector3.Distance(grid.path[i].worldPosition, seeker.transform.position) <= 0.1f)
            {
                i++;
            }

        }
        //if the guard is seeking: trying to get to the player
        else if (DecisionMaking.aiState == DecisionMaking.States.Seek)
        {
            Debug.Log("Seeking");
            //Guard is not in range
            if (!(Vector3.Distance(seeker.transform.position, player.transform.position) <= 4f))
            {
                Debug.Log("Get back here!");
                //Move closer to the player if he walked off
                seeker.transform.LookAt(player.position);
                seeker.transform.position = Vector3.MoveTowards(seeker.transform.position, new Vector3(player.position.x, 0, player.position.z), walkingSpeed * Time.deltaTime);
                startFollowingPath = true;
            }
            //In range
            else
            {
                //Now guard can attack the player
                Debug.Log("in range: attack!");
                DecisionMaking.aiState = DecisionMaking.States.Attack;
                startFollowingPath = true;
            }
            //if player goes out of the vision radius, he is lost
            if ((Vector3.Distance(seeker.transform.position, player.transform.position) > dm.visionRadius))
            {
                //search for him
                Debug.Log("He's gone!?");
                DecisionMaking.aiState = DecisionMaking.States.Search;
                dm.seen = false;
            }
        }
        //if guard is in attack mode
        else if (DecisionMaking.aiState == DecisionMaking.States.Attack)
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
                    time = 1f;
                }
            }
            //if player goes out of the vision radius, he is lost
            if (Vector3.Distance(seeker.transform.position, player.transform.position) > dm.visionRadius)
            {
                dm.seen = false;
                Debug.Log("I lost him!");
                DecisionMaking.aiState = DecisionMaking.States.Search;
            }
        }
        //If guard is running away
        else if (startFollowingPath == true && DecisionMaking.aiState == DecisionMaking.States.Flee)
        {
            //reset the pathfinding target to stationary guards position
            if (target != null)
            {
                target = null;
                i = 0;
                target = stationaryGuard;
                FindPath(seeker.position, target.position);
            }

            //move to next node in path
            seeker.transform.LookAt(grid.path[i].worldPosition);
            seeker.transform.position = Vector3.MoveTowards(seeker.transform.position, new Vector3(grid.path[i].worldPosition.x, 0, grid.path[i].worldPosition.z), walkingSpeed * Time.deltaTime);

            if ((Vector3.Distance(grid.path[i].worldPosition, seeker.transform.position) <= 0.1f) && DecisionMaking.safe == false)
            {
                i++;
            }

            //the guard is now safe
            if (DecisionMaking.safe)
            {
                i = 0;
                target = null;
                startFollowingPath = false;
            }
        }

        //Stationary Guard logic

        //If seeking
        if (StationaryGuard.aiState == StationaryGuard.States.Seek)
        {
            //Not in range
            if (!(Vector3.Distance(stationaryGuard.transform.position, player.transform.position) <= 4f))
            {
                stationaryGuard.transform.LookAt(player.position);
                stationaryGuard.transform.position = Vector3.MoveTowards(stationaryGuard.transform.position, new Vector3(player.position.x, 0, player.position.z), walkingSpeed * Time.deltaTime);
                StationaryGuard.aiState = StationaryGuard.States.Search;
                startFollowingPath = true;
            }
            //In range
            else
            {
                Debug.Log("in range: attack!");
                StationaryGuard.aiState = StationaryGuard.States.Attack;
                startFollowingPath = true;
            }
        }
        else if (StationaryGuard.aiState == StationaryGuard.States.Attack)
        {
            //He is now out of attack range, get in range again
            if (!(Vector3.Distance(stationaryGuard.transform.position, player.transform.position) <= 4f))
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
                    StationaryGuard.aiState = StationaryGuard.States.Seek;
                    time = 1f;
                }
            }

            if (Vector3.Distance(stationaryGuard.transform.position, player.transform.position) > statGuard.visionRadius)
            {
                Debug.Log("I lost him!");
                DecisionMaking.aiState = DecisionMaking.States.Search;
            }
        }
        //If the intruder wasnt seen, get back to your post
        else if (StationaryGuard.aiState == StationaryGuard.States.ReturnToPost)
        {
            Debug.Log("return to post");
            if (target != null)
            {
                target = null;
                i = 0;
            }
            
            //find path to post
            FindPath(stationaryGuard.position, StationaryGuard.originalPosition);
            stationaryGuard.transform.LookAt(grid.path[i].worldPosition);
            stationaryGuard.transform.position = Vector3.MoveTowards(stationaryGuard.transform.position, new Vector3(grid.path[i].worldPosition.x, 0, grid.path[i].worldPosition.z), walkingSpeed * Time.deltaTime);

            if ((Vector3.Distance(grid.path[i].worldPosition, stationaryGuard.transform.position) <= 0.1f) && DecisionMaking.safe == false)
            {
                i++;
            }

            if (DecisionMaking.safe)
            {
                i = 0;
                target = null;
                startFollowingPath = false;
            }
        }

        //if the location to intruders last seen location is far, make a path to it 
        else if(StationaryGuard.aiState == StationaryGuard.States.Search && StationaryGuard.farLastSeenLocation == true)
        {
            i = 0;
            target.position = StationaryGuard.lastSeenLocation;
            FindPath(stationaryGuard.position, target.position);
            stationaryGuard.transform.LookAt(grid.path[i].worldPosition);
            stationaryGuard.transform.position = Vector3.MoveTowards(stationaryGuard.transform.position, new Vector3(grid.path[i].worldPosition.x, 0, grid.path[i].worldPosition.z), walkingSpeed * Time.deltaTime);

            if ((Vector3.Distance(grid.path[i].worldPosition, stationaryGuard.transform.position) <= 0.1f) && DecisionMaking.safe == false)
            {
                i++;
            }
        }

        //if guard is helping, go to the last seen location of intruder
        if (DecisionMaking.safe == true && StationaryGuard.aiState == StationaryGuard.States.Help)
        {
            Debug.Log("helping");
            FindPath(stationaryGuard.position, DecisionMaking.lastSeenLocation);

            if (startFollowingPath && (statGuard.seen == false))
            {
                stationaryGuard.transform.LookAt(grid.path[i].worldPosition);
                stationaryGuard.transform.position = Vector3.MoveTowards(stationaryGuard.transform.position, new Vector3(grid.path[i].worldPosition.x, 0, grid.path[i].worldPosition.z), walkingSpeed * Time.deltaTime);
                if (Vector3.Distance(grid.path[i].worldPosition, stationaryGuard.transform.position) <= 1f)
                {
                    i++;
                }
            }
            else if(statGuard.seen == true)
            {
                startFollowingPath = false;
            }
        }
    }

    public void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        //store the nodes in the lists
        List<Node> reachable; //Open
        List<Node> explored;  //Closed
        List<Node> path;

        reachable = new List<Node>();
        explored = new List<Node>();
        path = new List<Node>();

        //clear the path if it is not empty
        path.Clear();

        //convert node position to world position
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        //add first node to the list
        reachable = new List<Node>();
        reachable.Add(startNode);

        //check which nodes are reachable and compare the cheapest ones
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

            //place the chosen node to the explored list and remove from reachable
            reachable.Remove(node);
            explored.Add(node);

            //we found the path
            if (node == targetNode)
            {
                RetracePath(startNode, targetNode);
                break;
            }

            //find the neighbours and get their costs
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

    //walk back the path
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

    //calculates the distance from the nodes
    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.graph_x - nodeB.graph_x);
        int dstY = Mathf.Abs(nodeA.graph_y - nodeB.graph_y);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
}