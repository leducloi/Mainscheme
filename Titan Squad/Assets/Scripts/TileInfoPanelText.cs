using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
This script is used for the UI for giving the tile data. It shows the stats of the tile's name, tile's movement cost, and tile's dodge cost.
This should update the text on wherever the mouse is on top of that is within bounds of the camera.
Text size for the UI, font, and other design aspects still need to be worked on.
*/

public class TileInfoPanelText : MonoBehaviour
{

    public RectTransform bottomBounds;
    public RectTransform topBounds;
    //Text UI objects used to display the text on screen
    public Text tileTypeText;
    public Text tileCostText;
    public Text tileDodgeText;
    public Text objectiveText;
    public Text turnsText;
    //This collision tile is used below to find the tile that the mouse is hovering over.
    CollisionTile tile;
    
    //This was taken from camerabehavior script and is used to check if the mouse is within camera bounds
    float moveUnit;
    
    void Awake()
    { 
      //Initailizes the text objects
        tileTypeText.text = "Terrain Type: ";
        tileCostText.text = "Terrain Cost: ";
        tileDodgeText.text = "Terrain Dodge: ";
        objectiveText.text = "--";
    }

    // Update is called once per frame
    void Update()
    {
        smartPosition();

        if (MapBehavior.instance == null)
            return;

        if (Level.instance.turnLimit == int.MaxValue)
            turnsText.transform.parent.gameObject.SetActive(false);
        else
            turnsText.text = "" + (Level.instance.turnLimit - GameManager.instance.turnCount);

        tile = MapBehavior.instance.getTileAtPos(Camera.main.ScreenToWorldPoint(Input.mousePosition)); 
        if (tile != null)
        {
            string cost = (tile.tileCost == 99) ? "--" : "" + tile.tileCost;
            tileTypeText.text = "Type: " + tile.tileType;
            tileCostText.text = "Cost: " + cost;
            tileDodgeText.text = "Dodge: " + tile.tileDodge;
        }
        setObjectiveText();
        
    }

    void smartPosition()
    {
        float pos = 0.02f;

        Vector3 mouse = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        Vector3 moveTo;
        Vector3 settings;
        if (mouse.x > .5f && Level.instance.donePlanning)
        {
            if (mouse.y < .5f)
            {
                settings = new Vector3(0, 0, 0);
                bottomBounds.anchorMin = settings;
                bottomBounds.anchorMax = settings;
                bottomBounds.pivot = settings;
                moveTo = new Vector3(Screen.height * pos, Screen.height * pos, 0);
                bottomBounds.position = moveTo;

                settings = new Vector3(0, 1, 0);
                topBounds.anchorMin = settings;
                topBounds.anchorMax = settings;
                topBounds.pivot = settings;
                moveTo = new Vector3(Screen.height * pos, Screen.height - Screen.height * pos, 0);
                topBounds.position = moveTo;

                return;
            }
        }
        settings = new Vector3(1, 0, 0);
        bottomBounds.anchorMin = settings;
        bottomBounds.anchorMax = settings;
        bottomBounds.pivot = settings;

        moveTo = new Vector3(Screen.width - Screen.height * pos, Screen.height * pos, 0);
        bottomBounds.position = moveTo;

        if (Level.instance.donePlanning)
        {
            settings = new Vector3(1, 1, 0);
            topBounds.anchorMin = settings;
            topBounds.anchorMax = settings;
            topBounds.pivot = settings;

            moveTo = new Vector3(Screen.width - Screen.height * pos, Screen.height - Screen.height * pos, 0);
            topBounds.position = moveTo;
        }
        else
        {
            settings = new Vector3(0, 1, 0);
            topBounds.anchorMin = settings;
            topBounds.anchorMax = settings;
            topBounds.pivot = settings;

            moveTo = new Vector3(Screen.height * pos, Screen.height - Screen.height * pos, 0);
            topBounds.position = moveTo;
        }
    }

    void setObjectiveText()
    {
        string objText = "";
        foreach (GameObject obj in Level.instance.activeObjectives)
        {
            Objective objective = obj.GetComponent<Objective>();
            switch (objective.type)
            {
                case "Route":
                    if (Level.instance.getObjectiveCount("Route") == 0)
                    {
                        objective.completeObjective();
                        return;
                    }
                    objText += objective.description + " (" + Level.instance.getObjectiveCount(objective.type) + ")";
                    break;
                case "Assassination":
                case "Sabotage":
                case "Rescue":
                    objText += objective.description + " (" + Level.instance.getObjectiveCount(objective.type) + ")";
                    break;
                case "Data Retrieval":
                case "Exfill":
                    objText += objective.description;
                    break;

            }

            objText += '\n';
        }
        objText.Remove(objText.Length - 1);
        objectiveText.text = objText;
    }
}
