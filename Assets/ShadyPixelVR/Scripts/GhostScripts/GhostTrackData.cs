using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GhostTrackData
{
    [HideInInspector]
    public string name;

    public List<GhostLapData> lap;

    // Use this for initialization
    public GhostTrackData()
    {
        //lap = new List<GhostLapData>();
        if(lap == null)
        {
            lap = new List<GhostLapData>();
        }
            
    }
}

