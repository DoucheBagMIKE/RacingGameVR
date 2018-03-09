using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GhostLapData
{
    public float lapTime;
    [HideInInspector]
    public List<Vector3> LapPositions;
    [HideInInspector]
    public List<Quaternion> lapRotations;

    public GhostLapData()
    {
        if(LapPositions == null)
        {
            LapPositions = new List<Vector3>();
            lapRotations = new List<Quaternion>();
        }
        
    }
}