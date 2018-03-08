using UnityEngine;
using VRTK;
using UnityEngine.SceneManagement;

public class ReloadSceneOnTouchPadPressed : MonoBehaviour {

    VRTK_ControllerEvents left;
    VRTK_ControllerEvents right;

    // Use this for initialization
    void Start ()
    {
        left = VRTK_SDKManager.instance.scriptAliasLeftController.GetComponent<VRTK_ControllerEvents>();
        right = VRTK_SDKManager.instance.scriptAliasRightController.GetComponent<VRTK_ControllerEvents>();

        right.TouchpadPressed += TouchpadPressed;
        left.TouchpadPressed += TouchpadPressed;
    }

    private void TouchpadPressed(object sender, ControllerInteractionEventArgs e)
    {
        DontDestroyOnLoad(GameManager.instance.gameObject);
        GameManager.instance.raceInfo.currentLap = 0;
        SceneManager.LoadScene(GameManager.instance.raceInfo.selectedTrack.sceneName, LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        right.TouchpadPressed -= TouchpadPressed;
        left.TouchpadPressed -= TouchpadPressed;
    }
}
