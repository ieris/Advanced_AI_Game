using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    public static bool sneaking = false;
    public static bool running = false;

	void Start ()
    {
	
	}

	void Update ()
    {
        //Attacking
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Attack!");
            //Play attack animation
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
    }
}
