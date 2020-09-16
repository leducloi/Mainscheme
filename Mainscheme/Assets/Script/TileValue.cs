using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TileValue
{
    static Dictionary<string, int> allTypeOfTiles;
    static TileValue()
    {
        allTypeOfTiles = new Dictionary<string, int>();
        allTypeOfTiles.Add("grass_tile_1", 1);
        allTypeOfTiles.Add("sand_tile", 2);
    }

    public static int GetTileValue(string tileName)
    {
        int tileValue = allTypeOfTiles[tileName];
        return tileValue;
    }
}
