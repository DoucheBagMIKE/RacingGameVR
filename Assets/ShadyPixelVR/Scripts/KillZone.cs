using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillZone : MonoBehaviour {

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        GameObject go = other.transform.root.gameObject;
        VehicleTracker tracker = go.GetComponent<VehicleTracker>();

        go.GetComponent<VehicleMovement>().StopVelocity();
        go.transform.position = tracker.trackedPosition;
        go.transform.rotation = tracker.trackedRotation;
    }
}
