//This script handles reading inputs from the player and passing it on to the vehicle. We 
//separate the input code from the behaviour code so that we can easily swap controls 
//schemes or even implement and AI "controller". Works together with the VehicleMovement script

using UnityEngine;
using VRTK;
using VRTK.Controllables.ArtificialBased;

public class PlayerInput : MonoBehaviour
{
    public float turnValue;
    public float thrustValue;

    public enum InputScheme { VR, KeyBoard};
    public InputScheme inputScheme;
	//We hide these in the inspector because we want 
	//them public but we don't want people trying to change them
	[HideInInspector] public float thruster;			//The current thruster value
	[HideInInspector] public float rudder;				//The current rudder value
	[HideInInspector] public bool isBraking;            //The current brake value

    [Tooltip("Object that controls TURN input.")]
    public ArtificialRotatorExtended turnControlObject;
    public float deadzone = 0.05f;
    [Tooltip("Object that controls THRUST input.")]
    public VRTK_ArtificialSlider thrustControlObject;

    void Update()
	{
        if (turnControlObject != null)
        {
            //turnValue = Mathf.Lerp(-1f, 1f, turnControlObject.GetNormalizedValue());
            turnValue = turnControlObject.GetStepValue(turnControlObject.GetValue());
        }

        if (thrustControlObject != null)
            thrustValue = thrustControlObject.GetNormalizedValue();

		//If a GameManager exists and the game is not active...
		if (GameManager.instance != null && !GameManager.instance.IsActiveGame())
		{
			//...set all inputs to neutral values and exit this method
			thruster = rudder = 0f;
			isBraking = false;
			return;
		}

        //Get the values of the thruster, rudder, and brake from the input class
        if(inputScheme == InputScheme.VR)
        {
            thruster = thrustValue;
            rudder = turnValue;
        }
        else
        {
            thruster = Input.GetAxis("Vertical");
            rudder = Input.GetAxis("Horizontal");
        }
        
        isBraking = false; //Input.GetButton(brakingKey);
	}
}
