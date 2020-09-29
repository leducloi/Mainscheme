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
    
    private Vector3 coordOffset;

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

        //Grab the tilemap component
        Tilemap tilemap = GetComponent<Tilemap>();

        //Compress the bounds to the map edges
        tilemap.CompressBounds();

        //Grab the bounds
        BoundsInt bounds = tilemap.cellBounds;

        //Get all the tiles with the correct bounds
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        //Finally, create the CollisionMap from the tilemap data
        map = new CollisionMap(allTiles, bounds.size.x, bounds.size.y);
        coordOffset = new Vector3(bounds.size.x / 2, bounds.size.y / 2, 0);
    }

    public CollisionTile[] getPathTo(Vector3 currPos, Vector3 tile, int movement)
    {
        //Get the start and destination tiles
        CollisionTile destination = getTileAtPos(tile, true);
        CollisionTile start = getTileAtPos(currPos, true);

        //Our variable to hold our created path
        //We use a List here since it is easier to add to it
        List<CollisionTile> path = new List<CollisionTile>();
        
        //Call our recursive pathfinding method
        path = getPath(ref start, ref destination, movement, path);

        //If the path we created was valid, turn it into an array and return it
        if (path != null)
            return path.ToArray();
        //Return null if no valid path
        return null;
    }

    //TODO - Mark tiles with enemies on them as impassible
    private List<CollisionTile> getPath(ref CollisionTile currPos, ref CollisionTile destination, int movementLeft, List<CollisionTile> currentPath)
    {
        //If our destination is invalid, just return out
        if (destination == null)
            return null;

        //Add the current tile to the path
        currentPath.Add(currPos);
        //If we didn't have enough movement to get here, this is not a valid path
        if (movementLeft < 0)
            return null;
        //If we've reached our destination, return our path
        if (currPos.Equals(destination))
            return currentPath;
        //If we've run out of movement instead, then return null for a failed path
        if (movementLeft == 0)
            return null;

        //Set up our possible paths
        List<CollisionTile> path1 = null;
        List<CollisionTile> path2 = null;
        List<CollisionTile> path3 = null;
        List<CollisionTile> path4 = null;

        //If we're here, we still have movement and we've not reached our destination. We must now check all adjacent tiles
        CollisionTile E = getTileAtPos(currPos.coordinate + new Vector3(1, 0, 0), false); //Tile to the East
        CollisionTile W = getTileAtPos(currPos.coordinate + new Vector3(-1, 0, 0), false); //Tile to the West
        CollisionTile N = getTileAtPos(currPos.coordinate + new Vector3(0, 1, 0), false); //Tile to the North
        CollisionTile S = getTileAtPos(currPos.coordinate + new Vector3(0, -1, 0), false); //Tile to the South

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
            path1 = getPath(ref E, ref destination, remainder, temp);
        }
        if (W != null)
        {
            int remainder = movementLeft - W.tileCost;
            List<CollisionTile> temp = new List<CollisionTile>();
            foreach (CollisionTile tile in currentPath)
                temp.Add(tile);
            path2 = getPath(ref W, ref destination, remainder, temp);
        }
        if (N != null)
        {
            int remainder = movementLeft - N.tileCost;
            List<CollisionTile> temp = new List<CollisionTile>();
            foreach (CollisionTile tile in currentPath)
                temp.Add(tile);
            path3 = getPath(ref N, ref destination, remainder, temp);
        }
        if (S != null)
        {
            int remainder = movementLeft - S.tileCost;
            List<CollisionTile> temp = new List<CollisionTile>();
            foreach (CollisionTile tile in currentPath)
                temp.Add(tile);
            path4 = getPath(ref S, ref destination, remainder, temp);
        }

        //Now we need to find which is the shortest path
        int path1Len = 99, path2Len = 99, path3Len = 99, path4Len = 99;
        //Get the cost of each path
        if (path1 != null)
        {
            path1Len = 0;
            foreach (CollisionTile tile in path1)
                path1Len += tile.tileCost;
        }
        if (path2 != null)
        {
            path2Len = 0;
            foreach (CollisionTile tile in path2)
                path2Len += tile.tileCost;
        }
        if (path3 != null)
        {
            path3Len = 0;
            foreach (CollisionTile tile in path3)
                path3Len += tile.tileCost;
        }
        if (path4 != null)
        {
            path4Len = 0;
            foreach (CollisionTile tile in path4)
                path4Len += tile.tileCost;
        }

        //Retrieve the length of the shortest path
        int shortestPathLength = Mathf.Min(path1Len, path2Len); ;
        shortestPathLength = Mathf.Min(shortestPathLength, path3Len);
        shortestPathLength = Mathf.Min(shortestPathLength, path4Len);

        //Return the path that has the shortest length
        if (path1 != null && path1Len == shortestPathLength)
            return path1;
        if (path2 != null && path2Len == shortestPathLength)
            return path2;
        if (path3 != null && path3Len == shortestPathLength)
            return path3;
        //If all paths are either null or not the shortest path, even if path4 is null it is the returned path
        //Returning null here means no path is valid
        return path4;

    }

    public CollisionTile getTileAtPos(Vector3 coord, bool isMouse)
    {
        //Grab the true position in reference to the grid
        Vector3 truePos = grid.WorldToCell(coord);
        //Now ask the Collision Map which tile is at our desired location
        CollisionTile tile = map.tileAt(truePos);

        return tile;
    }
}
