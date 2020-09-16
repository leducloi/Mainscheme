using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SetBlockedTiles
{
    static List<String> allNOTBlockedTiles;

    static SetBlockedTiles()
    {
        allNOTBlockedTiles = new List<String>();
        allNOTBlockedTiles.Add("grass_tile_1");
        allNOTBlockedTiles.Add("sand_tile");
    }

    public static Boolean CheckBlockedTile(String checkTileName)
    {
        Boolean isBlocked = false;
        if (!allNOTBlockedTiles.Contains(checkTileName))
        {
            isBlocked = true;
        }
        return isBlocked;
    }
}
