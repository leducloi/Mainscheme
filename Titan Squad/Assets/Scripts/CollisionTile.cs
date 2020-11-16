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

    //Bools to track what tiles contain units
    public bool hasEnemy;
    public bool hasPlayer;

    //Bool to track if the tile can be walked on - default true
    public bool passable = true;

    //If the tile is not passableEW, coming into the tile from the East is impossible and leaving the tile to the East is impossible
    public bool passableEW = true;
    //If the tile is not passableNS, coming into the tile from the North is impossible and leaving the tile to the North is impossible
    public bool passableNS = true;

    public bool lowCover = false;
    public bool highCover = false;

    //Use for A* algorithm
    public int gCost;
    public int fCost;
    public int hCost;
    //Connect a tile to the previous tile where it is from
    public CollisionTile cameFromTile;


    //These static variables hold the names of tiles that occupy the same categorical tile type
    private static string[] grassTiles = {"grasstile1", "grasstile2", "grasstile3", "grasstile4"};
    private static string[] sandTiles = { "dirttile1", "dirttile2", "dirttile3"};


    public CollisionTile(string tileName, float tileX, float tileY, string obName = null)
    {
        coordinate = new Vector3(tileX, tileY, 0);
        hasEnemy = false;
        hasPlayer = false;

        if (obName != null)
        {
            if (obName.Contains("wa"))
                highCover = true;
            if (obName.Contains("NS"))
                passableNS = false;
            if (obName.Contains("EW"))
                passableEW = false;
            if (!passableEW && !passableNS)
                passable = false;
        }

        //If the tile is a Grasslands tile, set its statistics to that of grasslands
        
        if (tileName.Contains("grass"))
        {
            tileType = "Grass";
            tileCost = 1;
            tileDodge = 0;
            return;
        }
        //If the tile is a Desert tile, set its statistics to that of desert
        if (tileName.Contains("dirt"))
        {
            tileType = "Dirt";
            tileCost = 2;
            tileDodge = -10;
            return;
        }
        if (tileName.Contains("concrete"))
        {
            tileType = "Floor";
            tileCost = 1;
            tileDodge = 0;
            return;
        }
        if (tileName.Contains("river"))
        {
            tileType = "River";
            tileCost = 4;
            tileDodge = -20;
            return;
        }
        if (tileName.Contains("Void"))
        {
            tileType = null;
            tileCost = 99;
            tileDodge = 0;
            passable = false;
            return;
        }
        //TODO - Other tile types

        //Tiles not in a category are simply marked impassible.
        tileType = "Water";
        passable = false;
        tileCost = 99;
        tileDodge = 0;
    }

    public bool isWalkable()
    {
        if (passable)
        {
            if (GameManager.instance.enemyPhase)
                return !hasPlayer;
            if (GameManager.instance.playerPhase)
                return !hasEnemy;
        }
        return false;        
    }

    

    public CollisionTile calculateFCost()
    {
        fCost = hCost + gCost;
        return this;
    }

    public string toString()
    {
        return "Tile Type: " + tileType + ", X: " + coordinate.x + ", Y: " + coordinate.y + ", Tile Cost: " + tileCost + ", Has Enemy: " + hasEnemy + ", Has Player: " + hasPlayer;
    }
}
