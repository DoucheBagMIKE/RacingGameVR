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

    private void Update()
    {
        if (GameManager.instance.currentState == GameManager.GameState.RaceStart || GameManager.instance.currentState == GameManager.GameState.Menu)
            return;

        lastTrackedTime += Time.deltaTime;

        if (isGhosting)
        {
            float framesPercentage = lastTrackedTime / trackedTimeOffset;
            int updates = Mathf.CeilToInt(framesPercentage);

            for (int i = 0; i < updates; i++)
            {
                if (framesPercentage - i > 1)
                {
                    UpdateGhost(1f);
                }
                else if (framesPercentage - 1 > 0)
                {
                    UpdateGhost(framesPercentage - i);
                }
            }

        }

        if (lastTrackedTime >= trackedTimeOffset)
        {
            lastTrackedTime -= trackedTimeOffset;

            UpdateTrackedVars();

        }
    }

    void UpdateTrackedVars()
    {
        if (GameManager.instance.raceInfo.currentLap > GameManager.instance.raceInfo.numberOfLaps - 1)
            return;
        int cLap = GameManager.instance.raceInfo.currentLap;
        trackData.lap[cLap].LapPositions.Add(transform.position);
        trackData.lap[cLap].lapRotations.Add(transform.rotation);

    }

    void UpdateGhost(float lerp = 1f)
    {
        GhostTrackData data = savedTrackData.lap.Count > 0 ? savedTrackData : trackData;
        if (pos < data.lap[ghostLap].LapPositions.Count)
        {
            int lPos = pos - 1 >= 0 ? pos - 1 : 0;
            int lLap = pos - 1 >= 0 ? ghostLap : ghostLap - 1;
            lLap = lLap < 0 ? 0 : lLap;

            rb.MovePosition(Vector3.Lerp(data.lap[lLap].LapPositions[lPos], data.lap[ghostLap].LapPositions[pos], lerp));
            rb.MoveRotation(Quaternion.Lerp(data.lap[lLap].lapRotations[lPos], data.lap[ghostLap].lapRotations[pos], lerp));

            if(lerp == 1)
            {
                pos++;
            }
        }
        else
        {
            pos = 0;
            ghostLap++;

            if (ghostLap > GameManager.instance.raceInfo.numberOfLaps - 1)
            {
                isGhosting = false;
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

        GhostTrackData data = savedTrackData.lap.Count > 0 ? savedTrackData : trackData;
        rb.MovePosition(data.lap[0].LapPositions[0]);
        rb.MoveRotation(data.lap[0].lapRotations[0]);
    }
}