﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Xml.XPath;
using UnityEngine;
using UnityEngine.Tilemaps;

/*
 * -----------------SCRIPT INFORMATION-----------------
 * This script is designed to be the logic behind the tilemaps. There should only be one of these at a time, as there should
 * only be a single map at a time, so it is implemented as a singleton. This script should be attached to the tilemaps of EVERY
 * map that is created. It will pull the tilemap information from the map and create a CollisionMap from that information. It will
 * then be used for all pathfinding queries including returning valid paths to selected tiles.
 * ----------------------------------------------------
 * Examples of script behavior: Finding the shortest path between two points, checking which tiles can be accessed by a unit,
 * checking which tiles a unit can see.
 * Examples of how not to use the script: Moving units, AI behaviors, checking map objectives.
 */

public class MapBehavior : MonoBehaviour
{
    private CollisionMap map;
    public static MapBehavior instance = null;
    public Grid grid;
    private float gridCellSize;
    private int mapHeight, mapWidth;
    public Tilemap tilemap;
    private const int MOVE_STRAIGHT_COST = 10;
    private Vector3 coordOffset;

    [SerializeField]
    private GameObject tileHighlight;

    private GameObject highlightHolder;
    private GameObject bfgHolder;
    private GameObject[] allPlayerObjects;

    // Start is called before the first frame update
    void Start()
    { 
        //If there are no instances of MapBehavior, just set it to this
        if (instance == null)
            instance = this;
        //If there is more than one instance of MapBehavior, destroy the copy and reset it
        else if (instance != this)
        {
            Destroy(instance);
            instance = this;
        }
        //Grab the grid component
        grid = GetComponentInParent<Grid>();
        //Grab cellSize of Grid
        gridCellSize = grid.cellSize.x;

        //Grab the tilemap component
        tilemap = GetComponent<Tilemap>();

        //Compress the bounds to the map edges
        tilemap.CompressBounds();

        //Grab the bounds
        BoundsInt bounds = tilemap.cellBounds;

        mapWidth = tilemap.cellBounds.size.x;
        mapHeight = tilemap.cellBounds.size.y;

        //Get all the tiles with the correct bounds
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        //Finally, create the CollisionMap from the tilemap data
        map = new CollisionMap(allTiles, bounds.size.x, bounds.size.y);
        coordOffset = new Vector3(bounds.size.x / 2, bounds.size.y / 2, 0);
        Level.instance.levelSetup();
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraBehavior>().setup();

        highlightHolder = new GameObject();
        bfgHolder = new GameObject();
        allPlayerObjects = GameObject.FindGameObjectsWithTag("Player");
    }

    private void Update()
    {
        if (Input.GetKeyDown("q"))
        {
            foreach (CollisionTile t in map.map)
                Debug.Log(t.toString());
        }
    }

    public CollisionMap getMap()
    {
        return map;
    }

    //Call this method at the end of every move command
    public void unitMoved(Vector3 start, Vector3 destination, bool setForEnemy = false)
    {
        //setForEnemy is when we want to set manually, playerPhase at the start is true, it causes enemy's tile set as hasPlayer
        if (setForEnemy)
        {
            map.updateUnitLocation(start, destination, false);
        }
        else
        {
            map.updateUnitLocation(start, destination, GameManager.instance.playerPhase);
        }

    }

    public CollisionTile[] getPathTo(Vector3 currPos, Vector3 tile, int? movement = null, bool ignorePlayer = false)
    {
        //Get the start and destination tiles
        CollisionTile destination = getTileAtPos(tile);
        CollisionTile start = getTileAtPos(currPos);

        //Our variable to hold our created path
        //We use a List here since it is easier to add to it
        List<CollisionTile> path = new List<CollisionTile>();
        //Call our recursive pathfinding method
        path = getPath(ref start, ref destination, movement, ignorePlayer);

        //If the path we created was valid, turn it into an array and return it
        if (path != null)
            return path.ToArray();
        //Return null if no valid path
        return null;
    }

    private List<CollisionTile> getPath(ref CollisionTile currPos, ref CollisionTile destination, int? movementCost = null, bool ignorePlayer = false)
    {
        //check if destination is a valid, unoccupied tile
        if (destination == null || !destination.passable || destination.hasEnemy)
        {
            if (!ignorePlayer && destination.hasPlayer)
            { 
                return null;
            }
                
        }
        //openList is List of tiles that is actively being looked for path
        //closeList is List of tiles that has already been checked 
        List<CollisionTile> openList = new List<CollisionTile>();
        List<CollisionTile> closeList = new List<CollisionTile>();

        initializeAllCollisionTiles();

        //initialize starting tile and ending tile;
        CollisionTile startTile = currPos;
        CollisionTile endTile = destination;

        openList.Add(startTile);

        //starting tile will have gCost of 0 => Find hCost and fCost
        startTile.gCost = 0;
        startTile.hCost = calculateDistance(startTile, endTile);
        startTile.calculateFCost();

        //finding shortest path using A* algorithm
        while (openList.Count > 0)
        {
            //get tile that has lowest fCost (the closet to the end tile)
            CollisionTile currentTile = getLowestFCost(openList);
            //when the current tile is the end tile => end the method
            if (currentTile == endTile)
            {
                List<CollisionTile> path = calculatePath(endTile, movementCost);
                return path;
            }

            //tile is already checked => Remove from openList and Add to closeList
            openList.Remove(currentTile);
            closeList.Add(currentTile);

            //find all neighbors of current tile
            List<CollisionTile> allNeighborTiles = findNeighborTiles(currentTile);
            foreach (CollisionTile eachNeighbor in allNeighborTiles)
            {
                //check if the tile is already checked out
                if (!closeList.Contains(eachNeighbor))
                {
                    int newGCost = currentTile.gCost + calculateGCostDistance(currentTile, eachNeighbor);
                    //check if tile is walkable
                    if (ignorePlayer)
                    {
                        if (eachNeighbor.passable && !eachNeighbor.hasEnemy)
                        {
                            if (newGCost < eachNeighbor.gCost)
                            {
                                //this tile is potentially closer to the end tile
                                eachNeighbor.cameFromTile = currentTile;
                                eachNeighbor.gCost = newGCost;
                                eachNeighbor.hCost = calculateDistance(eachNeighbor, endTile);
                                eachNeighbor.calculateFCost();
                                if (!openList.Contains(eachNeighbor))
                                {
                                    //check in the tile
                                    openList.Add(eachNeighbor);
                                }
                            }
                        }
                        else
                        {
                            //check out the tile
                            closeList.Add(eachNeighbor);
                        }
                    }
                    else
                    {
                        if (eachNeighbor.isWalkable())
                        {
                            if (newGCost < eachNeighbor.gCost)
                            {
                                //this tile is potentially closer to the end tile
                                eachNeighbor.cameFromTile = currentTile;
                                eachNeighbor.gCost = newGCost;
                                eachNeighbor.hCost = calculateDistance(eachNeighbor, endTile);
                                eachNeighbor.calculateFCost();
                                if (!openList.Contains(eachNeighbor))
                                {
                                    //check in the tile
                                    openList.Add(eachNeighbor);
                                }
                            }
                        }
                        else
                        {
                            //check out the tile
                            closeList.Add(eachNeighbor);
                        }
                    }
                    
                }
            }
        }
        return null;

    }

    private int calculateDistance(CollisionTile startTile, CollisionTile endTile)
    {
        //guessing distance from a tile to another
        int xDistance = (int)Mathf.Abs(startTile.coordinate.x - endTile.coordinate.x);
        int yDistance = (int)Mathf.Abs(startTile.coordinate.y - endTile.coordinate.y);
        int cost = MOVE_STRAIGHT_COST * (xDistance + yDistance);
        return cost;
    }

    private int calculateGCostDistance(CollisionTile startTile, CollisionTile endTile)
    {
        //calculate gCost when tile has tile cost
        int xDistance = (int)Mathf.Abs(startTile.coordinate.x - endTile.coordinate.x);
        int yDistance = (int)Mathf.Abs(startTile.coordinate.y - endTile.coordinate.y);
        int cost = MOVE_STRAIGHT_COST * (xDistance + yDistance);
        if (endTile.tileCost != 1)
        {
            cost = MOVE_STRAIGHT_COST * (xDistance + yDistance + endTile.tileCost);
        }
        return cost;
    }

    public List<CollisionTile> findNeighborTiles(CollisionTile currentTile)
    {
        //Getting neighbors of a tile and add into a list of all neighbors
        List<CollisionTile> allNeighborTiles = new List<CollisionTile>();
        CollisionTile E = getTileAtPos(currentTile.coordinate + new Vector3(1, 0, 0)); //Tile to the East
        CollisionTile W = getTileAtPos(currentTile.coordinate + new Vector3(-1, 0, 0)); //Tile to the West
        CollisionTile N = getTileAtPos(currentTile.coordinate + new Vector3(0, 1, 0)); //Tile to the North
        CollisionTile S = getTileAtPos(currentTile.coordinate + new Vector3(0, -1, 0)); //Tile to the South
        if (E != null)
            allNeighborTiles.Add(E);
        if (W != null)
            allNeighborTiles.Add(W);
        if (N != null)
            allNeighborTiles.Add(N);
        if (S != null)
            allNeighborTiles.Add(S);
        return allNeighborTiles;
    }

    private void initializeAllCollisionTiles()
    {
        //Set all tiles with infinite value of gCost
        CollisionTile[] allTiles = map.map;
        for (int i = 0; i < allTiles.Length; i++)
        {
            CollisionTile eachTile = allTiles[i];
            eachTile.gCost = int.MaxValue;
            eachTile.calculateFCost();
            eachTile.cameFromTile = null;
        }
    }

    private List<CollisionTile> calculatePath(CollisionTile endTile, int? movementCost = null)
    {
        //Trace the path using cameFromTile, each tile connected to previous node.
        List<CollisionTile> finalPath = new List<CollisionTile>();
        finalPath.Add(endTile);
        CollisionTile currentTile = endTile;
        while (currentTile.cameFromTile != null)
        {
            finalPath.Add(currentTile.cameFromTile);
            //Reduce movement cost by tile cost
            if (movementCost != null)
            {
                movementCost -= currentTile.tileCost;
            }
            currentTile = currentTile.cameFromTile;
        }
        //when out of movement cost
        if (movementCost < 0 && movementCost != null)
        {
            return null;
        }

        finalPath.Reverse();

        return finalPath;
    }

    private CollisionTile getLowestFCost(List<CollisionTile> checkCollisionTiles)
    {
        //Basic finiding lowest number in a List
        int lowestFCost = checkCollisionTiles[0].fCost;
        CollisionTile lowestFCostTile = checkCollisionTiles[0];
        for (int i = 1; i < checkCollisionTiles.Count; i++)
        {
            int currentFCost = checkCollisionTiles[i].fCost;
            if (currentFCost < lowestFCost)
            {
                lowestFCost = currentFCost;
                lowestFCostTile = checkCollisionTiles[i];
            }
        }

        return lowestFCostTile;
    }

    public CollisionTile getTileAtPos(Vector3 coord)
    {
        //Grab the true position in reference to the grid
        Vector3 truePos = grid.WorldToCell(coord);
        //Now ask the Collision Map which tile is at our desired location
        CollisionTile tile = map.tileAt(truePos);

        return tile;
    }

    //Get cellSize of Grid
    public float getGridCellSize()
    {
        return gridCellSize;
    }

    public int getMapHeigth()
    {
        return mapHeight;
    }

    public int getMapwidth()
    {
        return mapWidth;
    }


    //Gets the enemy units that are within a certain range
    public List<Unit> getUnitsInRange(Vector3 location, int range)
    {
        List<Unit> ret = new List<Unit>();

        //Gets enemy units if it's the player
        if (GameManager.instance.playerPhase)
        {
            foreach (Unit unit in Level.instance.enemyUnits)
            {
                if (unit == null)
                    continue;
                if (hasLineTo(location, unit.transform.position, range))
                    ret.Add(unit);
            }
        }
        //Gets player units if it's the enemy
        else if (GameManager.instance.enemyPhase)
        {
            foreach (Unit unit in Level.instance.playerUnits)
            {
                if (hasLineTo(location, unit.transform.position, range))
                    ret.Add(unit);
            }
        }
        
        return ret;
    }


    //Draws a grid-based line to the target, checking tile collision on the way
    public bool hasLineTo(Vector3 start, Vector3 destination, int range)
    {
        if (Mathf.Abs(destination.y - start.y) < Mathf.Abs(destination.x - start.x))
        {
            if (start.x > destination.x)
                return checkLineLow(destination, start, range);
            else
                return checkLineLow(start, destination, range);
        }
        else
        {
            if (start.y > destination.y)
                return checkLineHigh(destination, start, range);
            else
                return checkLineHigh(start, destination, range);
        }
    }
    //Helper for hasLineTo, checks low slope lines
    private bool checkLineLow(Vector3 start, Vector3 destination, int range)
    {
        int distanceChecked = 0;

        //Get our differentials
        int xDif = (int)(destination.x - start.x);
        int yDif = (int)(destination.y - start.y);

        //Set up the proper direction to move the y value
        int yIncrement = 1;
        if (yDif < 0)
        {
            yIncrement = -1;
            yDif = -yDif;
        }

        //Determines when to increment y
        int d = (2 * yDif) - xDif;
        //Y coordinate
        int y = (int)start.y;

        for (int x = (int)start.x; x <= (int)destination.x; x++)
        {
            CollisionTile currTile = getTileAtPos(new Vector3(x + 0.5f, y + 0.5f, 0f));
            if (currTile.coordinate.Equals(destination))
                return true;
            if (!currTile.passable)
                return false;
            if (distanceChecked >= range)
                return false;
            distanceChecked++;

            //Check if we should increment y
            if (d > 0)
            {
                y = y + yIncrement;
                d = d + (2 * (yDif - xDif));
                currTile = getTileAtPos(new Vector3(x + 0.5f, y + 0.5f, 0f));
                if (currTile.coordinate.Equals(destination))
                    return true;
                if (!currTile.passable)
                    return false;
                if (distanceChecked >= range)
                    return false;
                distanceChecked++;
            }
            else
                d = d + 2 * yDif;
        }
        return false;
    }
    //Helper for hasLineTo, checks high slope lines
    private bool checkLineHigh(Vector3 start, Vector3 destination, int range)
    {
        int distanceChecked = 0;

        //Get our differentials
        int xDif = (int)(destination.x - start.x);
        int yDif = (int)(destination.y - start.y);

        //Set up the proper direction to move the x value
        int xIncrement = 1;
        if (xDif < 0)
        {
            xIncrement = -1;
            xDif = -xDif;
        }

        //Determines when to increment x
        int d = (2 * xDif) - yDif;
        //X coordinate
        int x = (int)start.x;

        for (int y = (int)start.y; y <= (int)destination.y; y++)
        {
            CollisionTile currTile = getTileAtPos(new Vector3(x + 0.5f, y + 0.5f, 0f));
            if (currTile.coordinate.Equals(destination))
                return true;
            if (!currTile.passable)
                return false;
            if (distanceChecked >= range)
                return false;
            distanceChecked++;

            //Check if we should increment x
            if (d > 0)
            {
                x = x + xIncrement;
                d = d + (2 * (xDif - yDif));
                currTile = getTileAtPos(new Vector3(x + 0.5f, y + 0.5f, 0f));
                if (currTile.coordinate.Equals(destination))
                    return true;
                if (!currTile.passable)
                    return false;
                if (distanceChecked >= range)
                    return false;
                distanceChecked++;
            }
            else
                d = d + 2 * xDif;
        }
        return false;
    }

    public List<Unit> getBFGHits(Vector3 position, Vector3 target)
    {
        List<CollisionTile> path;
        if (Mathf.Abs(target.y - position.y) < Mathf.Abs(target.x - position.x))
        {
            Vector3 line2Offset = new Vector3(0, 1, 0);
            Vector3 line3Offset = new Vector3(0, -1, 0);
            if (position.x > target.x)
            {
                path = getLineLow(position, target, false);
                foreach (CollisionTile tile in getLineLow(position + line2Offset, target + line2Offset, false))
                    path.Add(tile);
                foreach (CollisionTile tile in getLineLow(position + line3Offset, target + line3Offset, false))
                    path.Add(tile);
            }
            else
            {
                path = getLineLow(position, target, true);
                foreach (CollisionTile tile in getLineLow(position + line2Offset, target + line2Offset, true))
                    path.Add(tile);
                foreach (CollisionTile tile in getLineLow(position + line3Offset, target + line3Offset, true))
                    path.Add(tile);
            }
        }
        else
        {
            Vector3 line2Offset = new Vector3(1, 0, 0);
            Vector3 line3Offset = new Vector3(-1, 0, 0);
            if (position.y > target.y)
            {
                path = getLineHigh(position, target, false);
                foreach (CollisionTile tile in getLineHigh(position + line2Offset, target + line2Offset, false))
                    path.Add(tile);
                foreach (CollisionTile tile in getLineHigh(position + line3Offset, target + line3Offset, false))
                    path.Add(tile);
            }
            else
            {
                path = getLineHigh(position, target, true);
                foreach (CollisionTile tile in getLineHigh(position + line2Offset, target + line2Offset, true))
                    path.Add(tile);
                foreach (CollisionTile tile in getLineHigh(position + line3Offset, target + line3Offset, true))
                    path.Add(tile);
            }
        }
        List<Unit> unitsHit = new List<Unit>();
        foreach (CollisionTile tile in path)
        {
            if (tile.hasEnemy)
            {
                Unit enemy = Level.instance.getUnitAtLoc(tile.coordinate);
                if (enemy != null)
                {
                    unitsHit.Add(enemy);
                }
            }
        }

        return unitsHit;
    }

    public void drawBFGLine(Vector3 position, Vector3 target)
    {
        List<CollisionTile> path;
        if (Mathf.Abs(target.y - position.y) < Mathf.Abs(target.x - position.x))
        {
            Vector3 line2Offset = new Vector3(0, 1, 0);
            Vector3 line3Offset = new Vector3(0, -1, 0);
            if (position.x > target.x)
            {
                path = getLineLow(position, target, false);
                foreach (CollisionTile tile in getLineLow(position + line2Offset, target + line2Offset, false))
                    path.Add(tile);
                foreach (CollisionTile tile in getLineLow(position + line3Offset, target + line3Offset, false))
                    path.Add(tile);
            }
            else
            {
                path = getLineLow(position, target, true);
                foreach (CollisionTile tile in getLineLow(position + line2Offset, target + line2Offset, true))
                    path.Add(tile);
                foreach (CollisionTile tile in getLineLow(position + line3Offset, target + line3Offset, true))
                    path.Add(tile);
            }
        }
        else
        {
            Vector3 line2Offset = new Vector3(1, 0, 0);
            Vector3 line3Offset = new Vector3(-1, 0, 0);
            if (position.y > target.y)
            {
                path = getLineHigh(position, target, false);
                foreach (CollisionTile tile in getLineHigh(position + line2Offset, target + line2Offset, false))
                    path.Add(tile);
                foreach (CollisionTile tile in getLineHigh(position + line3Offset, target + line3Offset, false))
                    path.Add(tile);
            }
            else
            {
                path = getLineHigh(position, target, true);
                foreach (CollisionTile tile in getLineHigh(position + line2Offset, target + line2Offset, true))
                    path.Add(tile);
                foreach (CollisionTile tile in getLineHigh(position + line3Offset, target + line3Offset, true))
                    path.Add(tile);
            }
        }

        tileHighlight.GetComponent<SpriteRenderer>().color = new Color (207f/255f, 42f/255f, 61f/255f);
        foreach (CollisionTile tile in path)
        {
            GameObject newTile = Instantiate(tileHighlight, tile.coordinate, Quaternion.identity) as GameObject;
            newTile.transform.SetParent(bfgHolder.transform);
        }
    }

    public void eraseBFGLine()
    {
        foreach (Transform t in bfgHolder.transform)
            Destroy(t.gameObject);
    }

    //Helper for hasLineTo, checks low slope lines
    private List<CollisionTile> getLineLow(Vector3 start, Vector3 destination, bool xDir)
    {
        //Get our differentials
        int xDif = (int)(destination.x - start.x);
        int yDif = (int)(destination.y - start.y);
        if (!xDir)
        {
            xDif = (int)(start.x - destination.x);
            yDif = (int)(start.y - destination.y);
        }

        //Set up the proper direction to move the y value
        int yIncrement = 1;
        if (yDif < 0)
        {
            yIncrement = -1;
            yDif = -yDif;
        }
        if (!xDir)
            yIncrement = -yIncrement;

        //Determines when to increment y
        int d = (2 * yDif) - xDif;
        //Y coordinate
        int y = (int)start.y;

        int x = (int)start.x;
        CollisionTile currTile = getTileAtPos(new Vector3(x + 0.5f, y + 0.5f, 0f));
        List<CollisionTile> path = new List<CollisionTile>();

        while (currTile != null && currTile.passable)
        {
            if (!currTile.passable)
                return path;

            path.Add(currTile);

            //Check if we should increment y
            if (d > 0)
            {
                y = y + yIncrement;
                d = d + (2 * (yDif - xDif));
                currTile = getTileAtPos(new Vector3(x + 0.5f, y + 0.5f, 0f));
                if (currTile == null || !currTile.passable)
                    return path;

                //path.Add(currTile);
            }
            else
                d = d + 2 * yDif;

            if (xDir)
                x++;
            else
                x--;
            currTile = getTileAtPos(new Vector3(x + 0.5f, y + 0.5f, 0f));
        }
        return path;
    }
    //Helper for hasLineTo, checks high slope lines
    private List<CollisionTile> getLineHigh(Vector3 start, Vector3 destination, bool yDir)
    {
        //Get our differentials
        int xDif = (int)(destination.x - start.x);
        int yDif = (int)(destination.y - start.y);
        if (!yDir)
        {
            xDif = (int)(start.x - destination.x);
            yDif = (int)(start.y - destination.y);
        }

        //Set up the proper direction to move the x value
        int xIncrement = 1;
        if (xDif < 0)
        {
            xIncrement = -1;
            xDif = -xDif;
        }
        if (!yDir)
            xIncrement = -xIncrement;

        //Determines when to increment x
        int d = (2 * xDif) - yDif;
        //X coordinate
        int x = (int)start.x;

        int y = (int)start.y;
        CollisionTile currTile = getTileAtPos(new Vector3(x + 0.5f, y + 0.5f, 0f));
        List<CollisionTile> path = new List<CollisionTile>();

        while (currTile != null && currTile.passable)
        {
            if (!currTile.passable)
                return path;

            path.Add(currTile);

            //Check if we should increment x
            if (d > 0)
            {
                x = x + xIncrement;
                d = d + (2 * (xDif - yDif));
                currTile = getTileAtPos(new Vector3(x + 0.5f, y + 0.5f, 0f));
                if (currTile == null || !currTile.passable)
                    return path;

                //path.Add(currTile);
            }
            else
                d = d + 2 * xDif;

            if (yDir)
                y++;
            else
                y--;

            currTile = getTileAtPos(new Vector3(x + 0.5f, y + 0.5f, 0f));
        }
        return path;
    }

    public void highlightTilesInRange(Vector3 currPos, int movement)
    {
        //Get the start tile
        CollisionTile start = getTileAtPos(currPos);

        //Our variable to hold our created path
        //We use a List here since it is easier to add to it
        List<CollisionTile> path = new List<CollisionTile>();

        //Call our recursive method
        path = getTilesInRange(start, movement, path);

        tileHighlight.GetComponent<SpriteRenderer>().color = new Color(45f/255f, 150f/255f, 1f);

        //Spawn in the highlight game objects
        foreach (CollisionTile tile in path)
        {
            GameObject highlight = Instantiate(tileHighlight, tile.coordinate, Quaternion.identity) as GameObject;
            highlight.transform.SetParent(highlightHolder.transform);
        }
    }

    //Deletes all highlight tiles held in highlightHolder
    public void deleteHighlightTiles()
    {
        foreach (Transform tile in highlightHolder.transform)
        {
            Destroy(tile.gameObject);
        }
    }

    private List<CollisionTile> getTilesInRange(CollisionTile currPos, int movementLeft, List<CollisionTile> currentPath)
    {
        //If we didn't have enough movement to get here, stop looking
        if (movementLeft < 0 || !currPos.isWalkable())
            return currentPath;

        //Add the current tile to the path
        if (!currentPath.Contains(currPos))
            currentPath.Add(currPos);
        

        //Set up our possible paths
        List<CollisionTile> path1 = null;
        List<CollisionTile> path2 = null;
        List<CollisionTile> path3 = null;
        List<CollisionTile> path4 = null;

        //If we're here, we still have movement and we've not reached our destination. We must now check all adjacent tiles
        CollisionTile E = getTileAtPos(currPos.coordinate + new Vector3(1, 0, 0)); //Tile to the East
        CollisionTile W = getTileAtPos(currPos.coordinate + new Vector3(-1, 0, 0)); //Tile to the West
        CollisionTile N = getTileAtPos(currPos.coordinate + new Vector3(0, 1, 0)); //Tile to the North
        CollisionTile S = getTileAtPos(currPos.coordinate + new Vector3(0, -1, 0)); //Tile to the South

        //If the tile is non-null, traverse along the path
        if (E != null)
        {
            //Get how much movement is remaining
            int remainder = movementLeft - E.tileCost;
            //We need to create a new list identical to currentPath so that other paths don't add to the same list
            //Instead of setting a new pointer to the list, copy the contents
            List<CollisionTile> temp = new List<CollisionTile>();
            foreach (CollisionTile tile in currentPath)
                temp.Add(tile);
            //Have path1 store the results of the first created path
            path1 = getTilesInRange(E, remainder, currentPath);
        }
        if (W != null)
        {
            int remainder = movementLeft - W.tileCost;
            List<CollisionTile> temp = new List<CollisionTile>();
            foreach (CollisionTile tile in currentPath)
                temp.Add(tile);
            path2 = getTilesInRange(W, remainder, currentPath);
        }
        if (N != null)
        {
            int remainder = movementLeft - N.tileCost;
            List<CollisionTile> temp = new List<CollisionTile>();
            foreach (CollisionTile tile in currentPath)
                temp.Add(tile);
            path3 = getTilesInRange(N, remainder, currentPath);
        }
        if (S != null)
        {
            int remainder = movementLeft - S.tileCost;
            List<CollisionTile> temp = new List<CollisionTile>();
            foreach (CollisionTile tile in currentPath)
                temp.Add(tile);
            path4 = getTilesInRange(S, remainder, currentPath);
        }

        return currentPath;
    }

    //Get player objects based on position
    public GameObject getClosestPlayerObject(Vector3 enemyPosition)
    {
        GameObject closestPlayer = allPlayerObjects[0];
        float lowestDistance = Vector3.Distance(closestPlayer.transform.position, enemyPosition);
        foreach(GameObject eachPlayer in allPlayerObjects)
        {
            float currentDistance = Vector3.Distance(eachPlayer.transform.position, enemyPosition);
            if (currentDistance < lowestDistance)
            {
                lowestDistance = currentDistance;
                closestPlayer = eachPlayer;
            }
        }
        return closestPlayer;
    }

}
