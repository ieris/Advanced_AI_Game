using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class DecisionMaking : MonoBehaviour
{
    //Sight
    public float resolution;
    public int stepCount;
    public float stepAngleSize;


    //public bool startFollowingPath = false;

    public LineRenderer lineRender;
    public LayerMask playerMask;
    public LayerMask walls;

    public Transform head;
    public Transform aiPos;
    public List<Transform> visibleTargets = new List<Transform>();

    public int health = 100;
    public int damage = 10;
    public float speed = 1.5f;
    public int visionRadius = 10;
    public int visionAngle = 60;
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

    void LateUpdate()
    {
        sightVisualisation();
    }
	void Start ()
    {
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
        //sightVisualisation();

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
                //Debug.Log("Not visible");
            }
        }
        else
        {
            lineRender.SetVertexCount(0);
            //Debug.Log("Not visible");
        }

        //sightVisualisation();

    }


    void sightVisualisation()
    {
        stepCount = Mathf.RoundToInt(visionAngle * resolution);
        stepAngleSize = visionAngle / (visionAngle * stepCount);

        Debug.Log(stepAngleSize);
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - visionAngle / 2 + stepAngleSize * i;
            Debug.DrawLine(transform.position, transform.position + DirFromAngle(angle, true) * visionRadius, Color.red);
        }
    }

    public void Wandering()
    {

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
