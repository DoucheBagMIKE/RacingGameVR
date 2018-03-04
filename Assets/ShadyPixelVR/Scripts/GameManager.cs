using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager instance;

    [Header("Game State")]
    [Tooltip("Current state of the game.")]
    public GameState currentState;

    public enum GameState
    {
        Menu,
        RaceStart,
        Racing,
        RaceEnd
    }

    [Header("Race Settings")]
    [Tooltip("How many laps will the race go on for.")]
    public int numberOfLaps = 3;
    [Tooltip("Player reference. If blank, will set at runtime when avaliable.")]
    public VehicleMovement playerCarController;

    [Tooltip("Current player time on lap.")]
    public float currentLapTime = 0.0f;

    [Tooltip("Current lap player is on.")]
    public int currentLap = 1;

    // Use this for initialization
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

    }

    public void Start()
    {
        StartCoroutine(RaceStart());
    }

    public void Update()
    {
        if (currentState == GameState.Racing)
            RaceUpdate();
    }

    public IEnumerator RaceStart()
    {
        //Sets current state to Start
        currentState = GameState.RaceStart;

        //Find ref to player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        //..if found player, find ref to car controller
        if(playerObj!= null)
            playerCarController = playerObj.GetComponent<VehicleMovement>();
        else
            Debug.Log("Can not find player object!");

        if(playerCarController==null)
            Debug.Log("Can not find player car!");

        //wait 5 seconds before start of race
        //should have countdown timer here
        Debug.Log("START YOUR ENGINES!");
        yield return new WaitForSeconds(1f);
        Debug.Log("GET READY!");
        yield return new WaitForSeconds(1f);
        Debug.Log("3..");
        yield return new WaitForSeconds(1f);
        Debug.Log("2..");
        yield return new WaitForSeconds(1f);
        Debug.Log("1..");
        yield return new WaitForSeconds(1f);
        Debug.Log("GO!");

        //change game state to racing
        currentState = GameState.Racing;

        yield return null;
    }

    public void RaceUpdate()
    {
        currentLapTime += Time.deltaTime;
    }

    public void CompleteLap()
    {
        //debug to console, goto next lap, reset lap timer.
        Debug.Log("Lap: " + currentLapTime);
        currentLap++;
        currentLapTime = 0.0f;

        //last lap check
        if (currentLap == numberOfLaps)
            Debug.Log("LAST LAP!");

        //end race check
        if (currentLap > numberOfLaps)
            EndRace();
    }

    public void EndRace()
    {
        currentState = GameState.RaceEnd;
    }
}
