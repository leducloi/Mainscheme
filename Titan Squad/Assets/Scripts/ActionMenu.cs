﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ActionMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{ 
    public Canvas menu; // Assign in inspector
    private bool isShowing; // Bool to determine if the menu should be visible or not
    public GameObject panel = null;

    public PlayerUnit currUnit;

    public List<Button> buttons;
    public Button[] buttons2;

    private int currButton;
    private int currButton2;

    private bool usingMouse = false;
    private bool actionMenu = true;

    public Objective currObjective;


    // Start is called before the first frame update
    void Start()//Starts with the menu being disabled
    {
        currButton = 0;
        currButton2 = 0;
        

        foreach (Button b in buttons2)
            b.gameObject.SetActive(false);
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
        if (menu.enabled && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)) && actionMenu)
        {
            UIManager.instance.playMenuDown();
            hideMenu();
            currUnit.deselected();
        }
        //Cancel attack select
        if (currUnit != null && currUnit.canAttack && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
        {
            UIManager.instance.playMenuDown();
            menu.enabled = true;
            currUnit.canAttack = false;
            CameraBehavior.instance.pauseWASD = true;
            UIManager.instance.clearOutlines();
        }
        //Cancel move command
        if (currUnit != null && currUnit.canMove && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)) && !currUnit.bonusMove)
        {
            UIManager.instance.playMenuDown();
            menu.enabled = true;
            CameraBehavior.instance.pauseWASD = true;
            currUnit.canMove = false;
            PathArrowControl.instance.destroyAllArrows();
            MapBehavior.instance.deleteHighlightTiles();
        }

        if (menu.enabled && !usingMouse)
        {
            if (actionMenu)
            {
                if (Input.GetKeyDown("w") && currButton > 0)
                {
                    buttons[currButton].OnDeselect(null);
                    currButton--;
                    if (!buttons[currButton].interactable)
                    {
                        if (currButton > 0)
                            currButton--;
                        else
                            currButton++;
                    }
                    buttons[currButton].OnSelect(null);
                }
                else if (Input.GetKeyDown("s") && currButton < buttons.Count)
                {
                    buttons[currButton].OnDeselect(null);
                    currButton++;
                    if (!buttons[currButton].interactable)
                        currButton++;
                    buttons[currButton].OnSelect(null);
                }
                else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    buttons[currButton].onClick.Invoke();
                }
            }
            else
            {
                if (Input.GetKeyDown("w") && currButton2 > 0)
                {
                    buttons2[currButton2].OnDeselect(null);
                    currButton2--;
                    if (!buttons2[currButton2].interactable)
                        currButton2--;
                    buttons2[currButton2].OnSelect(null);
                }
                else if (Input.GetKeyDown("s") && currButton2 < buttons2.Length)
                {
                    buttons2[currButton2].OnDeselect(null);
                    currButton2++;
                    if (!buttons2[currButton2].interactable)
                        currButton2++;
                    buttons2[currButton2].OnSelect(null);
                }
                else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    buttons2[currButton2].onClick.Invoke();
                }
            }
        }

        if (menu.enabled && !actionMenu && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
        {
            UIManager.instance.playMenuDown();
            switchToActionMenu();
        }
    }

    public void switchToActionMenu()
    {
        actionMenu = true;

        foreach (Button b in buttons2)
            b.gameObject.SetActive(false);
        foreach (Button b in buttons)
            b.gameObject.SetActive(true);

        if (currObjective == null)
            buttons[0].gameObject.SetActive(false);

        if (Level.instance.isTutorial)
            buttons[4].gameObject.SetActive(false);

        if (buttons[4].gameObject.activeInHierarchy)
        {
            if (MapBehavior.instance.getAlliesInRange(currUnit.transform.position, 1).Count > 1)
                buttons[5].gameObject.SetActive(true);
            else
                buttons[5].gameObject.SetActive(false);
        }
        else
            buttons[5].gameObject.SetActive(false);

        currButton = 0;
        currButton2 = 0;

        menu.enabled = true;
    }

    public void switchToAbilityMenu()
    {
        actionMenu = false;

        foreach (Button b in buttons2)
            b.interactable = true;

        foreach (Button b in buttons)
            b.gameObject.SetActive(false);
        foreach (Button b in buttons2)
            b.gameObject.SetActive(true);

        for(int x = 0; x < currUnit.abilityNames.Length; x++)
            buttons2[x].GetComponentInChildren<Text>().text = currUnit.abilityNames[x];

        currButton = 0;
        currButton2 = 0;

        if (currUnit.usingAbility1)
            buttons2[0].interactable = false;
        if (currUnit.usingAbility2)
            buttons2[1].interactable = false;
        if (currUnit.usingAbility3 || currUnit.actionPoints < 2)
            buttons2[2].interactable = false;

        menu.enabled = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        usingMouse = true;
        buttons[currButton].OnDeselect(null);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        usingMouse = false;
        buttons[currButton].OnSelect(null);
    }


    public void displayMenu(GameObject unitSelected)
    {
        currButton = 0;
        currUnit = unitSelected.GetComponent<PlayerUnit>();

        if (Level.instance.isTutorial)
            buttons[4].gameObject.SetActive(false);
        else
            buttons[4].gameObject.SetActive(true);

        if (buttons[4].gameObject.activeInHierarchy)
        {
            if (MapBehavior.instance.getAlliesInRange(currUnit.transform.position, 1).Count > 1)
                buttons[5].gameObject.SetActive(true);
            else
                buttons[5].gameObject.SetActive(false);
        }
        else
            buttons[5].gameObject.SetActive(false);
        
        buttons[2].interactable = true;
        smartMenuPosition();
        CameraBehavior.instance.pauseWASD = true;
        UIManager.instance.currUnit = currUnit;

        getInteractableObjective();
        if (currObjective == null)
            buttons[0].gameObject.SetActive(false);
        else
        {
            buttons[0].gameObject.SetActive(true);
            buttons[0].GetComponentInChildren<Text>().text = currObjective.interactText;
        }


        buttons[currButton].OnSelect(null);
        if (MapBehavior.instance.getUnitsInRange(currUnit.transform.position, currUnit.equippedWeapon.maxRange, currUnit.equippedWeapon.minRange).Count == 0)
        {
            buttons[2].interactable = false;
        }
        StartCoroutine(finishDraw());
    }

    public void hideMenu()
    {
        menu.enabled = false;
        if (currUnit != null && !currUnit.canAttack && !currUnit.canMove && !currUnit.selectAbility)
            currUnit.deselected();
        CameraBehavior.instance.pauseWASD = false;
        foreach (Button b in buttons2)
            b.interactable = true;
    }

    public void enableMove()
    {
        usingMouse = false;
        currUnit.canMove = true;
        menu.enabled = false;
        CameraBehavior.instance.pauseWASD = false;
    }

    public void beginAttack(List<Unit> unitsInRange)
    {
        usingMouse = false;
        currUnit.canAttack = true;
        StartCoroutine(currUnit.selectTarget(unitsInRange));
        CameraBehavior.instance.pauseWASD = false;
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


        //float tileSize = Camera.main.WorldToScreenPoint(new Vector3(1f, 1f, 0f)).x;


        //Get the position of the unit, both world and actual position
        Vector3 worldPosition = currUnit.transform.position;
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        Vector3 rectLocation = new Vector3();

        

        float tileSize = Camera.main.WorldToScreenPoint(worldPosition + new Vector3(1f, 0f, 0f)).x - screenPosition.x;
        
        

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

            rectLocation.y = screenPosition.y;
        }
        else
            rectLocation.y = screenPosition.y + tileSize / 2;
        rectLocation.z = 0f;

        bounds.position = rectLocation;
    }

    IEnumerator finishDraw()
    {
        yield return new WaitForSeconds(0.025f);
        menu.enabled = true;
    }

    void getInteractableObjective()
    {
        CollisionTile playerTile = MapBehavior.instance.getTileAtPos(currUnit.transform.position);
        foreach (GameObject objective in Level.instance.activeObjectives)
        {
            foreach (CollisionTile tile in objective.GetComponent<Objective>().tilesToHighlight)
                if (tile.coordinate == playerTile.coordinate)
                {
                    currObjective = objective.GetComponent<Objective>();
                    return;
                }
        }
        currObjective = null;
    }

    public void callInventory()
    {
        List <PlayerUnit> unit = new List<PlayerUnit>();
        unit.Add(currUnit);
        UIManager.instance.showInventory(unit);
        menu.enabled = false;
    }

    public void callTrade()
    {
        List<PlayerUnit> units = new List<PlayerUnit>();
        foreach(Unit u in MapBehavior.instance.getAlliesInRange(currUnit.transform.position, 1))
        {
            units.Add((PlayerUnit)u);
        }
        InventoryManager.instance.trading = true;
        UIManager.instance.showInventory(units);
        
        menu.enabled = false;
    }
}
