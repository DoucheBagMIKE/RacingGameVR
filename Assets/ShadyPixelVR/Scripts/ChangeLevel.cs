using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeLevel : MonoBehaviour {

    public void LoadLevel(TrackAsset trackAsset)
    {
        GameManager.instance.LoadLevel(trackAsset);
    }

    public void ReloadLevel()
    {
        GameManager.instance.ReloadLevel();
    }
}
