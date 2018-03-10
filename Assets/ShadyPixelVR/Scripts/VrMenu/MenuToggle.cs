using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class MenuToggle : MonoBehaviour {

    public VRTK_ControllerEvents controllerEvents;
    public Menu menuObject;

    public void Awake()
    {
        if (menuObject == null)
            menuObject = GameManager.instance.vrMenu;
    }

    public void OnEnable()
    {
        controllerEvents.ButtonTwoPressed += ControllerEvents_ButtonTwoPressed;
    }
    public void OnDisable()
    {
        controllerEvents.ButtonTwoPressed -= ControllerEvents_ButtonTwoPressed;
    }

    public void ControllerEvents_ButtonTwoPressed(object sender, ControllerInteractionEventArgs e)
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
