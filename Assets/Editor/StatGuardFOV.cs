using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(StationaryGuard))]
public class StatGuardFOV : Editor
{

    void OnSceneGUI()
    {
        StationaryGuard fow2 = (StationaryGuard)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fow2.transform.position, Vector3.up, Vector3.forward, 360, fow2.visionRadius);
        Vector3 viewAngleA2 = fow2.DirFromAngle(-fow2.visionAngle / 2, false);
        Vector3 viewAngleB2 = fow2.DirFromAngle(fow2.visionAngle / 2, false);

        Handles.DrawLine(fow2.transform.position, fow2.transform.position + viewAngleA2 * fow2.visionRadius);
        Handles.DrawLine(fow2.transform.position, fow2.transform.position + viewAngleB2 * fow2.visionRadius);

        Handles.color = Color.red;
        Handles.DrawWireArc(fow2.transform.position, Vector3.up, Vector3.forward, 360, fow2.audioRangeZoneOne);
        Handles.color = Color.magenta;
        Handles.DrawWireArc(fow2.transform.position, Vector3.up, Vector3.forward, 360, fow2.audioRangeZoneTwo);
        Handles.color = Color.yellow;
        Handles.DrawWireArc(fow2.transform.position, Vector3.up, Vector3.forward, 360, fow2.audioRangeZoneThree);

        Handles.color = Color.blue;
        Handles.DrawWireArc(fow2.transform.position, Vector3.up, Vector3.forward, 360, fow2.visionRadius / 2);
    }
}
