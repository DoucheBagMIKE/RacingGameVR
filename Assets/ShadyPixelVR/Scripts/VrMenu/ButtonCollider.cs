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
        if(other.gameObject.tag == "PlayerHands")
        {
            button.onClick.Invoke();
        }
    }
}
