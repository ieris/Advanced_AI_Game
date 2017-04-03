using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class DecisionMaking : MonoBehaviour
{
    //Hearing range zones
    //Anything in zone one is guaranteed to be heard
    //Anything in zone two has 75% chance to be heard
    //Anything in zone three has 25% chance to be heard
    private int zoneOne = 100;
    private int zoneTwo = 75;
    private int zoneThree = 25;
    public float audioRangeZoneOne = 10f;
    public float audioRangeZoneTwo = 15f;
    public float audioRangeZoneThree = 20f;

    private Vector3 lastHeardLocation;

    //Guard ability
    public int hitAccuracy = 60;
    public int blockAccuracy = 20;
    public float attackSpeed = 2.5f;

    public int health = 100;
    public int damage = 10;
    public float speed = 1.5f;

    //Pathfdinging
    Pathfinding pathfinding;

    public bool wandering = false;

    //Sight
    public bool seen = false;


    //public bool startFollowingPath = false;

    public LineRenderer lineRender;
    public LayerMask playerMask;
    public LayerMask walls;

    public Transform head;
    public Transform aiPos;
    public List<Transform> visibleTargets = new List<Transform>();

    
    public float visionRadius = 0.2f;
    public float visionAngle = 60f;  
    public float rotationSpeed = 0.2f;
    
    public static States aiState;

    public Transform player;
    public float angleToPlayer;

    public GameObject[] waypoints;
    public int currentWaypoint = 0;
    public Vector3 directionToWaypoint;
    public float angleToWaypoint;

    void Awake()
    {
        pathfinding = GetComponent<Pathfinding>();
        aiState = States.Wander;
    }

    void LateUpdate()
    {
        sightVisualisation();
    }
	void Start ()
    {
        pathfinding = new Pathfinding();

        lineRender = GetComponent<LineRenderer>();
        lineRender.SetWidth(0.05f, 0.05f);
        lineRender.SetColors(Color.red, Color.red);
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    void Update ()
    {
        //Wandering();
        watching();
        //sightVisualisation();
        //Debug.Log(aiState);
        switch (aiState)
        {
            case States.Wander:
                //Debug.Log("Wandering");
                wandering = true;
                //Debug.Log("YES " + wandering);
                Wandering();
                //execute closed code & listener for interaction
                break;
            case States.Search:
                Searching();
                //execute code for animating the door open, switch to open when done
                break;
            case States.Seek:
                wandering = false;
                //Debug.Log("NO " + wandering);
                Seeking();
                //listening code for event to close door
                break;
            case States.Attack:
                Attacking();
                //code to animate door closed and switch to closed state
                break;
            case States.Flee:
                Fleeing();
                //code to animate door closed and switch to closed state
                break;
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

    public void listening()
    {
        //Sound source is coming from zone one (guaranteed to be heard)
        if(Vector3.Distance(transform.position, lastHeardLocation) <= audioRangeZoneOne)
        {
            aiState = States.Seek;
        }
        //Sound source is coming from zone two (75% chance to be heard)
        else if ((Vector3.Distance(transform.position, lastHeardLocation) <= audioRangeZoneTwo) && (Vector3.Distance(transform.position, lastHeardLocation) > audioRangeZoneOne))
        {

        }
        //Sound source is coming from zone three (25% chance to be heard)
        else if ((Vector3.Distance(transform.position, lastHeardLocation) <= audioRangeZoneThree) && (Vector3.Distance(transform.position, lastHeardLocation) > audioRangeZoneTwo))
        {

        }
    }

    public void watching()
    {
        //sightVisualisation();

        //Calculate the angle from guard to the player
        //If angle from guard to player is less than the field of view angle
        //And the player is not behind an obstacle, the guard can see the player

        visibleTargets.Clear();
        float distanceToTarget = Vector3.Distance(transform.position, player.position);
        Debug.Log((distanceToTarget <= visionRadius) + " vision radius " + visionRadius +  " distance " + distanceToTarget);
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        angleToPlayer = Vector3.Angle(directionToPlayer, transform.forward);
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, visionRadius, playerMask);
        if (angleToPlayer < visionAngle / 2)
        {
            if (distanceToTarget <= visionRadius && (!Physics.Raycast(transform.position, directionToPlayer, distanceToTarget, walls)))
            {
                visibleTargets.Add(player);

                lineRender.SetVertexCount(2);
                lineRender.SetPosition(0, transform.position);
                lineRender.SetPosition(1, player.position);

                seen = true;
            }
            else
            {
                lineRender.SetVertexCount(0);
                seen = false;
                //Debug.Log("Not visible");
            }
        }
        else
        {
            seen = false;
            lineRender.SetVertexCount(0);
            //Debug.Log("Not visible");
        }

        //sightVisualisation();

    }


    public void sightVisualisation()
    {
        var myAngle = visionRadius * Vector3.up;

        //Debug.DrawLine(transform.position, hit.point);
    }

    public void Wandering()
    {
        //wandering = true;
        //watching();
        //Debug.Log("wandering " + wandering);
        if (seen)
        {
            Pathfinding.startFollowingPath = false;
            aiState = States.Seek;
            Debug.Log(aiState);            
        }
    }
    public void Searching()
    {

    }
    public void Seeking()
    {
        //Debug.Log("I am now seeking the intruder");
        //pathfinding.target = player.transform;
        //pathfinding.FindPath(pathfinding.seeker.position, pathfinding.target.position);

    }
    public void Attacking()
    {
        float hitSuccess;

        //If guard is in range of the intruder
        if(Vector3.Distance(transform.position, player.transform.position) <= 2f)
        {
            hitSuccess = UnityEngine.Random.Range(1.0f, 10.0f);


        }
    }
    public void Fleeing()
    {

    }

}
