using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class DecisionMaking : MonoBehaviour
{
    public Animator anim;

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

    private float confidenceRating;
    private Transform lastHeardLocation;
    private int guardHearing = 40;
    public bool heard = false;

    private float randomRating;
    private Vector3 lastSeenLocation;
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

    public bool wandering = false;
    public bool help = false;
    public bool safe = false;

    //Sight
    public bool seen = false;
    private bool lastLocationChecked = false;
    private bool randomFinished = false;

    //public bool startFollowingPath = false;

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
        //statGuard = GetComponent<StationaryGuard>();
        playerGameObj = GetComponent<Player>();
        aiState = States.Wander;
    }

    void LateUpdate()
    {
        sightVisualisation();
    }
    void Start()
    {
        anim = GetComponent<Animator>();
        pathfinding = new Pathfinding();
        //statGuard = new StationaryGuard();
        playerGameObj = new Player();

        lineRender = GetComponent<LineRenderer>();
        lineRender.SetWidth(0.05f, 0.05f);
        lineRender.SetColors(Color.red, Color.red);

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

    public static void IgnoreLayerCollision(int guardMask, int lightMask, bool ignore = true)
    {
        Debug.Log("ignore");
    }

    void Update()
    {
        if(aiState != States.Flee)
        {
            watching();
        }   
        //listening();
        //sightVisualisation();

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
        Flee,
        Dead
    }

    public void listening()
    {
        //Debug.Log(heard);
        float distanceToTarget = Vector3.Distance(transform.position, player.position);

        //Sound source is coming from zone one (guaranteed to be heard)
        //But only if player is not sneaking
        if (distanceToTarget <= audioRangeZoneOne)
        {
            if(Player.sneaking == false)
            {
                //Debug.Log("Who goes there?");
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

                //Debug.Log("confidence raring: " + confidenceRating);
                //Check if guard can hear it
                if (confidenceRating >= guardHearing)
                {
                    heard = true;
                    //Debug.Log("Heard!");
                }
            }

            if(Player.running == true)
            {
                //Debug.Log("Who goes there?");
                heard = true;
                //Debug.Log("Heard!");
                aiState = States.Seek;
            }

            
        }
        //Sound source is coming from zone three (25% chance to be heard)
        else if ((distanceToTarget <= audioRangeZoneThree) && (distanceToTarget > audioRangeZoneTwo))
        {
            if(Player.sneaking == false)
            {
                confidenceRating = zoneThreePercentage * (randomRating / 10);
                Debug.Log("confidence raring: " + confidenceRating);

                //Check if guard can hear it
                if (confidenceRating >= guardHearing)
                {
                    heard = true;
                }
            }
            if (Player.running == true)
            {
                Debug.Log("Who goes there?");
                heard = true;
                aiState = States.Seek;
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
        //Debug.Log((distanceToTarget <= visionRadius) + " vision radius " + visionRadius +  " distance " + distanceToTarget);
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        angleToPlayer = Vector3.Angle(directionToPlayer, transform.forward);
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, visionRadius, playerMask);
        //Collider[] lightsInViewRadius = Physics.OverlapSphere(transform.position, visionRadius, visionMask);
        if (angleToPlayer < visionAngle / 2)
        {
            if (distanceToTarget <= visionRadius && (!Physics.Raycast(transform.position, directionToPlayer, distanceToTarget, walls)) && Player.visibleInLight == true)
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

                seen = true;
            }
            else
            {
                lineRender.SetVertexCount(0);
                seen = false;
            }
        }
        else
        {
            seen = false;
            lineRender.SetVertexCount(0);
        }

        //sightVisualisation();

    }

    public void sightVisualisation()
    {
        var myAngle = visionRadius * Vector3.up;
    }

    public void Wandering()
    {
        anim.Play("walk");
        if (seen)
        {
            Pathfinding.startFollowingPath = false;
            aiState = States.Seek;
        }
    }
    public void Searching()
    {
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
        }
        //If intruder not seen
        //Walk towards last seen location
        //If nothing is seen
        //Pick a random direction to walk in
        else if (!seen && lastLocationChecked == false)
        {
            //#Tergum <3

            //Debug.Log("Where was he last?");

            //Walk towards last seen location
            if (!(Vector3.Distance(transform.position, lastSeenLocation) <= 1f))
            {
                //transform.LookAt(lastSeenLocation);
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(lastSeenLocation.x, 0f, lastSeenLocation.z), pathfinding.walkingSpeed * Time.deltaTime);
            }
            else if (Vector3.Distance(transform.position, lastSeenLocation) <= 1f)
            {
                Debug.Log(lastLocationChecked);
                lastLocationChecked = true;
            }
            
        }
        else if(!seen && lastLocationChecked == true)
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

        if(randomFinished == true)
        {
            randomDirection = new Vector3(UnityEngine.Random.Range(10.0f, -20.0f), 0, UnityEngine.Random.Range(27.0f, -27.0f));
            randomFinished = false;
        }           
        
    }

    public void Seeking()
    {
        anim.Play("run");
    }
    public void Attacking()
    {
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

            //If heavily injured, RUN
            if (health == 10)
            {
                Transform fleeLocation = transform;
                aiState = States.Flee;
            }

            if (health <= 0)
            {
                anim.Play("die");
                Debug.Log("dead");
            }
        }
    }

    public void Fleeing()
    {
        //anim.Play("run");
        //Debug.Log("fleeing" + stationaryGuard.transform.position);
        Pathfinding.startFollowingPath = false;        

        //pathfinding.waypointIndex = 0;
        //help = true;

        /*Vector3 directionToStatGuard = (transform.position - stationaryGuard.position).normalized;
        float distanceToStatGuard = Vector3.Distance(transform.position, stationaryGuard.position);

        Debug.Log(directionToStatGuard);
        Debug.Log(distanceToStatGuard);*/

        //In reach of stationary guard - safe area

        if (!(Vector3.Distance(transform.position, stationaryGuard.position) <= 5f))
        {            
            pathfinding.target = stationaryGuard;
            //Debug.Log("fleeing" + pathfinding.target.transform.position);
            pathfinding.FindPath(transform.position, stationaryGuard.transform.position);
            Pathfinding.startFollowingPath = true;
        }

        /*if (Physics.Raycast(transform.position, directionToStatGuard) && distanceToStatGuard <= 2f)
        {                      
            safe = true;
            StationaryGuard.aiState = StationaryGuard.States.Search;
        }*/
    }
}