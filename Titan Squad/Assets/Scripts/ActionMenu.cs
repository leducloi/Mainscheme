using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionMenu : MonoBehaviour
{ 
     public Canvas menu; // Assign in inspector
     private bool isShowing; // Bool to determine if the menu should be visible or not

    private PlayerUnit currUnit;
    
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
        currUnit = null;
    }

    // Update is called once per frame
    void Update() {//Basic way of getting the menu to show up
        if (menu.enabled && Input.GetKeyDown(KeyCode.Escape))
        {
            menu.enabled = false;
            currUnit.deselected();
        }
    }

    public void displayMenu(GameObject unitSelected)
    {
        menu.enabled = true;
        currUnit = unitSelected.GetComponent<PlayerUnit>();
    }

    public void enableMove()
    {
        currUnit.canMove = true;
        menu.enabled = false;
    }

}
