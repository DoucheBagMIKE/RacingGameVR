using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonCollider : MonoBehaviour {

    public Button button;

	// Use this for initialization
	void Start () {

        button = GetComponent<Button>();
		
	}

    public void OnTriggerEnter(Collider other)
    {
        //if player touched button..
        if (other.gameObject.tag == "PlayerHands")
        {
            //..select object
            button.Select();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        //if player leaves trigger zone
        if (other.gameObject.tag == "PlayerHands")
        {
            //..invoke onClick events
            button.onClick.Invoke();
        }
    }
}
