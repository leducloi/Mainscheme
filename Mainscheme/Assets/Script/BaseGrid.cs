using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BaseGrid : MonoBehaviour
{
    private Tilemap tilemap;
    private int width;
    private int height;
    private float cellSize;
    TileBase[] allTiles;
    private PathFinding pathfinding;
    private BoundsInt bounds;
    public static BaseGrid Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
        tilemap = GetComponent<Tilemap>();

        bounds = tilemap.cellBounds;

        Vector3 cell = tilemap.cellSize;
        cellSize = cell.x;
        width = bounds.size.x;
        height = bounds.size.y;
        Debug.Log(width + " " + height);
        pathfinding = new PathFinding(width, height, cellSize);
        allTiles = tilemap.GetTilesBlock(bounds);
        setBlocked();
    }

    private void setBlocked()
    {
        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if (tile != null)
                {
                    Boolean isBlocked = SetBlockedTiles.CheckBlockedTile(tile.name);
                    PathNode node = pathfinding.GetNode(x, y);
                    if (SetBlockedTiles.CheckBlockedTile(tile.name))
                    {
                        node.SetIsBlocked(isBlocked);
                    }
                    else
                    {
                        node.SetTileValueNode(TileValue.GetTileValue(tile.name));
                    }
                }
                else
                {
                    pathfinding.GetNode(x, y).SetIsBlocked(true);
                }
            }
        }
    }

    public PathFinding GetPathFinding()
    {
        return pathfinding;
    }

    public float GetCellSize()
    {
        return cellSize;
    }
}
