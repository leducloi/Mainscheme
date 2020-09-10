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

    public PathNode(Grid<PathNode> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        this.isBlocked = false;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public void SetIsBlocked(Boolean isBlocked)
    {
        this.isBlocked = isBlocked;
        grid.TriggerGridObjectChanged(x, y);
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
