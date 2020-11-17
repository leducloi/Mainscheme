using System.Collections;
using System.Collections.Generic;
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
    public Tilemap obstacles = null;
    private float gridCellSize;
    private int mapHeight, mapWidth;
    public Tilemap tilemap;
    private const int MOVE_STRAIGHT_COST = 10;
    private Vector3 coordOffset;

    [SerializeField]
    private GameObject tileHighlight = null;

    private GameObject highlightHolder;
    private GameObject bfgHolder;
    private GameObject objectiveHolder;
    private GameObject[] allPlayerObjects;

    private Color red = new Color(207f / 255f, 42f / 255f, 61f / 255f);
    private Color blue = new Color(45f / 255f, 150f / 255f, 1f);
    private Color green = new Color(45f / 255f, 1f, 150f / 255f);
    private Color orange = new Color(1f, 142f / 255f, 42f / 255f);
    private Color yellow = Color.yellow;

    private Color currColor;


    // Start is called before the first frame update
    void Start()
    {
        //If there are no instances of MapBehavior, just set it to this
        if (instance == null)
            instance = this;
        //If there is more than one instance of MapBehavior, destroy the copy and reset it
        else if (instance != this)
        {
            Destroy(instance.objectiveHolder);
            Destroy(instance.highlightHolder);
            Destroy(instance.bfgHolder);
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

        TileBase[] obTiles = null;
        if (obstacles != null)
            obTiles = obstacles.GetTilesBlock(bounds);

        //Finally, create the CollisionMap from the tilemap data
        map = new CollisionMap(allTiles, obTiles, bounds.size.x, bounds.size.y);
        coordOffset = new Vector3(bounds.size.x / 2, bounds.size.y / 2, 0);
        StartCoroutine(Level.instance.planning());
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraBehavior>().setup();

        highlightHolder = new GameObject("Highlight Holder");
        bfgHolder = new GameObject("BFG Holder");
        objectiveHolder = new GameObject("Objective Holder");
        highlightHolder.transform.SetParent(transform);
        bfgHolder.transform.SetParent(transform);
        objectiveHolder.transform.SetParent(transform);

        

        currColor = blue;
        
    }

    private void Update()
    {
        
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
        

        if (ignorePlayer)
        {
            //Keep finding the nearest tile back if the current nearest is occupied
            while (destination.hasPlayer || destination.hasEnemy)
            {
                List<CollisionTile> neighbors = findNeighborTiles(destination);
                int lowestHCost = int.MaxValue;
                int index = 0;
                CollisionTile backup = null;
                foreach (CollisionTile neighbor in neighbors)
                {
                    if (!neighbor.isWalkable())
                        continue;

                    if (neighbor.hasEnemy)
                    {
                        backup = neighbor;
                        continue;
                    }

                    int hCost = calculateDistance(start, neighbor);
                    if (hCost < lowestHCost)
                    {
                        lowestHCost = hCost;
                        index = neighbors.IndexOf(neighbor);
                    }
                }
                if (lowestHCost == int.MaxValue && backup != null)
                    destination = backup;
                else
                    destination = neighbors[index];
            }
            
        }
        //Call our recursive pathfinding method
        path = getPath(ref start, ref destination, movement);

        //If the path we created was valid, turn it into an array and return it
        if (path != null)
            return path.ToArray();
        //Return null if no valid path
        return null;
    }

    private List<CollisionTile> getPath(ref CollisionTile currPos, ref CollisionTile destination, int? movementCost = null, bool ignorePlayer = false)
    {
        //check if destination is a valid, unoccupied tile
        if (destination == null || !destination.passable || destination.hasEnemy || destination.hasPlayer)
        {
            return null;
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
                                if (eachNeighbor.coordinate.x > currentTile.coordinate.x && !currentTile.passableEW)
                                    continue;
                                if (eachNeighbor.coordinate.x < currentTile.coordinate.x && !eachNeighbor.passableEW)
                                    continue;
                                if (eachNeighbor.coordinate.y > currentTile.coordinate.y && !currentTile.passableNS)
                                    continue;
                                if (eachNeighbor.coordinate.y < currentTile.coordinate.y && !eachNeighbor.passableNS)
                                    continue;

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

    public CollisionTile stepOutInto(Vector3 currPosition, Unit target, int range, int minRange)
    {
        //If the unit has a line to the target where they are at, just return their current position
        if (hasLineTo(currPosition, target.transform.position, range, minRange))
            return getTileAtPos(currPosition);

        foreach (CollisionTile tile in findNeighborTiles(getTileAtPos(currPosition)))
        {
            if (hasLineTo(tile.coordinate, target.transform.position, range - 1, minRange - 1))
                return tile;
        }
        //If there's no line to the target, return null
        return null;
    }

    //Gets the enemy units that are within a certain range
    public List<Unit> getUnitsInRange(Vector3 location, int range, int minRange)
    {
        List<Unit> ret = new List<Unit>();

        CollisionTile currTile = getTileAtPos(location);

        Unit currUnit = Level.instance.getUnitAtLoc(location);
        //If the unit is taking cover, calculate step-out
        if (currUnit != null && currUnit.takingCover)
        {
            currUnit.takingCover = false;
            List<CollisionTile> stepOutTiles = new List<CollisionTile>();
            //Find passable tiles to step out into
            foreach (CollisionTile tile in findNeighborTiles(getTileAtPos(location)))
            {
                if (!tile.passable)
                    continue;
                if (!currTile.passableEW && location.x < tile.coordinate.x)
                    continue;
                if (!currTile.passableNS && location.y < tile.coordinate.y)
                    continue;
                if (!tile.passableNS && location.y > tile.coordinate.y)
                    continue;
                if (!tile.passableEW && location.x > tile.coordinate.x)
                    continue;

                stepOutTiles.Add(tile);
            }
            //Calculate enemies in the range of each of those tiles
            foreach (CollisionTile tile in stepOutTiles)
            {
                //Decrease the range, since their actual position is not changing they are just leaning out
                foreach (Unit u in getUnitsInRange(tile.coordinate, range - 1, minRange - 1))
                {
                    if (!ret.Contains(u))
                        ret.Add(u);
                }
            }
            currUnit.takingCover = true;
        }

        //Gets enemy units if it's the player
        if (GameManager.instance.playerPhase)
        {
            foreach (Unit unit in Level.instance.enemyUnits)
            {
                if (unit == null || unit.isCloaked)
                    continue;
                if (!ret.Contains(unit) && hasLineTo(location, unit.transform.position, range, minRange))
                    ret.Add(unit);
            }
        }
        //Gets player units if it's the enemy
        else if (GameManager.instance.enemyPhase)
        {
            foreach (Unit unit in Level.instance.selectedUnits)
            {
                if (unit == null || unit.isCloaked)
                    continue;
                if (!ret.Contains(unit) && hasLineTo(location, unit.transform.position, range, minRange))
                    ret.Add(unit);
            }
        }
        
        return ret;
    }

    public List<Unit> getAlliesInRange(Vector3 location, int range)
    {
        List<Unit> ret = new List<Unit>();

        //Gets player units if it's the player
        if (GameManager.instance.playerPhase)
        {
            foreach (Unit unit in Level.instance.playerUnits)
            {
                if (unit == null)
                    continue;
                if (hasLineTo(location, unit.transform.position, range, 0))
                    ret.Add(unit);
            }
        }
        //Gets enemy units if it's the enemy
        else if (GameManager.instance.enemyPhase)
        {
            foreach (Unit unit in Level.instance.enemyUnits)
            {
                if (hasLineTo(location, unit.transform.position, range, 0))
                    ret.Add(unit);
            }
        }

        return ret;
    }

    public void drawLineTo(Vector3 position, Vector3 target)
    {
        List<CollisionTile> path;
        if (Mathf.Abs(target.y - position.y) < Mathf.Abs(target.x - position.x))
        {
            if (position.x > target.x)
            {
                path = getLineLow(position, target, false, true);
            }
            else
            {
                path = getLineLow(position, target, true, true);
            }
        }
        else
        {
            if (position.y > target.y)
            {
                path = getLineHigh(position, target, false, true);
            }
            else
            {
                path = getLineHigh(position, target, true, true);
            }
        }

        setColor('r');
        tileHighlight.GetComponent<SpriteRenderer>().color = currColor;
        foreach (CollisionTile tile in path)
        {
            GameObject newTile = Instantiate(tileHighlight, tile.coordinate, Quaternion.identity) as GameObject;
            newTile.transform.SetParent(bfgHolder.transform);
        }
    }

    //Draws a grid-based line to the target, checking tile collision on the way
    public bool hasLineTo(Vector3 start, Vector3 destination, int range, int minRange)
    {
        if (Mathf.Abs(destination.y - start.y) < Mathf.Abs(destination.x - start.x))
        {
            if (start.x > destination.x)
                return checkLineLow(destination, start, range, minRange);
            else
                return checkLineLow(start, destination, range, minRange);
        }
        else
        {
            if (start.y > destination.y)
                return checkLineHigh(destination, start, range, minRange);
            else
                return checkLineHigh(start, destination, range, minRange);
        }
    }

    //Helper for hasLineTo, checks low slope lines
    private bool checkLineLow(Vector3 start, Vector3 destination, int range, int minRange)
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

        CollisionTile lastTile = null;
        for (int x = (int)start.x; x <= (int)destination.x; x++)
        {
            CollisionTile currTile = getTileAtPos(new Vector3(x + 0.5f, y + 0.5f, 0f));
            if (currTile == null)
                return false;
            if (!currTile.passable || (lastTile != null && !lastTile.passableEW && lastTile.highCover))
                return false;
            if (currTile.coordinate.Equals(destination) && distanceChecked >= minRange)
                return true;
            if (distanceChecked >= range)
                return false;
            distanceChecked++;

            lastTile = currTile;

            //Check if we should increment y
            if (d > 0)
            {
                y = y + yIncrement;
                d = d + (2 * (yDif - xDif));
                currTile = getTileAtPos(new Vector3(x + 0.5f, y + 0.5f, 0f));
                if (currTile == null)
                    return false;
                if (yIncrement > 0)
                {
                    if (!currTile.passable || ((!lastTile.passableNS /*|| !currTile.passableEW*/) && lastTile.highCover))
                        return false;
                }
                else
                {
                    if (!currTile.passable || ((!currTile.passableNS /*|| !currTile.passableEW*/) && currTile.highCover))
                        return false;
                }
                
                if (currTile.coordinate.Equals(destination) && distanceChecked >= minRange)
                    return true;
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
    private bool checkLineHigh(Vector3 start, Vector3 destination, int range, int minRange)
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

        CollisionTile lastTile = null;

        for (int y = (int)start.y; y <= (int)destination.y; y++)
        {
            CollisionTile currTile = getTileAtPos(new Vector3(x + 0.5f, y + 0.5f, 0f));
            if (currTile == null)
                return false;
            if (!currTile.passable || (lastTile != null && !lastTile.passableNS && lastTile.highCover))
                return false;
            if (currTile.coordinate.Equals(destination) && distanceChecked >= minRange)
                return true;
            if (distanceChecked >= range)
                return false;
            distanceChecked++;

            lastTile = currTile;

            //Check if we should increment x
            if (d > 0)
            {
                x = x + xIncrement;
                d = d + (2 * (xDif - yDif));
                currTile = getTileAtPos(new Vector3(x + 0.5f, y + 0.5f, 0f));
                if (currTile == null)
                    return false;

                if (xIncrement > 0)
                {
                    if (!currTile.passable || ((/*!currTile.passableNS ||*/ !lastTile.passableEW) && lastTile.highCover))
                        return false;
                }
                else
                {
                    if (!currTile.passable || (!currTile.passableEW && currTile.highCover))
                        return false;
                }

                if (xIncrement > 0 && !getTileAtPos(new Vector3(currTile.coordinate.x - 1, currTile.coordinate.y, 0)).passableEW)
                    return false;
                if (currTile.coordinate.Equals(destination) && distanceChecked >= minRange)
                    return true;
                if (distanceChecked >= range)
                    return false;
                distanceChecked++;

                lastTile = currTile;
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
                    if (!path.Contains(tile))
                        path.Add(tile);
                foreach (CollisionTile tile in getLineHigh(position + line3Offset, target + line3Offset, false))
                    if (!path.Contains(tile))
                        path.Add(tile);
            }
            else
            {
                path = getLineHigh(position, target, true);
                foreach (CollisionTile tile in getLineHigh(position + line2Offset, target + line2Offset, true))
                    if (!path.Contains(tile))
                        path.Add(tile);
                foreach (CollisionTile tile in getLineHigh(position + line3Offset, target + line3Offset, true))
                    if (!path.Contains(tile))
                        path.Add(tile);
            }
        }
        List<Unit> unitsHit = new List<Unit>();
        foreach (CollisionTile tile in path)
        {
            if (tile.hasEnemy)
            {
                Unit enemy = Level.instance.getUnitAtLoc(tile.coordinate);
                if (enemy != null && !unitsHit.Contains(enemy))
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

        setColor('r');
        tileHighlight.GetComponent<SpriteRenderer>().color = currColor;
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

    //Helper for BFGLine, checks low slope lines
    private List<CollisionTile> getLineLow(Vector3 start, Vector3 destination, bool xDir, bool hasCollision = false)
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

        CollisionTile lastTile = null;
        while (currTile != null && currTile.passable)
        {
            if (xDir && lastTile != null && !lastTile.passableEW)
                return path;
            else if (!xDir && !currTile.passableEW)
                return path;
            if (!currTile.passable || (hasCollision && !currTile.isWalkable()) )
                return path;

            path.Add(currTile);

            lastTile = currTile;

            //Check if we should increment y
            if (d > 0)
            {
                y = y + yIncrement;
                d = d + (2 * (yDif - xDif));
                currTile = getTileAtPos(new Vector3(x + 0.5f, y + 0.5f, 0f));
                if (currTile == null || !currTile.passable || (hasCollision && !currTile.isWalkable()))
                    return path;

                if (hasCollision && currTile.isWalkable())
                    path.Add(currTile);

                lastTile = currTile;
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

    //Helper for BFGLine, checks high slope lines
    private List<CollisionTile> getLineHigh(Vector3 start, Vector3 destination, bool yDir, bool hasCollision = false)
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

        CollisionTile lastTile = null;
        while (currTile != null && currTile.passable)
        {
            if (yDir && lastTile != null && !lastTile.passableNS)
                return path;
            else if (!yDir && !currTile.passableNS)
                return path;
            if (!currTile.passable || (hasCollision && !currTile.isWalkable()))
                return path;

            path.Add(currTile);

            lastTile = currTile;

            //Check if we should increment x
            if (d > 0)
            {
                x = x + xIncrement;
                d = d + (2 * (xDif - yDif));
                currTile = getTileAtPos(new Vector3(x + 0.5f, y + 0.5f, 0f));
                if (currTile == null || !currTile.passable || (hasCollision && !currTile.isWalkable()))
                    return path;
                
                if (hasCollision && currTile.isWalkable())
                    path.Add(currTile);

                lastTile = currTile;
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

    public void setColor(char colorChar)
    {
        switch (colorChar)
        {
            case 'r':
            case 'R':
                currColor = red;
                break;

            case 'b':
            case 'B':
                currColor = blue;
                break;

            case 'g':
            case 'G':
                currColor = green;
                break;

            case 'o':
            case 'O':
                currColor = orange;
                break;

            case 'y':
            case 'Y':
                currColor = yellow;
                break;

            default:
                currColor = blue;
                break;
        }
    }

    public void highlightTilesInRange(Vector3 currPos, int movement, int minRange, int maxRange, bool forEnemy = false)
    {
        //Get the start tile
        CollisionTile start = getTileAtPos(currPos);

        //Our variable to hold our created path
        //We use a List here since it is easier to add to it
        List<CollisionTile> path = new List<CollisionTile>();

        //Call our recursive method
        path = getTilesInRange(start, movement, path, forEnemy);

        List<CollisionTile> range = new List<CollisionTile>();
        foreach (CollisionTile tile in path)
        {
            int count = 0;
            for (int x = 0; x <= maxRange; x++)
            {
                for (int y = maxRange - x; y >= Mathf.Clamp(minRange - x, 0, int.MaxValue); y--)
                {
                    CollisionTile q1 = getTileAtPos(new Vector3((int)tile.coordinate.x + x, (int)tile.coordinate.y + y, 0));
                    CollisionTile q2 = getTileAtPos(new Vector3((int)tile.coordinate.x + x, (int)tile.coordinate.y - y, 0));
                    CollisionTile q3 = getTileAtPos(new Vector3((int)tile.coordinate.x - x, (int)tile.coordinate.y - y, 0));
                    CollisionTile q4 = getTileAtPos(new Vector3((int)tile.coordinate.x - x, (int)tile.coordinate.y + y, 0));
                    if (q1 != null && !range.Contains(q1) && !path.Contains(q1) && hasLineTo(tile.coordinate, q1.coordinate, maxRange, minRange))
                        range.Add(q1);
                    if (q2 != null && !range.Contains(q2) && !path.Contains(q2) && hasLineTo(tile.coordinate, q2.coordinate, maxRange, minRange))
                        range.Add(q2);
                    if (q3 != null && !range.Contains(q3) && !path.Contains(q3) && hasLineTo(tile.coordinate, q3.coordinate, maxRange, minRange))
                        range.Add(q3);
                    if (q4 != null && !range.Contains(q4) && !path.Contains(q4) && hasLineTo(tile.coordinate, q4.coordinate, maxRange, minRange))
                        range.Add(q4);
                }
                count++;
            }
        }

        tileHighlight.GetComponent<SpriteRenderer>().color = currColor;

        //Spawn in the highlight game objects
        foreach (CollisionTile tile in path)
        {
            GameObject highlight = Instantiate(tileHighlight, tile.coordinate, Quaternion.identity) as GameObject;
            highlight.transform.SetParent(highlightHolder.transform);
        }

        setColor('r');
        tileHighlight.GetComponent<SpriteRenderer>().color = currColor;
        foreach (CollisionTile tile in range)
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

    private List<CollisionTile> getTilesInRange(CollisionTile currPos, int movementLeft, List<CollisionTile> currentPath, bool forEnemy)
    {
        //If we didn't have enough movement to get here, stop looking
        if (movementLeft < 0 || !currPos.passable)
            return currentPath;

        if (forEnemy && currPos.hasPlayer)
            return currentPath;
        else if (!forEnemy && currPos.hasEnemy)
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
        if (E != null && currPos.passableEW)
        {
            //Get how much movement is remaining
            int remainder = movementLeft - E.tileCost;
            //Have path1 store the results of the first created path
            path1 = getTilesInRange(E, remainder, currentPath, forEnemy);
        }
        if (W != null && W.passableEW)
        {
            int remainder = movementLeft - W.tileCost;
            path2 = getTilesInRange(W, remainder, currentPath, forEnemy);
        }
        if (N != null && currPos.passableNS)
        {
            int remainder = movementLeft - N.tileCost;
            path3 = getTilesInRange(N, remainder, currentPath, forEnemy);
        }
        if (S != null && S.passableNS)
        {
            int remainder = movementLeft - S.tileCost;
            path4 = getTilesInRange(S, remainder, currentPath, forEnemy);
        }
        return currentPath;
    }

    public void highlightTilesWithin(Vector3 currPos, int range)
    {
        //Get the start tile
        CollisionTile start = getTileAtPos(currPos);

        //Our variable to hold our created path
        //We use a List here since it is easier to add to it
        List<CollisionTile> path = new List<CollisionTile>();

        //Call our recursive method
        path = getTilesWithin(start, range, path);

        tileHighlight.GetComponent<SpriteRenderer>().color = currColor;

        //Spawn in the highlight game objects
        foreach (CollisionTile tile in path)
        {
            GameObject highlight = Instantiate(tileHighlight, tile.coordinate, Quaternion.identity) as GameObject;
            highlight.transform.SetParent(highlightHolder.transform);
        }
    }

    public List<CollisionTile> getTilesWithin(CollisionTile currPos, int rangeLeft, List<CollisionTile> currentPath)
    {
        List<CollisionTile> pathEast = tilesWithinEast(currPos.coordinate, rangeLeft, currentPath);

        Vector3 W = currPos.coordinate + new Vector3(-1, 0, 0); //Tile to the West

        List<CollisionTile> pathWest = tilesWithinWest(W, rangeLeft - 1, currentPath);

        return currentPath;
    }

    private List<CollisionTile> tilesWithinEast(Vector3 position, int rangeLeft, List<CollisionTile> currentPath)
    {
        //If we didn't have enough movement to get here, stop looking
        if (rangeLeft < 0)
            return currentPath;

        CollisionTile currPos = getTileAtPos(position);

        //Add the current tile to the path
        if (currPos != null && !currentPath.Contains(currPos))
            currentPath.Add(currPos);
        

        //Set up our possible paths
        List<CollisionTile> path1 = null;
        List<CollisionTile> path2 = null;
        List<CollisionTile> path3 = null;

        //If we're here, we still have movement and we've not reached our destination. We must now check all adjacent tiles

        Vector3 E = position + new Vector3(1, 0, 0);
        Vector3 N = position + new Vector3(0, 1, 0);
        Vector3 S = position + new Vector3(0, -1, 0);

        int remainder = rangeLeft - 1;

        path1 = tilesWithinEast(E, remainder, currentPath);
        path2 = tilesWithinEast(N, remainder, currentPath);
        path3 = tilesWithinEast(S, remainder, currentPath);

        return currentPath;
    }

    private List<CollisionTile> tilesWithinWest(Vector3 position, int rangeLeft, List<CollisionTile> currentPath)
    {
        //If we didn't have enough movement to get here, stop looking
        if (rangeLeft < 0)
            return currentPath;

        CollisionTile currPos = getTileAtPos(position);

        //Add the current tile to the path
        if (currPos != null && !currentPath.Contains(currPos))
            currentPath.Add(currPos);
        

        //Set up our possible paths
        List<CollisionTile> path1 = null;
        List<CollisionTile> path2 = null;
        List<CollisionTile> path3 = null;

        //If we're here, we still have movement and we've not reached our destination. We must now check all adjacent tiles

        Vector3 W = position + new Vector3(-1, 0, 0);
        Vector3 N = position + new Vector3(0, 1, 0);
        Vector3 S = position + new Vector3(0, -1, 0);

        int remainder = rangeLeft - 1;

        path1 = tilesWithinWest(W, remainder, currentPath);
        path2 = tilesWithinWest(N, remainder, currentPath);
        path3 = tilesWithinWest(S, remainder, currentPath);

        return currentPath;
    }

    //Get player objects based on position
    public GameObject getClosestPlayerObject(Vector3 enemyPosition)
    {
        GameObject closestPlayer = allPlayerObjects[0];
        float lowestDistance = Vector3.Distance(closestPlayer.transform.position, enemyPosition);
        foreach (GameObject eachPlayer in allPlayerObjects)
        {
            if (eachPlayer == null || eachPlayer.GetComponent<PlayerUnit>().isCloaked)
                continue;
            float currentDistance = Vector3.Distance(eachPlayer.transform.position, enemyPosition);
            if (currentDistance < lowestDistance)
            {
                lowestDistance = currentDistance;
                closestPlayer = eachPlayer;
            }
        }


        return closestPlayer;
    }

    public void hightlightCustomTiles(List<CollisionTile> tilesToHighlight, char color)
    {
        //Set color
        setColor(color);
        tileHighlight.GetComponent<SpriteRenderer>().color = currColor;
        
        //Spawn highlight tiles
        foreach (CollisionTile tile in tilesToHighlight)
        {
            GameObject highlight = Instantiate(tileHighlight, tile.coordinate, Quaternion.identity) as GameObject;
            highlight.transform.SetParent(highlightHolder.transform);
        }
    }

    public List<CollisionTile> getJumpableTiles(Vector3 position, int range)
    {
        CollisionTile currPos = getTileAtPos(position);

        //Get every tile within range to narrow later
        List<CollisionTile> allTilesInRange = new List<CollisionTile>();
        allTilesInRange = getTilesWithin(currPos, range, allTilesInRange);

        List<CollisionTile> tilesToHighlight = new List<CollisionTile>();
        foreach (CollisionTile tile in allTilesInRange)
        {
            //If the tile is walkable, we can't grapple to it
            if (tile.passable && tile.passableEW && tile.passableNS)
                continue;
            if (!tile.passable)
            {
                //Get all the adjacent tiles to the grapple tile
                List<CollisionTile> neighbors = findNeighborTiles(tile);
                foreach (CollisionTile neighbor in neighbors)
                {
                    //Add to the highlight list the walkable tiles
                    if (neighbor.isWalkable())
                        tilesToHighlight.Add(neighbor);
                }
                continue;
            }

            if (!tile.passableEW)
            {
                if (!tilesToHighlight.Contains(tile) && tile.isWalkable() && tile.passableNS)
                    tilesToHighlight.Add(tile);
                CollisionTile east = getTileAtPos(new Vector3(tile.coordinate.x + 1, tile.coordinate.y, 0));
                if (east != null && east.isWalkable() && east.passableNS)
                    if (!tilesToHighlight.Contains(east))
                        tilesToHighlight.Add(east);
            }
            if (!tile.passableNS)
            {
                CollisionTile south = getTileAtPos(new Vector3(tile.coordinate.x, tile.coordinate.y - 1, 0));
                CollisionTile north = getTileAtPos(new Vector3(tile.coordinate.x, tile.coordinate.y + 1, 0));
                if (south != null && !south.passableNS)
                {
                    if (north != null && !tilesToHighlight.Contains(north) && north.isWalkable())
                    {
                        tilesToHighlight.Add(north);
                    }
                }
                else if (north != null && !north.passableNS)
                {
                    if (!tilesToHighlight.Contains(tile) && tile.isWalkable())
                    {
                        tilesToHighlight.Add(tile);
                    }
                }
            }
            if (!tile.passableNS || !tile.passableEW)
                continue;


            //Get all the adjacent tiles to the grapple tile
            List<CollisionTile> adjTiles = findNeighborTiles(tile);
            foreach (CollisionTile neighbor in adjTiles)
            {
                //Add to the highlight list the walkable tiles
                if (neighbor.isWalkable())
                    tilesToHighlight.Add(neighbor);
            }
        }

        return tilesToHighlight;
    }

    public void deleteObjectiveTiles(GameObject toDelete)
    {
        foreach (Transform t in toDelete.transform)
            Destroy(t.gameObject);
        Destroy(toDelete);
    }

    public void deleteAllObjectiveTiles()
    {
        foreach (Transform t in objectiveHolder.transform)
            deleteObjectiveTiles(t.gameObject);
    }

    public GameObject highlightObjectiveTiles(List<CollisionTile> tilesToHighlight)
    {
        tileHighlight.GetComponent<SpriteRenderer>().color = currColor;

        GameObject objective = new GameObject();
        
        objective.transform.SetParent(objectiveHolder.transform);

        //Spawn highlight tiles
        foreach (CollisionTile tile in tilesToHighlight)
        {
            GameObject highlight = Instantiate(tileHighlight, tile.coordinate, Quaternion.identity) as GameObject;
            highlight.transform.SetParent(objective.transform);
        }

        return objective;
    }

    public void setPlayerArray()
    {
        List<GameObject> pObj = new List<GameObject>();
        foreach (Unit u in Level.instance.selectedUnits)
            pObj.Add(u.gameObject);
        allPlayerObjects = pObj.ToArray();
    }
}
