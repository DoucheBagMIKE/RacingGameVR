using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class MenuToggle : MonoBehaviour {

    public VRTK_ControllerEvents controllerEvents;
    public Menu menuObject;

    public void OnEnable()
    {
        controllerEvents.ButtonOnePressed += ControllerEvents_ButtonOnePressed;
    }
    public void OnDisable()
    {
        controllerEvents.ButtonOnePressed -= ControllerEvents_ButtonOnePressed;
    }

    public void ControllerEvents_ButtonOnePressed(object sender, ControllerInteractionEventArgs e)
    {
        //find the new state of the menu..
        bool newState = !menuObject.menuState;
        //..set the menus target state to new state..
        menuObject.menuState = newState;

        if(newState == true)
        {
            //..should the menu open..
            //..set the tracked object to the controller that hit the button..
            menuObject.SetTrackedObject(e.controllerReference.scriptAlias);
            //..open the menu
            menuObject.OpenMenu();
        }
        else
        {
            //..should the menu close..

            //..close the menu
            menuObject.CloseMenu();
        }
    }
}
