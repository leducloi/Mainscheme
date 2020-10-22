using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionMenu : MonoBehaviour
{ 
    public Canvas menu; // Assign in inspector
    private bool isShowing; // Bool to determine if the menu should be visible or not
    public GameObject panel = null;

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
        RectTransform bounds = panel.GetComponent<RectTransform>();

        float tileSize = Camera.main.WorldToScreenPoint(new Vector3(1f, 1f, 0f)).x;


        //Get the position of the unit, both world and actual position
        Vector3 worldPosition = currUnit.transform.position;
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        Vector3 rectLocation = new Vector3();

        

        //Set up x coordinate
        //If the unit is on the left side of the map:
        if (screenPosition.x < Screen.width / 2)
        {
            //If there is room on the left side, place it on the left side
            if (screenPosition.x - tileSize > bounds.sizeDelta.x)
            {
                Vector2 setting = new Vector2(1, 1);
                bounds.anchorMin = setting;
                bounds.anchorMax = setting;
                bounds.pivot = setting;

                rectLocation.x = screenPosition.x - tileSize / 2;
            }
            //If not, place it on the right
            else
            {
                Vector2 setting = new Vector2(0, 1);
                bounds.anchorMin = setting;
                bounds.anchorMax = setting;
                bounds.pivot = setting;

                rectLocation.x = screenPosition.x + tileSize/2;
            }
        }
        //If the unit is on the right side of the map:
        else
        {
            //If there is room on the right side, place it on the right side
            if (Screen.width - bounds.sizeDelta.x - tileSize > screenPosition.x)
            {
                Vector2 setting = new Vector2(0, 1);
                bounds.anchorMin = setting;
                bounds.anchorMax = setting;
                bounds.pivot = setting;

                rectLocation.x = screenPosition.x + tileSize / 2;
            }
            //If not, place it on the left
            else
            {
                Vector2 setting = new Vector2(1, 1);
                bounds.anchorMin = setting;
                bounds.anchorMax = setting;
                bounds.pivot = setting;

                rectLocation.x = screenPosition.x - tileSize / 2;
            }
        }

        //Set up y coordinate
        if (!(screenPosition.y > bounds.sizeDelta.y + tileSize))
        {
            Vector2 setting = new Vector2(bounds.anchorMin.x, 0);
            bounds.anchorMin = setting;
            bounds.anchorMax = setting;
            bounds.pivot = setting;
        }
        rectLocation.y = screenPosition.y + tileSize / 2;
        rectLocation.z = 0f;

        bounds.position = rectLocation;
    }

}
