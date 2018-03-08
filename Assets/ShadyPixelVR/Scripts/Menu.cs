using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Menu : MonoBehaviour
{
    [Tooltip("Object that holds the menu GameObjects as children.")]
    public GameObject menuHolder;
    [Tooltip("Object that holds the tab the menu will start on.")]
    public GameObject startTab;
    [Tooltip("If menu should be currently active.")]
    public bool menuState;
    VRTK_TransformFollow transformFollow;

    public void Start()
    {
        transformFollow = GetComponent<VRTK_TransformFollow>();

        if (menuState)
            OpenMenu();
        else
            CloseMenu();
    }

    public void SetTrackedObject(GameObject objectToTrack)
    {
        transformFollow.gameObjectToFollow = objectToTrack;
    }

    public void OpenMenu()
    {
        ShowTab(startTab);
    }

    public void CloseMenu()
    {
        HideTabs();
    }

    public void HideTabs()
    {
        //Loops through and sets all children of the menu to not active.
        for (int i = 0; i < menuHolder.transform.childCount; i++)
        {
            menuHolder.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void ShowTab(GameObject tabGameObject)
    {
        //Hides all tabs in the menu.
        HideTabs();

        //Sets the one tab to active.
        tabGameObject.SetActive(true);
    }
}
