using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevelOnStart : MonoBehaviour {

    private void Start()
    {
        SceneManager.LoadScene(GameManager.instance.raceInfo.selectedTrack.sceneName, LoadSceneMode.Single);
    }
}
