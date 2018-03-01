//This script handles reading inputs from the player and passing it on to the vehicle. We 
//separate the input code from the behaviour code so that we can easily swap controls 
//schemes or even implement and AI "controller". Works together with the VehicleMovement script

using UnityEngine;
using VRTK;
using VRTK.Controllables.ArtificialBased;

public class PlayerInput : MonoBehaviour
{
	public string verticalAxisName = "Vertical";        //The name of the thruster axis
	public string horizontalAxisName = "Horizontal";    //The name of the rudder axis
	public string brakingKey = "Brake";                 //The name of the brake button

    public VRTK_ArtificialSlider verticalAxisController;
    public VRTK_ArtificialSlider horizontalAxisController;


	//We hide these in the inspector because we want 
	//them public but we don't want people trying to change them
	[HideInInspector] public float thruster;			//The current thruster value
	[HideInInspector] public float rudder;				//The current rudder value
	[HideInInspector] public bool isBraking;			//The current brake value

	void Update()
	{
		//If the player presses the Escape key and this is a build (not the editor), exit the game
		if (Input.GetButtonDown("Cancel") && !Application.isEditor)
			Application.Quit();

		//If a GameManager exists and the game is not active...
		if (GameManager.instance != null && !GameManager.instance.IsActiveGame())
		{
			//...set all inputs to neutral values and exit this method
			thruster = rudder = 0f;
			isBraking = false;
			return;
		}

        //Get the values of the thruster, rudder, and brake from the input class

        //thruster
        if (Mathf.Abs(verticalAxisController.GetNormalizedValue()) > Mathf.Abs(Input.GetAxis(verticalAxisName)) && verticalAxisController!=null)
            thruster = verticalAxisController.GetNormalizedValue();
        else
            thruster = Input.GetAxis(verticalAxisName);

        //rudder
        float val = horizontalAxisController.GetStepValue(horizontalAxisController.GetValue());
        if (Mathf.Abs(val) > Mathf.Abs(Input.GetAxis(horizontalAxisName)) && horizontalAxisController !=null)
            rudder = val;
        else
            rudder = Input.GetAxis(horizontalAxisName);

        //brake
        isBraking = Input.GetButton(brakingKey);
	}
}
