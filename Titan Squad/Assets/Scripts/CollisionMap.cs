using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

/*
 * -----------------SCRIPT INFORMATION-----------------
 * This script is entirely a class. The purpose of this class is to hold an array of CollisionTiles created based off of an
 * input TileBase array, which is the way tilemaps are stored in Unity. In other words, it takes the useless TileBase array and turns
 * it into a new array with useful tile information for each of the tiles in the tilemap.
 * ----------------------------------------------------
 */

public class CollisionMap
{
    //Holds a version of the Tilemap with tile information
    public CollisionTile[] map;

    //Rows and Columns of the map, originating at the bottom left corner
    public int rows;
    public int cols;

    public CollisionMap (TileBase[] rawTiles, int width, int height)
    {
        rows = height;
        cols = width;

        map = new CollisionTile[rows * cols];
        //Division more costly, use inverse and multiply
        float inverseCols = 1.0f / cols;

        int len = rawTiles.Length;
        for(int x = 0; x < len; x++)
        {
            //To get the row, divide position by # of columns
            //To get the columns, get the remainder from the division of columns
            map[x] = new CollisionTile(rawTiles[x].name, x % cols + .5f, (int)(x * inverseCols) + .5f );
            //The tilemap is offset by .5, so we need to offset where our tiles are by the same amount
        }
    }

    public CollisionTile tileAt(Vector3 location)
    {
        //If the location clicked on is outside of our tile range, return null
        if (location.x < 0 || location.y < 0 || location.x >= cols || location.y >= rows)
            return null;

        //If the location was potentially valid, generate an index
        int index = (int)(location.x + location.y * cols);

        //Return the value of that index or null if the generated index was invalid
        if (index < 0 || index >= map.Length)
            return null;
        return map[index];
    }
}
