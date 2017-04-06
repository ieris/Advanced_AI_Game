using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;

public class Player : MonoBehaviour
{
    public static bool sneaking = false;
    public static bool running = false;
    public static bool visibleInLight = false;

    private int damage = 10;
    private float timer = 1.5f;
    private bool rotate = false;

    public static int health = 100;

    FirstPersonController fpc;    

    void Awake()
    {
        fpc = GetComponent<FirstPersonController>();
    }

    void Start()
    {
        fpc = new FirstPersonController();
    }

	void Update ()
    {
        //Debug.Log(visibleInLight);
        //Attacking
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Attack!");

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 5f))
            {
                Debug.Log("raycasting");
                if (hit.transform.tag == "Guard")
                {
                    hit.collider.gameObject.GetComponent<DecisionMaking>().anim.Play("hurt");
                    hit.collider.gameObject.GetComponent<DecisionMaking>().health -= damage;
                    Debug.Log("guard health: " + hit.collider.gameObject.GetComponent<DecisionMaking>().health);            
                }
            }
        }

        //If control button is being held
        //I am sneaking
        if (Input.GetButton("Fire1"))
        {
            sneaking = true;
        }
        else
        {
            sneaking = false;
        }

        if(Input.GetKey(KeyCode.LeftShift))
        {
            running = true;
        }
        else
        {
            running = false;
        }

        if (health <= 0  && rotate == false)
        {
            rotate = true;
            death();
        }
    }

    void death()
    {
        GetComponent<FirstPersonController>().enabled = false;
        Debug.Log("Game Over");
        Time.timeScale = 0;
    }
}
