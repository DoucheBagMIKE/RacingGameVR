using UnityEngine;
[System.Serializable]
public class TrackStats
{
    public string name;
    public float totalPlayTime;
    public HighScoreEntry fastestLap;
    public HighScoreEntry fastestTotal;
    //[HideInInspector]
    public GhostTrackData ghost;

}

