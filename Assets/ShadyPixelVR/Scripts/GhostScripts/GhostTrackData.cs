using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GhostTrackData
{
    [HideInInspector]
    public string name;

    [HideInInspector]
    public List<GhostLapData> lap;

    // Use this for initialization
    private void Awake()
    {
        lap = new List<GhostLapData>();
        if(lap == null)
        {
            lap = new List<GhostLapData>();
        }
            
    }
}

