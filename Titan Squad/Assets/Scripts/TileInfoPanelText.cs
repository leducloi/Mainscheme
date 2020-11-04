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

    public RectTransform bounds;
    //Text UI objects used to display the text on screen
    public Text tileTypeText;
    public Text tileCostText;
    public Text tileDodgeText;
    public Text objectiveText;
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

        tile = MapBehavior.instance.getTileAtPos(Camera.main.ScreenToWorldPoint(Input.mousePosition)); 
        if (tile != null)
        {
            string cost = (tile.tileCost == 99) ? "--" : "" + tile.tileCost;
            tileTypeText.text = "Type: " + tile.tileType;
            tileCostText.text = "Cost: " + cost;
            tileDodgeText.text = "Dodge: " + tile.tileDodge;
            objectiveText.text = "Pos: " + tile.coordinate;
        }
        
      
    }

    void smartPosition()
    {
        Vector3 mouse = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        Vector3 moveTo;
        Vector3 settings;
        if (mouse.x > .5f && Level.instance.donePlanning)
        {
            if (mouse.y < .5f)
            {
                settings = new Vector3(1, 1, 0);
                bounds.anchorMin = settings;
                bounds.anchorMax = settings;
                bounds.pivot = settings;
                moveTo = new Vector3(Screen.width - Screen.height * 0.02f, Screen.height - Screen.height * 0.02f, 0);
                bounds.position = moveTo;
                return;
            }
        }
        settings = new Vector3(1, 0, 0);
        bounds.anchorMin = settings;
        bounds.anchorMax = settings;
        bounds.pivot = settings;

        moveTo = new Vector3(Screen.width - Screen.height * 0.02f, Screen.height * .02f, 0);
        bounds.position = moveTo;
    }

}
