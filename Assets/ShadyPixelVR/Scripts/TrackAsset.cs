using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(
    fileName = "ShadyPixel/Track", 
    menuName = "TrackConfig")]

public class TrackAsset : ScriptableObject {

    [Header("Track Config")]
    public string sceneName;

    [Tooltip("Default number of laps the track will run for.")]
    public int numberOfLaps = 4;
    [Tooltip("If enabled the perTrackRotateToGroundAmount is used instead of the one in the car controler, to smooth the players view.")]
    public bool usePerTrackRotateToGroundAmount = false;
    [Tooltip("modifer for the players rotateToGroundAmount, that controls how snappy the player view is.")]
    public float perTrackRotateToGroundAmount;

    public bool useOnGroundGravity;
    public float onGroundGravity;

    public bool useInAitGravity;
    public float inAirGravity;

    public void Initalize(int laps= 0)
    {
        GameManager gm = GameManager.instance;
        RaceInfo raceInfo = GameManager.instance.raceInfo;

        GameObject trackStart = GameObject.FindGameObjectWithTag("StartPoint");

        // test to see if theres a startpoint to place the player at.
        if (trackStart == null)
        {
            Debug.LogError(string.Format("Track {0} doesnt have an object with the StartPoint tag. so it cant finnish setting up the level.", sceneName));
            return;
        }

        if(laps == 0)
        {
            raceInfo.numberOfLaps = numberOfLaps;
        }
        else
        {
            raceInfo.numberOfLaps = laps;
        }

        raceInfo.lapTimes = new float[raceInfo.numberOfLaps];

        if (gm.playerCarController == null)
            gm.playerCarController = Instantiate(raceInfo.selectedCar).GetComponent<VehicleMovement>();

        if (usePerTrackRotateToGroundAmount)
            gm.playerCarController.rotateToGroundAmount = perTrackRotateToGroundAmount;

        gm.playerCarController.transform.position = trackStart.transform.position;
        gm.playerCarController.transform.rotation = trackStart.transform.rotation;

    }
}
