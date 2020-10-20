using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
BUG: Menu does not appear if a player was not selected from the last time you load the game
*/
public class ActionMenu : MonoBehaviour
{ 
     public Canvas menu; // Assign in inspector
     private static bool isShowing; // Bool to determine if the menu should be visible or not
    
    //I created 4 buttons for possible actions. We can remove some buttons if we do not need them. 
     //public Button attackButton;
     //public Button moveButton;//Not sure what you wanted to use for other buttons other than attack or wait
     //public Button abilityButton;
     //public Button waitButton;
     

    // Start is called before the first frame update
    void Start()//Starts with the menu being disabled
    {
        menu = GetComponent<Canvas>();
        menu.enabled = (false);
        UIManager.instance.actionMenu.SetActive(false);//Is set to false on initialization so the menu doesn't automatically appear
    }

    // Update is called once per frame
    void Update() {//Basic way of getting the menu to show up
        isShowing = UIManager.instance.actionMenu.activeSelf;//Checks if the menu is active and whether to show it or not
        menu.enabled = (isShowing);//Shows or hides the menu
    }

}
