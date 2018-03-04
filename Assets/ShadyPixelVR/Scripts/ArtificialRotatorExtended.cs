using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRTK.Controllables.ArtificialBased;


public class ArtificialRotatorExtended : VRTK_ArtificialRotator
{
    public bool _grabbed;

    protected override void InteractableObjectGrabbed(object sender, InteractableObjectEventArgs e)
    {
        base.InteractableObjectGrabbed(sender, e);
        _grabbed = true;
    }

    protected override void InteractableObjectUngrabbed(object sender, InteractableObjectEventArgs e)
    {
        base.InteractableObjectUngrabbed(sender, e);
        _grabbed = false;
    }
}
