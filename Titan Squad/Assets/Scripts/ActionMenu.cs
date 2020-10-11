using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionMenu : MonoBehaviour
{ 
     public Canvas menu; // Assign in inspector
     private bool isShowing; // Bool to determine if the menu should be visible or not
    
    //I created 4 buttons for possible actions. We can remove some buttons if we do not need them. 
     //public Button attackButton;
     //public Button moveButton;//Not sure what you wanted to use for other buttons other than attack or wait
     //public Button abilityButton;
     //public Button waitButton;
     

    // Start is called before the first frame update
    void Start()//Starts with the menu being disabled
    {
        menu = GetComponent<Canvas>();
        isShowing = false;
        menu.enabled = (isShowing);
    }

    // Update is called once per frame
    void Update() {//Basic way of getting the menu to show up
        //Need to get it to only appear when a player unit is chosen
         if (Input.GetMouseButtonDown(0) ) {
             isShowing = !isShowing;//On every click it changes bool to hide the menu or show it
             menu.enabled = (isShowing);//This toggles whether it is visible to the player or not
             /*
             This needs to be fixed to where it only toggles when a pplayer character is chosen.
             It also needs to have button functionailty but I'm unsure if that needs to be done in this script.
             */

         }
    }

}
