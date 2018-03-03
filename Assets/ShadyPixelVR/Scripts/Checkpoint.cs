using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {

    [Tooltip("Finish line component. Will try to set at runtime if null.")]
    public FinishLine finishLine;

	// Use this for initialization
	void Start () {

        //if null at start, try to find one.
        if (finishLine == null)
            finishLine = FindObjectOfType<FinishLine>();
		
	}

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "PlayerSensor")
        {
            finishLine.readyToCompleteLap = true;
        }
    }
}
