using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostLapTracker : MonoBehaviour {


    VehicleMovement movement;

    List<Vector3> LapPositions;
    List<Quaternion> lapRotations;

    float trackedTimeOffset = 0.2f;
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

        FinishLine finish = GameObject.FindGameObjectWithTag("FinishLine").GetComponent<FinishLine>();
        finish.lapComplete += StartGhostingPlayerLaps;
    }

    void UpdateTrackedVars()
    {
        LapPositions.Add(transform.position);
        lapRotations.Add(transform.rotation);

        lastTrackedTime = 0f;
    }

    private void Update()
    {
        lastTrackedTime += Time.deltaTime;

        if (lastTrackedTime >= trackedTimeOffset && movement.isOnGround)
        {
            UpdateTrackedVars();
        }
    }

    void StartGhostingPlayerLaps()
    {

    }

    private void OnDestroy()
    {
        FinishLine finish = GameObject.FindGameObjectWithTag("FinishLine").GetComponent<FinishLine>();
        finish.lapComplete -= StartGhostingPlayerLaps;
    }
}