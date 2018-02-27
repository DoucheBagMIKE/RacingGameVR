using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRTK.GrabAttachMechanics;

[RequireComponent(typeof(VRTK_RotateTransformGrabAttach))]
public class SteeringWheel :  VRTK_InteractableObject {

    public Vector3 _grabPoint;
    VRTK_RotateTransformGrabAttach rotateGrabAttach;

    public TextMesh debugText;

    public float value { get { return rotateGrabAttach.GetAngle(); } }

    protected override void Awake()
    {
        base.Awake();
        rotateGrabAttach = GetComponent <VRTK_RotateTransformGrabAttach>();
    }
    protected override void FixedUpdate()
    {
        base.Update();

        if(debugText != null)
        {
            debugText.text = value.ToString();
        }
    }

    public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
    {
        base.OnInteractableObjectUngrabbed(e);
        rotateGrabAttach.ResetRotation(true);
    }

}
