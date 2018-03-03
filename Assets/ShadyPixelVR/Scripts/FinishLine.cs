using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour {

    [Tooltip("A gameobject with a 'Checkpoint' component will set this to true when 'PlayerSensor' gameobject passes through the checkpoint.")]
    public bool readyToCompleteLap;

    void OnTriggerEnter(Collider other)
    {
        //if player crosses finish line
        if(other.tag == "PlayerSensor")
        {
            //..and passed through the checkpoint
            if(readyToCompleteLap)
            {
                CompleteLap();
            }
        }
    }

    public void CompleteLap()
    {
        readyToCompleteLap = false;
        GameManager.instance.CompleteLap();
    }
}
