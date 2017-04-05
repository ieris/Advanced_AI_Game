using UnityEngine;
using System.Collections;

public class Light : MonoBehaviour
{

    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.tag == "Light")
        {
            print("Entered");
            Player.visibleInLight = true;
        }       
    }
    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Light")
        {
            print("Exited");
            Player.visibleInLight = false;
        }
    }
}
