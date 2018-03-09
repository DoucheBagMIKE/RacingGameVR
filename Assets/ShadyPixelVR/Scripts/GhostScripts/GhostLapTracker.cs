using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostLapTracker : MonoBehaviour {

    public GameObject ghostCar;
    Rigidbody rb;
    VehicleMovement movement;
    [HideInInspector]
    public GhostTrackData trackData;
    [HideInInspector]
    public GhostTrackData savedTrackData;
    int ghostLap;
    int pos;

    float trackedTimeOffset = 0.01f;
    float lastTrackedTime;

    bool isGhosting = false;

    private void Awake()
    {
        movement = GetComponent<VehicleMovement>();

        if (movement == null)
        {
            Debug.LogError("No Vehicile Movement SCript Attached to this GameObject");
            return;
        }

        trackData = new GhostTrackData();

    }

    private void Start()
    {
        GameObject go = GameObject.FindGameObjectWithTag("FinishLine");
        if(go != null)
        {
            FinishLine finish = GameObject.FindGameObjectWithTag("FinishLine").GetComponent<FinishLine>();
            if (finish != null)
                finish.lapComplete += StartGhostingPlayerLaps;
        }

        trackData.name = GameManager.instance.raceInfo.selectedTrack.name;
        trackData.lap = new List<GhostLapData>();
        for (int i = 0; i < GameManager.instance.raceInfo.numberOfLaps; i++)
        {
            trackData.lap.Add(new GhostLapData());
        }

    }

    void UpdateTrackedVars()
    {
        if (GameManager.instance.raceInfo.currentLap > GameManager.instance.raceInfo.numberOfLaps - 1)
            return;
        int cLap = GameManager.instance.raceInfo.currentLap;
        trackData.lap[cLap].LapPositions.Add(transform.position);
        trackData.lap[cLap].lapRotations.Add(transform.rotation);

        lastTrackedTime = 0f;
    }

    private void Update()
    {
        if (GameManager.instance.currentState == GameManager.GameState.RaceStart || GameManager.instance.currentState == GameManager.GameState.Menu)
            return;

        lastTrackedTime += Time.deltaTime;

        if (lastTrackedTime >= trackedTimeOffset)
        {
            UpdateTrackedVars();

            if(isGhosting)
            {
                GhostTrackData data = savedTrackData != null ? savedTrackData : trackData;
                if (pos < data.lap[ghostLap].LapPositions.Count)
                {
                    rb.MovePosition(data.lap[ghostLap].LapPositions[pos]);
                    rb.MoveRotation(data.lap[ghostLap].lapRotations[pos]);
                    pos++;
                }
                else
                {
                    pos = 0;
                    ghostLap++;
                    if(ghostLap > GameManager.instance.raceInfo.numberOfLaps - 1)
                    {
                        isGhosting = false;
                    }
                }
            }

        }
    }

    public void StartGhostingPlayerLaps()
    {
        FinishLine finish = GameObject.FindGameObjectWithTag("FinishLine").GetComponent<FinishLine>();
        finish.lapComplete -= StartGhostingPlayerLaps;

        isGhosting = true;
        ghostCar = Instantiate(ghostCar);
        rb = ghostCar.GetComponent<Rigidbody>();
        ghostLap = 0;
        pos = 0;
    }
}