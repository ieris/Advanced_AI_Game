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
    }

}
