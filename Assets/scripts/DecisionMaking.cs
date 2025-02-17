﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DecisionMaking : MonoBehaviour
{
    public static bool walkToLastLocation = false;
    private int increment = 0;
    public Animator anim;
    public GameObject statusSphere;

    //Hearing range zones
    //Anything in zone one is guaranteed to be heard
    //Anything in zone two has 75% chance to be heard
    //Anything in zone three has 25% chance to be heard
    private int zoneOnePercentage = 100;
    private int zoneTwoPercentage = 75;
    private int zoneThreePercentage = 25;
    public float audioRangeZoneOne = 8f;
    public float audioRangeZoneTwo = 11f;
    public float audioRangeZoneThree = 14f;

    private float confidenceRating;
    private Vector3 lastLocation;
    private Vector3 lastHeardLocation;
    private int guardHearing = 40;
    public bool heard = false;

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
    public static bool safe = false;

    //Sight
    public bool seen = false;
    public static bool lastLocationChecked = false;
    private bool randomFinished = false;

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

    public Transform player;
    public float angleToPlayer;

    public GameObject[] waypoints;
    public int currentWaypoint = 0;
    public Vector3 directionToWaypoint;
    public float angleToWaypoint;

    void Awake()
    {
        pathfinding = GetComponent<Pathfinding>();
        playerGameObj = GetComponent<Player>();
        aiState = States.Wander;
    }

    void Start()
    {
        anim = GetComponent<Animator>();
        pathfinding = new Pathfinding();
        playerGameObj = new Player();

        //draws line when intruder is seen
        lineRender = GetComponent<LineRenderer>();
        lineRender.SetWidth(0.05f, 0.05f);
        lineRender.SetColors(Color.red, Color.red);

        //randomises the condifence rating associated to sound
        randomRating = UnityEngine.Random.Range(1.0f, 10.0f);

        randomDirection = new Vector3(UnityEngine.Random.Range(10.0f, -20.0f), transform.position.y, UnityEngine.Random.Range(27.0f, -27.0f));
        Debug.Log(transform.position.y);
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
        
        if(safe == true)
        {
            anim.Play("idle");
            aiState = States.Idle;
        }
        if(aiState != States.Flee || aiState != States.Dead)
        {
            watching();
        }
        if (aiState != States.Flee || aiState != States.Dead)
        {
            if(!seen)
            {
                listening();
            }           
        }

        switch (aiState)
        {
            case States.Wander:
                Wandering();
                //execute closed code & listener for interaction
                break;
            case States.Search:
                Searching();
                //execute code for animating the door open, switch to open when done
                break;
            case States.Seek:
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
            case States.Dead:
                Dead();
                //code to animate door closed and switch to closed state
                break;
            case States.Idle:
                Idle();
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
        Flee,
        Idle,
        Dead
    }

    public void listening()
    {
        float distanceToTarget = Vector3.Distance(transform.position, player.position);
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        //Sound source is coming from zone one (guaranteed to be heard)
        //But only if player is not sneaking

        if ((player.GetComponent<CharacterController>().velocity.magnitude > 0))
        {
            if (distanceToTarget <= audioRangeZoneOne)
            {
                if (Player.sneaking == false)
                {
                    Debug.Log("Who goes there?");
                    lastHeardLocation = player.transform.position;
                    heard = true;
                    
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
                        
                        Debug.Log("Heard!");
                        lastHeardLocation = player.transform.position;
                        heard = true;
                    }
                }

                if (Player.running == true)
                {
                    Debug.Log("Who goes there?");
                    lastHeardLocation = player.transform.position;
                    heard = true;
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
                        lastHeardLocation = player.transform.position;
                    }
                }
                if (Player.running == true)
                {
                    Debug.Log("Who goes there?");

                    lastHeardLocation = player.transform.position;
                    heard = true;
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

        if(heard)
        {
            
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
            if ((distanceToTarget <= visionRadius/2) && (!Physics.Raycast(transform.position, directionToPlayer, distanceToTarget, walls)))
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
                        Debug.Log("Last seen at: " + lastSeenLocation);
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
                        Debug.Log("Last seen at: " + lastSeenLocation);
                    }
                }

                seen = true;
            }
        }
        else
        {
            seen = false;
            lineRender.SetVertexCount(0);
        }
    }

    public void Wandering()
    {
        statusSphere.GetComponent<Renderer>().material.color = Color.blue;

        anim.Play("walk");
        if (seen)
        {
            Pathfinding.startFollowingPath = false;
            aiState = States.Seek;
        }
        else if(heard && !seen)
        {
            Pathfinding.startFollowingPath = false;
            aiState = States.Search;
        }
        else if(seen && heard)
        {
            Pathfinding.startFollowingPath = false;
            aiState = States.Seek;
        }
    }
    public void Searching()
    {
        Debug.Log(seen);
        statusSphere.GetComponent<Renderer>().material.color = Color.yellow;
        anim.Play("walk");

        //Search state lasts 10 secs
        if (searchTimer >= 0)
        {
            searchTimer -= Time.deltaTime;
        }
        //after 10 seconds go back to wander
        else
        {
            Debug.Log("Time to go back to work!");
            Pathfinding.startFollowingPath = true;           
            aiState = States.Wander;
            lastLocationChecked = false;
            searchTimer = 10f;
        }
        //If intruder is seen the timer is reset
        if (seen)
        {
            Debug.Log("INTRUDER!");
            searchTimer = 10f;
            aiState = States.Seek;
            lastLocationChecked = false;
            lastLocation = lastSeenLocation;
            transform.LookAt(lastSeenLocation);
        }
        //if intruder is heard reset the timer
        if(heard && !seen)
        {
            Debug.Log("Who's there?");
            searchTimer = 10f;
            lastLocation = lastHeardLocation;
            transform.LookAt(lastHeardLocation);
        }
        //If intruder not seen
        //Walk towards last seen location
        //If nothing is seen
        //Pick a random direction to walk in        
        else if ((!seen && lastLocationChecked == false))
        {
            
            //#Tergum <3

            Debug.Log("Where did I see him last?");
            Debug.Log(lastLocation);
            //Walk towards last seen location

            if (!(Vector3.Distance(transform.position, lastLocation) <= 1f))
            {
                //transform.LookAt(lastLocation);
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(lastLocation.x, 0f, lastLocation.z), pathfinding.walkingSpeed * Time.deltaTime);
            }
            else if (Vector3.Distance(transform.position, lastLocation) <= 1f)
            {
                Debug.Log(lastLocationChecked);
                lastLocationChecked = true;
            }
            
        }

        //last location has already been checked so now go in a random direction
        else if(!seen && lastLocationChecked == true)
        { 
            if (!(Vector3.Distance(transform.position, randomDirection) <= 5f))
            {
                Debug.Log("Walk towards random direction");
                transform.LookAt(randomDirection);
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(randomDirection.x, 0, randomDirection.z), pathfinding.walkingSpeed * Time.deltaTime);

                //check if random direction is in front of a wall
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
            //walk to the random direction once and then pick a new one
            else if (Vector3.Distance(transform.position, randomDirection) <= 5f)
            {
                randomFinished = true;
            }
        }

        //pick a new random direction
        if(randomFinished == true)
        {
            randomDirection = new Vector3(UnityEngine.Random.Range(10.0f, -20.0f), 0, UnityEngine.Random.Range(27.0f, -27.0f));
            randomFinished = false;
        }                  
    }

    public void Seeking()
    {
        statusSphere.GetComponent<Renderer>().material.color = Color.magenta;
        anim.Play("run");
    }
    public void Attacking()
    {
        statusSphere.GetComponent<Renderer>().material.color = Color.red;

        //controls the attack speed
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

            //If guard is in range of the intruder
            if (Vector3.Distance(transform.position, player.transform.position) <= 2f)
            {
                hitSuccess = UnityEngine.Random.Range(1.0f, 10.0f);
            }                    
        }
        
        //If heavily injured, RUN and store your location
        if (health == 10)
        {
            aiState = States.Flee;
        }
    }

    //run for help
    public void Fleeing()
    {
        statusSphere.GetComponent<Renderer>().material.color = Color.white;

        anim.Play("walk");

        //die
        if (health == 0)
        {
            aiState = States.Dead;
        }       

        //In reach of stationary guard - safe area

        if (Vector3.Distance(transform.position, stationaryGuard.position) <= 4f)
        {
            safe = true;
            Pathfinding.startFollowingPath = false;
            aiState = States.Idle;
            StationaryGuard.aiState = StationaryGuard.States.Help;
        }
    }

    //if dead, play animation and remove script from game
    public void Dead()
    {
        statusSphere.GetComponent<Renderer>().material.color = Color.black;
        anim.Play("die");
        if (this.anim.GetCurrentAnimatorStateInfo(0).IsName("die") && this.anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            Debug.Log("now dead");
            this.GetComponent<Animator>().Stop();
            Destroy(GetComponent<DecisionMaking>());
            Destroy(gameObject);
        }
    }
    public void Idle()
    {
        Debug.Log("idlleee");
        statusSphere.GetComponent<Renderer>().material.color = Color.cyan;
    }
}