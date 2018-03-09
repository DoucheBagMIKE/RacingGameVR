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

    
}
