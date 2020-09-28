using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/*
 * -----------------SCRIPT INFORMATION-----------------
 * This script is only a class. The purpose of this class is to hold tile information for later ease of access. This class not only
 * holds the information, but also holds tile names of certain categories of tiles. Since the actual name of the tile
 * is inconsequential for the game and only serves an aesthetic variation, it is more useful to categorize the tiles variants in a 
 * way relevant to the game. This class makes on its own that transition from useless tile name to useful tile category, notated
 * in the class as tileType.
 * ----------------------------------------------------
 */

public class CollisionTile
{
    //Tile Type: This is the categorical type of the tile. For instance, there are many mountain sprites, but they are all Mountains
    public string tileType;
    //Tile Cost: This is the cost to move through the tile. ******Non-zero*******
    public int tileCost;
    //Tile Dodge: This is the bonus the tile gives to an occupying unit's dodge chance
    public int tileDodge;
    //X: The x and y coordinates of the tile
    public Vector3 coordinate;

    //These static variables hold the names of tiles that occupy the same categorical tile type
    private static string[] grassTiles = {"TilesetExample_6", "TilesetExample_7", "TilesetExample_15", "TilesetExample_16",
                                            "TilesetExample_38"};
    private static string[] sandTiles = { "TilesetExample_25", "TilesetExample_26", "TilesetExample_27",
                                            "TilesetExample_32", "TilesetExample_33", "TilesetExample_34",
                                              "TilesetExample_41", "TilesetExample_42", "TilesetExample_43"};


    public CollisionTile(string tileName, float tileX, float tileY)
    {
        coordinate = new Vector3(tileX, tileY, 0);

        //If the tile is a Grasslands tile, set its statistics to that of grasslands
        foreach (string name in grassTiles)
        {
            if (tileName.Equals(name))
            {
                tileType = "Grasslands";
                tileCost = 1;
                tileDodge = 0;
                return;
            }
        }
        //If the tile is a Desert tile, set its statistics to that of desert
        foreach (string name in sandTiles)
        {
            if (tileName.Equals(name))
            {
                tileType = "Desert";
                tileCost = 2;
                tileDodge = -10;
                return;
            }
        }
        //TODO - Other tile types

        //Tiles not in a category are simply marked impassible.
        tileType = "Impassible";
        tileCost = 99;
        tileDodge = 0;
    }

    public string toString()
    {
        return "Tile Type: " + tileType + ", X: " + coordinate.x + ", Y: " + coordinate.y + ", Tile Cost: " + tileCost;
    }
}
