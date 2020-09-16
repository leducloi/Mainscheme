using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    private Grid<PathNode> grid;
    public int x;
    public int y;

    public int gCost;
    public int hCost;
    public int fCost;

    public PathNode cameFromNode;
    public Boolean isBlocked;
    public int tileValue;

    public PathNode(Grid<PathNode> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        this.isBlocked = false;
        this.tileValue = 1;
    }

    public PathNode CalculateFCost()
    {
        fCost = gCost + hCost;
        return this;
    }

    public PathNode SetIsBlocked(Boolean isBlocked)
    {
        this.isBlocked = isBlocked;
        grid.TriggerGridObjectChanged(x, y);
        return this;
    }

    public PathNode SetTileValueNode(int tileValue)
    {
        this.tileValue = tileValue;
        grid.TriggerGridObjectChanged(x, y);
        return this;
    }

    public override string ToString()
    {
        if (isBlocked)
        {
            return ("Blocked");
        }
        else
        {
            return ("");
        }
        
    }
}
