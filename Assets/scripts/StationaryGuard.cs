﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StationaryGuard : MonoBehaviour
{
    public Animator anim;
    public GameObject statusSphere;
    //Hearing range zones
    //Anything in zone one is guaranteed to be heard
    //Anything in zone two has 75% chance to be heard
    //Anything in zone three has 25% chance to be heard
    private int zoneOnePercentage = 100;
    private int zoneTwoPercentage = 75;
    private int zoneThreePercentage = 25;
    public float audioRangeZoneOne = 10f;
    public float audioRangeZoneTwo = 15f;
    public float audioRangeZoneThree = 20f;

    public static Vector3 originalPosition;
    private float confidenceRating;
    private Transform lastHeardLocation;
    private int guardHearing = 40;
    public bool heard = false;

    public static bool farLastSeenLocation = false;
    private float randomRating;
    public static Vector3 lastSeenLocation;
    private float searchTimer = 10f;

    //Guard ability
    public int hitAccuracy = 60;
    public int blockAccuracy = 20;
    public float attackSpeed = 0f;

    public int health = 100;
    public int damage = 10;
    public float speed = 1.5f;

    //Pathfdinging
    Pathfinding pathfinding;
    StationaryGuard statGuard;
    Player playerGameObj;

    public bool help = false;
    public bool safe = false;
    public bool goingToHelp = false;

    //Sight
    public bool seen = false;
    private bool lastLocationChecked = false;
    private bool randomFinished = false;

    //public bool startFollowingPath = false;
    private bool seenAtLeastOnce = false;
    public LineRenderer lineRender;
    public LayerMask playerMask;
    public LayerMask walls;
    public LayerMask visionMask;
    public LayerMask guardMask;
    public LayerMask lightMast;

    public Transform stationaryGuard;
    public Transform head;
    public Transform aiPos;
    public List<Transform> visibleTargets = new List<Transform>();

    public float visionRadius = 8f;
    public float visionAngle = 60f;
    public float rotationSpeed = 0.2f;
    Vector3 randomDirection;

    public static States aiState;
    public bool returnToPost = false;
    public Transform player;
    public float angleToPlayer;

    public GameObject[] waypoints;
    public int currentWaypoint = 0;
    public Vector3 directionToWaypoint;
    public float angleToWaypoint;

    void Awake()
    {
        pathfinding = GetComponent<Pathfinding>();
        aiState = States.Idle;
    }
    void Start()
    {
        originalPosition = transform.position;
        anim = GetComponent<Animator>();
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

    void Update()
    {
        if (health == 0)
        {
            aiState = States.Dead;
        }
        if (aiState != States.Dead)
        {
            watching();
        }
        if (aiState != States.Dead)
        {
            if (!seen)
            {
                listening();
            }
        }
        switch (aiState)
        {
            case States.Idle:
                Idle();
                //execute code for animating the door open, switch to open when done
                break;
            case States.Search:
                Searching();
                //execute code for animating the door open, switch to open when done
                break;
            case States.Seek:
                //Debug.Log("NO " + wandering);
                Seeking();
                //listening code for event to close door
                break;
            case States.Attack:
                Attacking();
                //code to animate door closed and switch to closed state
                break;
            case States.Help:
                Helping();
                //code to animate door closed and switch to closed state
                break;
            case States.Dead:
                Dead();
                //code to animate door closed and switch to closed state
                break;
            case States.ReturnToPost:
                ReturnToPost();
                //code to animate door closed and switch to closed state
                break;
        }
    }

    public enum States
    {
        Idle,
        Search,
        Seek,
        Attack,
        Help,
        ReturnToPost,
        Dead
    }

    public void listening()
    {
        //Debug.Log(heard);
        float distanceToTarget = Vector3.Distance(transform.position, player.position);

        //Sound source is coming from zone one (guaranteed to be heard)
        //But only if player is not sneaking

        if ((player.GetComponent<CharacterController>().velocity.magnitude > 0))
        {
            if (distanceToTarget <= audioRangeZoneOne)
            {
                if (Player.sneaking == false)
                {
                    Debug.Log("Who goes there?");
                    //Debug.Log("Heard!");
                    heard = true;
                    aiState = States.Seek;
                }

            }
            //Sound source is coming from zone two (75% chance to be heard)
            //Guaranteed to be heard if running
            else if ((distanceToTarget <= audioRangeZoneTwo) && (distanceToTarget > audioRangeZoneOne))
            {
                if (Player.sneaking == false)
                {
                    confidenceRating = zoneTwoPercentage * (randomRating / 10);

                    Debug.Log("confidence raring: " + confidenceRating);
                    //Check if guard can hear it
                    if (confidenceRating >= guardHearing)
                    {
                        heard = true;
                        Debug.Log("Heard!");
                        aiState = States.Seek;
                    }
                }

                if (Player.running == true)
                {
                    Debug.Log("Who goes there 2 ?");
                    heard = true;
                    aiState = States.Seek;
                }
                else
                {
                    heard = false;
                }


            }
            //Sound source is coming from zone three (25% chance to be heard)
            else if ((distanceToTarget <= audioRangeZoneThree) && (distanceToTarget > audioRangeZoneTwo))
            {
                if (Player.sneaking == false)
                {
                    confidenceRating = zoneThreePercentage * (randomRating / 10);
                    Debug.Log("confidence raring: " + confidenceRating);

                    //Check if guard can hear it
                    if (confidenceRating >= guardHearing)
                    {
                        heard = true;
                        aiState = States.Seek;
                    }
                }
                if (Player.running == true)
                {
                    Debug.Log("Who goes there 3 ?");
                    heard = true;
                    aiState = States.Seek;
                }
                else
                {
                    heard = false;
                }
            }
        }
        else
        {
            heard = false;
        }
    }

    public void watching()
    {
        //Calculate the angle from guard to the player
        //If angle from guard to player is less than the field of view angle
        //And the player is not behind an obstacle, the guard can see the player

        visibleTargets.Clear();
        float distanceToTarget = Vector3.Distance(transform.position, player.position);
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        angleToPlayer = Vector3.Angle(directionToPlayer, transform.forward);
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, visionRadius, playerMask);
        if (angleToPlayer < visionAngle / 2)
        {
            //Will see regardless of light for half guards view range
            if ((distanceToTarget <= visionRadius / 2) && (!Physics.Raycast(transform.position, directionToPlayer, distanceToTarget, walls)))
            {
                seenAtLeastOnce = true;
                visibleTargets.Add(player);

                lineRender.SetVertexCount(2);
                lineRender.SetPosition(0, transform.position);
                lineRender.SetPosition(1, player.position);

                RaycastHit hit;
                if (Physics.Raycast(transform.position, directionToPlayer, out hit))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        lastSeenLocation = hit.point;
                    }
                }

                seen = true;
            }
            //Will see if intruder is visible
            else if (distanceToTarget <= visionRadius && (!Physics.Raycast(transform.position, directionToPlayer, distanceToTarget, walls)) && Player.visibleInLight == true)
            {
                visibleTargets.Add(player);

                lineRender.SetVertexCount(2);
                lineRender.SetPosition(0, transform.position);
                lineRender.SetPosition(1, player.position);

                RaycastHit hit;
                if (Physics.Raycast(transform.position, directionToPlayer, out hit))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        lastSeenLocation = hit.point;
                    }
                }
                seenAtLeastOnce = true;
                seen = true;
            }
            /*else
            {
                lineRender.SetVertexCount(0);
                seen = false;
            }*/
        }
        else
        {
            seen = false;
            lineRender.SetVertexCount(0);
        }
    }

    public void Idle()
    {
        anim.Play("idle");
        statusSphere.GetComponent<Renderer>().material.color = Color.blue;       
    }

    public void Helping()
    {
        statusSphere.GetComponent<Renderer>().material.color = Color.green;
        anim.Play("walk");


    }
    public void Searching()
    {
        //lastLocationChecked = true;
        statusSphere.GetComponent<Renderer>().material.color = Color.yellow;
        anim.Play("walk");
        //Search state lasts 10 secs
        if (searchTimer >= 0)
        {
            searchTimer -= Time.deltaTime;
        }
        else
        {
            Debug.Log("Time to go back to work!");
            Pathfinding.startFollowingPath = true;
            aiState = States.ReturnToPost;
            lastLocationChecked = false;
            searchTimer = 10f;
        }
        //If intruder is seen the timer is reset
        if (seen || heard)
        {
            Debug.Log("INTRUDER!");
            searchTimer = 10f;
            aiState = States.Seek;
            lastLocationChecked = false;
        }
        //If intruder not seen
        //Walk towards last seen location
        //If nothing is seen
        //Pick a random direction to walk in
        else if (((!seen && lastLocationChecked == false) || (!heard && lastLocationChecked == false)) && seenAtLeastOnce)
        {
            //#Tergum <3

            //Debug.Log("Where was he last?");

            //Walk towards last seen location
            if (!(Vector3.Distance(transform.position, lastSeenLocation) <= 1f))
            {
                if(Vector3.Distance(transform.position, lastSeenLocation) >= 6f)
                {
                    transform.LookAt(lastSeenLocation);
                    transform.position = Vector3.MoveTowards(transform.position, new Vector3(lastSeenLocation.x, 0f, lastSeenLocation.z), pathfinding.walkingSpeed * Time.deltaTime);
                    farLastSeenLocation = false;
                }
                else
                {
                    farLastSeenLocation = true;
                }
            }
            else if (Vector3.Distance(transform.position, lastSeenLocation) <= 1f)
            {
                Debug.Log(lastLocationChecked);
                lastLocationChecked = true;
            }

        }
        else if (!seen && lastLocationChecked == true)
        {
            if (!(Vector3.Distance(transform.position, randomDirection) <= 5f))
            {
                Debug.Log("Walk towards random direction");
                transform.LookAt(randomDirection);
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(randomDirection.x, 0, randomDirection.z), pathfinding.walkingSpeed * Time.deltaTime);

                RaycastHit hit;
                if (Physics.Raycast(transform.position, randomDirection, out hit))
                {
                    if (hit.transform.CompareTag("Obstacle") && Vector3.Distance(transform.position, hit.point) <= 1f)
                    {
                        //Debug.Log("Chose a more suitable random direction");
                        randomDirection = new Vector3(UnityEngine.Random.Range(10.0f, -20.0f), 0, UnityEngine.Random.Range(27.0f, -27.0f));
                    }
                }
            }
            else if (Vector3.Distance(transform.position, randomDirection) <= 5f)
            {
                randomFinished = true;
            }
        }

        if (randomFinished == true)
        {
            randomDirection = new Vector3(UnityEngine.Random.Range(10.0f, -20.0f), 0, UnityEngine.Random.Range(27.0f, -27.0f));
            randomFinished = false;
        }
    }

    public void ReturnToPost()
    {
        anim.Play("walk");
        statusSphere.GetComponent<Renderer>().material.color = Color.cyan;
    }

    public void Seeking()
    {
        statusSphere.GetComponent<Renderer>().material.color = Color.magenta;
        anim.Play("run");
    }
    public void Attacking()
    {
        statusSphere.GetComponent<Renderer>().material.color = Color.red;

        if (attackSpeed <= 1)
        {

            attackSpeed += Time.deltaTime;
        }
        else
        {
            anim.Play("attack");
            attackSpeed = 0f;

            if (this.anim.GetCurrentAnimatorStateInfo(0).IsName("attack"))
            {
                Debug.Log("Animation stopped playing");
                Player.health -= damage;
                Debug.Log("health: " + Player.health);
            }
            float hitSuccess;

            //If guard 
            //If guard is in range of the intruder
            if (Vector3.Distance(transform.position, player.transform.position) <= 2f)
            {
                hitSuccess = UnityEngine.Random.Range(1.0f, 10.0f);
            }
        }
    }

    public void Dead()
    {
        statusSphere.GetComponent<Renderer>().material.color = Color.black;
        anim.Play("die");
        if (this.anim.GetCurrentAnimatorStateInfo(0).IsName("die") && this.anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            Debug.Log("now dead");
            this.GetComponent<Animator>().Stop();
            Destroy(GetComponent<StationaryGuard>());
            Destroy(gameObject);
        }
    }
}