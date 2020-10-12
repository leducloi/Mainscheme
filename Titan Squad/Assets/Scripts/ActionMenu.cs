using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionMenu : MonoBehaviour
{ 
    public Canvas menu; // Assign in inspector
    private bool isShowing; // Bool to determine if the menu should be visible or not

    public PlayerUnit currUnit;
    
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
        UIManager.instance.currUnit = currUnit;
    }

    // Update is called once per frame
    void Update()
    {
        //Cancel unit select
        if (menu.enabled && Input.GetKeyDown(KeyCode.Escape))
        {
            menu.enabled = false;
            currUnit.deselected();
            currUnit = null;
            UIManager.instance.currUnit = currUnit;
        }
        //Cancel attack select
        if (currUnit != null && currUnit.canAttack && Input.GetKeyDown(KeyCode.Escape))
        {
            menu.enabled = true;
            currUnit.canAttack = false;
            UIManager.instance.clearOutlines();
        }
        //Cancel move command
        if (currUnit != null && currUnit.canMove && Input.GetKeyDown(KeyCode.Escape))
        {
            menu.enabled = true;
            currUnit.canMove = false;
        }
    }

    public void displayMenu(GameObject unitSelected)
    {
        menu.enabled = true;
        currUnit = unitSelected.GetComponent<PlayerUnit>();
        UIManager.instance.currUnit = currUnit;
    }

    public void enableMove()
    {
        currUnit.canMove = true;
        menu.enabled = false;
    }

    public void beginAttack(List<Unit> unitsInRange)
    {
        currUnit.canAttack = true;
        StartCoroutine(currUnit.selectTarget(unitsInRange));
        menu.enabled = false;
    }

}
