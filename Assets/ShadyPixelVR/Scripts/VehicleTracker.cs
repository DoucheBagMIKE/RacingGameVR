using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleTracker : MonoBehaviour {

    VehicleMovement movement;

    [ReadOnly]
    public Vector3 trackedPosition;
    [ReadOnly]
    public Quaternion trackedRotation;

    public float trackedTimeOffset;
    float lastTrackedTime;

    private void Awake()
    {
        movement = GetComponent<VehicleMovement>();

        if (movement == null)
        {
            Debug.LogError("No Vehicile Movement SCript Attached to this GameObject");
            return;
        }

        UpdateTrackedVars();
    }

    void UpdateTrackedVars()
    {
        trackedPosition = transform.position;
        trackedRotation = transform.rotation;

        lastTrackedTime = 0f;
    }

    private void Update()
    {
        lastTrackedTime += Time.deltaTime;

        if(lastTrackedTime >= trackedTimeOffset && movement.isOnGround)
        {
            UpdateTrackedVars();
        }
    }
}
