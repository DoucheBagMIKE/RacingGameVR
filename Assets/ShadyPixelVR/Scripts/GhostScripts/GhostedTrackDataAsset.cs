using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class GhostedTrackDataAsset : ScriptableObject
{
    public List<string> keys;
    public List<GhostTrackData> values;

    private void Awake()
    {
        if (keys == null)
        {
            keys = new List<string>();
            values = new List<GhostTrackData>();
        }
    }
}