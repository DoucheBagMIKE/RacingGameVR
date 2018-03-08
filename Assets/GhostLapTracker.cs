using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostLapTracker : MonoBehaviour {

    public GameObject ghostCar;
    Rigidbody rb;
    VehicleMovement movement;

    List<GhostLapData> lapData;
    int ghostLap;
    int pos;

    //List<Vector3> LapPositions;
    //List<Quaternion> lapRotations;

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
        

        lapData = new List<GhostLapData>();

        for (int i = 0; i < GameManager.instance.raceInfo.numberOfLaps;i++)
        {
            lapData.Add(new GhostLapData());
        }

    }

    void UpdateTrackedVars()
    {
        if (GameManager.instance.raceInfo.currentLap > GameManager.instance.raceInfo.numberOfLaps - 1)
            return;
        int cLap = GameManager.instance.raceInfo.currentLap;
        lapData[cLap].LapPositions.Add(transform.position);
        lapData[cLap].lapRotations.Add(transform.rotation);

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
                if(pos < lapData[ghostLap].LapPositions.Count)
                {
                    //ghostCar.transform.position = lapData[ghostLap].LapPositions[pos];
                    //ghostCar.transform.rotation = lapData[ghostLap].lapRotations[pos];
                    rb.MovePosition(lapData[ghostLap].LapPositions[pos]);
                    rb.MoveRotation(lapData[ghostLap].lapRotations[pos]);
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

    void StartGhostingPlayerLaps()
    {
        FinishLine finish = GameObject.FindGameObjectWithTag("FinishLine").GetComponent<FinishLine>();
        finish.lapComplete -= StartGhostingPlayerLaps;

        isGhosting = true;
        ghostCar = Instantiate(ghostCar);
        rb = ghostCar.GetComponent<Rigidbody>();
        ghostLap = 0;
        pos = 0;



    }

    public class GhostLapData
    {
        public List<Vector3> LapPositions;
        public List<Quaternion> lapRotations;

        public GhostLapData()
        {
            LapPositions = new List<Vector3>();
            lapRotations = new List<Quaternion>();
        }
    }
}