using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VehicleGUI : MonoBehaviour {

    public Text mphText;
    public Slider thrustSlider;
    VehicleMovement movement;
    PlayerInput input;

	// Use this for initialization
	void Start () {

        movement = GetComponent<VehicleMovement>();
        input = GetComponent<PlayerInput>();

        if (mphText != null)
            mphText.text = "";

        if (thrustSlider != null)
            thrustSlider.value = 0;
	}
	
	// Update is called once per frame
	void Update () {

        if (GameManager.instance.currentState == GameManager.GameState.Menu || movement == null)
            return;

        if (mphText != null)
            UpdateMPH();

        if (thrustSlider != null)
            UpdateThrust();



    }

    public void UpdateThrust()
    {
        if (input == null)
            return;

        thrustSlider.value = input.thruster;
    }

    public void UpdateMPH()
    {
        float speed = Mathf.RoundToInt(Mathf.Abs( movement.speed));

        if (mphText != null)
            mphText.text = "" + speed;

    }
}
