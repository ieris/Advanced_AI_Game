using UnityEngine;
using System.Collections;

public class Light : MonoBehaviour
{
    public Transform player;


	void Start ()
    {

	}

	void Update ()
    {

	}

    void onTriggerEnter(Collider col)
    {
        if(col.gameObject.tag == "Player")
        {
            Player.visibleInLight = true;
        }
    }

    void onTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            Player.visibleInLight = false;
        }
    }
}
