using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(
    fileName = "ShadyPixel/Track", 
    menuName = "TrackConfig")]

public class TrackAsset : ScriptableObject {

    [Title("Track Config")]
    public string sceneName;

    [Range(0,12), PropertyTooltip("Default number of laps the track will run for.")]
    public int numberOfLaps = 4;

    [PropertyTooltip("If enabled the perTrackRotateToGroundAmount is used instead of the one in the car controler, to smooth the players view.")]
    public bool usePerTrackRotateToGroundAmount = false;

    [ShowIf("usePerTrackRotateToGroundAmount"), HideLabel, Indent]
    public float perTrackRotateToGroundAmount;

    public bool useOnGroundGravity;
    [ShowIf("useOnGroundGravity"), HideLabel, Indent]
    public float onGroundGravity;

    public bool useInAitGravity;
    [ShowIf("useInAitGravity"), HideLabel, Indent]
    public float inAirGravity;

    
}
