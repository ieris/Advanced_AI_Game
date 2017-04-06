using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor (typeof (DecisionMaking))]
public class FieldOfViewEditor : Editor {

	void OnSceneGUI() {
        DecisionMaking fow = (DecisionMaking)target;
		Handles.color = Color.white;
		Handles.DrawWireArc (fow.transform.position, Vector3.up, Vector3.forward, 360, fow.visionRadius);
		Vector3 viewAngleA = fow.DirFromAngle (-fow.visionAngle / 2, false);
		Vector3 viewAngleB = fow.DirFromAngle (fow.visionAngle / 2, false);

		Handles.DrawLine (fow.transform.position, fow.transform.position + viewAngleA * fow.visionRadius);
		Handles.DrawLine (fow.transform.position, fow.transform.position + viewAngleB * fow.visionRadius);

		Handles.color = Color.red;
        Handles.DrawWireArc(fow.transform.position, Vector3.up, Vector3.forward, 360, fow.audioRangeZoneOne);
        Handles.color = Color.magenta;
        Handles.DrawWireArc(fow.transform.position, Vector3.up, Vector3.forward, 360, fow.audioRangeZoneTwo);
        Handles.color = Color.yellow;
        Handles.DrawWireArc(fow.transform.position, Vector3.up, Vector3.forward, 360, fow.audioRangeZoneThree);

        Handles.color = Color.blue;
        Handles.DrawWireArc(fow.transform.position, Vector3.up, Vector3.forward, 360, fow.visionRadius/2);

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
