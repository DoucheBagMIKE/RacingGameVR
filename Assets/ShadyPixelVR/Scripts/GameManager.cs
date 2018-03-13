using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using Malee;

public class GameManager : MonoBehaviour {

    public static GameManager instance;

    public HighScoreTable highScoreAsset;

    [HideInInspector]
    public GhostLapTracker playerGhostData;

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

    [Tooltip("Some race setup Options and the runtime variables for the current race.")]
    public RaceInfo raceInfo;

    [Tooltip("Player reference. If blank, will set at runtime when avaliable.")]
    public VehicleMovement playerCarController;

    [Header("GUI")]
    public Menu vrMenu;

    #region Debug

    [Header("Debug Options")]
    [Reorderable]
    public DebugOptionList debugOptionList;

    [System.Serializable]
    public class DebugOption
    {
        public KeyCode keyCode;
        public UnityEvent unityEvents;

    }

    [System.Serializable]
    public class DebugOptionList : ReorderableArray<DebugOption>
    { }

    public void CheckForDebugInput()
    {
        foreach(DebugOption option in debugOptionList)
        {
            if(Input.GetKeyDown(option.keyCode))
            {
                option.unityEvents.Invoke();
            }
        }
    }

    #endregion

    // Use this for initialization
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            if(instance != this)
                Destroy(this);

            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single && scene.name != "Loader")
        {
            SceneManager.MoveGameObjectToScene(gameObject, scene);
            StartRace();
        }
    }

    public void StartRace()
    {
        if(raceInfo.selectedTrack == null)
        {
            Debug.LogError("No TrackSelected in the GameManager.");
            return;
        }
        // create a copy of the trackAsset so we can change the variables at runtime without saving them.
        // raceInfo.selectedTrack = Instantiate(raceInfo.selectedTrack);
        // Initalize the Level.
        Initalize();

        //raceInfo.selectedTrack.Initalize(raceInfo.numberOfLaps);

        StartCoroutine(CountDown());
    }

    public void Update()
    {
        CheckForDebugInput();

        if (currentState == GameState.Racing)
            RaceUpdate();
    }

    public IEnumerator CountDown()
    {
        //Sets current state to Start
        currentState = GameState.RaceStart;

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

        TrackStats highScore  = highScoreAsset.GetOrCreate(raceInfo.selectedTrack.sceneName);
        if (highScore.ghost != null && highScore.ghost.lap.Count > 0)
        {
            playerGhostData.savedTrackData = highScore.ghost;
            playerGhostData.StartGhostingPlayerLaps();
        }

        CheckLastLap();

        //change game state to racing
        currentState = GameState.Racing;

        yield return null;
    }

    public void RaceUpdate()
    {
        raceInfo.currentLapTime += Time.deltaTime;
    }

    public void CompleteLap()
    {
        //debug to console, goto next lap, reset lap timer.
        Debug.Log("Lap: " + raceInfo.parseTimeFromSeconds(raceInfo.currentLapTime));

        raceInfo.lapTimes[raceInfo.currentLap] = raceInfo.currentLapTime;
        raceInfo.currentLap++;
        
        raceInfo.currentLapTime = 0.0f;

        //last lap check
        CheckLastLap();

        //end race check
        if (raceInfo.currentLap == raceInfo.numberOfLaps)
            EndRace();
    }

    public void EndRace()
    {
        currentState = GameState.RaceEnd;
        raceInfo.logStats();
        highScoreAsset.UpdateHighScores();
    }

    void CheckLastLap()
    {
        if (raceInfo.currentLap + 1 == raceInfo.numberOfLaps)
            Debug.Log("LAST LAP!");
    }

    public void Initalize()
    {
        GameObject trackStart = GameObject.FindGameObjectWithTag("StartPoint");

        // test to see if theres a startpoint to place the player at.
        if (trackStart == null)
        {
            Debug.LogError(string.Format("Track {0} doesnt have an object with the StartPoint tag. so it cant finnish setting up the level.", raceInfo.selectedTrack.sceneName));
            return;
        }

        if (raceInfo.numberOfLaps == 0)
        {
            raceInfo.numberOfLaps = raceInfo.selectedTrack.numberOfLaps;
        }

        raceInfo.lapTimes = new float[raceInfo.numberOfLaps];

        if (playerCarController == null)
            playerCarController = Instantiate(raceInfo.selectedCar).GetComponent<VehicleMovement>();

        if (raceInfo.selectedTrack.usePerTrackRotateToGroundAmount)
            playerCarController.rotateToGroundAmount = raceInfo.selectedTrack.perTrackRotateToGroundAmount;

        playerCarController.transform.position = trackStart.transform.position;
        playerCarController.transform.rotation = trackStart.transform.rotation;

        playerGhostData = playerCarController.GetComponent<GhostLapTracker>();

    }

    public void LoadLevel(TrackAsset trackAsset)
    {
        raceInfo.selectedTrack = trackAsset;
        DontDestroyOnLoad(gameObject);
        raceInfo.currentLap = 0;
        Menu m;
        m = GetComponentInChildren<Menu>();
        m.HideTabs();
        m.menuState = false;
        SceneManager.LoadScene(raceInfo.selectedTrack.sceneName, LoadSceneMode.Single);
    }

    public void ReloadLevel()
    {
        DontDestroyOnLoad(gameObject);
        raceInfo.currentLap = 0;
        Menu m;
        m = GetComponentInChildren<Menu>();
        m.HideTabs();
        m.menuState = false;
        SceneManager.LoadScene(raceInfo.selectedTrack.sceneName, LoadSceneMode.Single);
    }
}
