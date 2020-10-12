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
    //Text UI objects used to display the text on screen
    public Text tileTypeText;
    public Text tileCostText;
    public Text tileDodgeText;
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
        
    }

    // Update is called once per frame
    void Update()
    {  
      //This is borrowed from the cameraBehavior script and could probably be cleaned up but it works as needed
      //Without it, whenever the mouse is out of bounds you receieve errors.
      moveUnit = MapBehavior.instance.getGridCellSize();
      //if (Input.mousePosition.y >= Screen.height - moveUnit)
      //  {
      //      return;
      //  }
      //if (Input.mousePosition.y <= moveUnit)
      //  {
      //    return;
      //  }
      //if (Input.mousePosition.x >= Screen.width - moveUnit)
      //  {
      //      return;
      //  }
      //if (Input.mousePosition.x <= moveUnit)
      //  {
      //     return; 
      //  }
      //else{
        tile = MapBehavior.instance.getTileAtPos(Camera.main.ScreenToWorldPoint(Input.mousePosition)); 
        if (tile != null)
            {
                string cost = (tile.tileCost == 99) ? "--" : "" + tile.tileCost;
                tileTypeText.text = "Terrain Type: " + tile.tileType;
                tileCostText.text = "Terrain Cost: " + cost;
                tileDodgeText.text = "Terrain Dodge: " + tile.tileDodge;
            }
        
      
    }

}
