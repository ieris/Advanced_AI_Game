using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class DecisionMaking : MonoBehaviour
{
    public LineRenderer lineRender;
    public LayerMask playerMask;
    public LayerMask walls;

    public Transform head;
    public Transform aiPos;
    public List<Transform> visibleTargets = new List<Transform>();

    public int health = 100;
    public int damage = 10;
    public float speed = 1.5f;
    public int visionRadius = 20;
    public int visionAngle = 120;
    public int audioRange = 40;
    public float rotationSpeed = 0.2f;
    
    public States aiState = States.Wander;

    public Transform player;
    public Vector3 directionToPlayer;
    public float distanceToTarget;
    public float angleToPlayer;

    public GameObject[] waypoints;
    public int currentWaypoint = 0;
    public Vector3 directionToWaypoint;
    public float angleToWaypoint;

    void Awake()
    {

    }

	void Start ()
    {
        
        lineRender = GetComponent<LineRenderer>();
        lineRender.SetWidth(0.05f, 0.05f);
        lineRender.SetColors(Color.red, Color.red);

    }
	
	void Update ()
    {
        //Wandering();
        //watching();
        switch (aiState)
        {
            case States.Wander:
                //Debug.Log("Wandering");
                //Wandering();
                //execute closed code & listener for interaction
                break;
            case States.Search:
                Debug.Log("Searching");
                Searching();
                //execute code for animating the door open, switch to open when done
                break;
            case States.Seek:
                Debug.Log("Seeking");
                Seeking();
                //listening code for event to close door
                break;
            case States.Attack:
                Debug.Log("Attacking");
                Attacking();
                //code to animate door closed and switch to closed state
                break;
            case States.Flee:
                Debug.Log("Fleeing");
                Fleeing();
                //code to animate door closed and switch to closed state
                break;
        }

        if (waypoints.Length > 0)
        {
            
            //directionToWaypoint = (waypoints[currentWaypoint].transform.position - transform.position);
            //angleToWaypoint = Vector3.Angle(directionToWaypoint, transform.forward);

            //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionToWaypoint), rotationSpeed * Time.deltaTime);
            //transform.Translate(0, 0, Time.deltaTime * speed);
            if (Vector3.Distance(waypoints[currentWaypoint].transform.position, transform.position) < 3.0f)
            {
                //Debug.Log("success!");
                currentWaypoint++;
                Debug.Log(currentWaypoint);

                if (currentWaypoint >= waypoints.Length)
                {
                    Debug.Log("ranOut");
                    currentWaypoint = 0;
                }
            }

            //Rotate to waypoint
            Vector3 directionToWayPoint = waypoints[currentWaypoint].transform.position - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionToWayPoint), rotationSpeed * Time.deltaTime);
            transform.Translate(0,0,Time.deltaTime * speed);
        }
    }

    public enum States
    {
        Wander,
        Search,
        Seek,
        Attack,
        Flee
    }

    public void watching()
    {
        //Calculate the angle from guard to the player
        //If angle from guard to player is less than the field of view angle
        //And the player is not behind an obstacle, the guard can see the player

        visibleTargets.Clear();
        distanceToTarget = Vector3.Distance(transform.position, player.position);
        directionToPlayer = (player.position - transform.position);
        angleToPlayer = Vector3.Angle(directionToPlayer, transform.forward);
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, visionRadius, playerMask);
        if (angleToPlayer < visionAngle / 2)
        {
            if (!Physics.Raycast(transform.position, directionToPlayer, distanceToTarget, walls))
            {
                visibleTargets.Add(player);

                lineRender.SetVertexCount(2);
                lineRender.SetPosition(0, transform.position);
                lineRender.SetPosition(1, player.position);
            }
            else
            {
                lineRender.SetVertexCount(0);
                Debug.Log("Not visible");
            }
        }
        else
        {
            lineRender.SetVertexCount(0);
            Debug.Log("Not visible");
        }

    }

    public void Wandering()
    {
        if (waypoints.Length > 0)
        {
            currentWaypoint = 0;
            directionToWaypoint = (waypoints[currentWaypoint].transform.position - transform.position);
            angleToWaypoint = Vector3.Angle(directionToWaypoint, transform.forward);

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionToWaypoint), rotationSpeed * Time.deltaTime);
            transform.Translate(0, 0, Time.deltaTime * speed);
            if (Vector3.Distance(waypoints[currentWaypoint].transform.position, transform.position) < 3.0f)
            {
                //Debug.Log("success!");
                currentWaypoint++;
                //Debug.Log(currentWaypoint);

                if(currentWaypoint >= waypoints.Length)
                {
                    Debug.Log("ranOut");
                    currentWaypoint = 0;
                }
            }

            //Rotate to waypoint
            //Vector3 directionToWayPoint = waypoints[currentWaypoint].transform.position - transform.position;
            //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionToWayPoint), rotationSpeed * Time.deltaTime);
            //transform.Translate(0,0,Time.deltaTime * speed);
        }

        /*if(Vector3.Distance(player.position, this.transform.position) < 10 && (angleToPlayer < 30 || aiState == States.Seek))
        {

        }*/
    }
    private void Searching()
    {

    }
    private void Seeking()
    {

    }
    private void Attacking()
    {

    }
    private void Fleeing()
    {

    }

}
