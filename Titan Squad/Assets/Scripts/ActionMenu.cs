using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionMenu : MonoBehaviour
{ 
    public Canvas menu; // Assign in inspector
    private bool isShowing; // Bool to determine if the menu should be visible or not

    public PlayerUnit currUnit;
    
     

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
        if (menu.enabled && currUnit != null)
            smartMenuPosition();
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
        currUnit = unitSelected.GetComponent<PlayerUnit>();
        smartMenuPosition();
        menu.enabled = true;
        
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

    //Positions the menu intelligently based on unit position
    void smartMenuPosition()
    {
        //If the currUnit is somehow not set, return out
        if (currUnit == null)
        {
            Debug.Log("currUnit = null, no position update");
            return;
        }

        //Get bounds for our menu
        RectTransform bounds = GetComponent<RectTransform>();

        //Get the position of the unit, both world and actual position
        Vector3 unitPos = currUnit.transform.position;
        Vector3 actualPos = Camera.main.WorldToScreenPoint(unitPos);

        //Check if the unit is on the left or the right side of the screen
        if (actualPos.x < Camera.main.pixelWidth / 2)
        {
            //If there's room on the left, place the menu on the left
            if (actualPos.x - 16 > bounds.rect.width)
                unitPos += new Vector3(-5f, 0f, 0f);
            else
                unitPos += new Vector3(.5f, 0f, 0f);
        }
        else
        {
            //If there's room on the right, place the menu on the right
            if (actualPos.x + bounds.rect.width + 16 < Camera.main.pixelWidth)
                unitPos += new Vector3(.5f, 0f, 0f);
            else
                unitPos += new Vector3(-5f, 0f, 0f);
        }


        //If the unit is too low for the menu to show up properly, move it up to fit
        if (actualPos.y < bounds.rect.height)
        {
            actualPos.y = bounds.rect.height + 10;
        }
        else
        {
            actualPos.y += 16;
        }
        actualPos = Camera.main.ScreenToWorldPoint(actualPos);

        transform.position = new Vector3(unitPos.x, actualPos.y, 0f);
    }

}
