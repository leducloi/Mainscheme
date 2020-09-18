using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public class PathFinding
{
    private const int MOVE_STRAIGHT_COST = 10;

    public static PathFinding Instance { get; private set; }
    private Grid<PathNode> grid;
    private List<PathNode> openList;
    private List<PathNode> closedList;
    private int width;
    private int height;
    private float cellSize;

    public PathFinding(int width, int height, float cellSize)
    {
        Instance = this;
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        grid = new Grid<PathNode>(width, height, cellSize, (Grid<PathNode> g, int x, int y) => new PathNode(g, x, y));
    }

    public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition, int? movementCost = null)
    {
        grid.GetXY(startWorldPosition, out int startX, out int startY);
        grid.GetXY(endWorldPosition, out int endX, out int endY);
        if (endX >= 0 && endY >= 0 && endX < width && endY < height)
        {
            List<PathNode> path = FindPath(startX, startY, endX, endY, movementCost);
            if (path == null)
            {
                return null;
            }
            else
            {
                List<Vector3> vectorPath = new List<Vector3>();
                foreach (PathNode pathNode in path)
                {
                    vectorPath.Add(new Vector3(pathNode.x, pathNode.y) * grid.GetCellSize() + Vector3.one * grid.GetCellSize() * .5f);
                }
                return vectorPath;
            }
        }
        else
        {
            return null;
        }
    }

    public List<PathNode> FindPath(int startX, int startY, int endX, int endY, int? movementCost = null)
    {
        PathNode startNode = grid.GetGridObject(startX, startY);
        PathNode endNode = grid.GetGridObject(endX, endY);

        openList = new List<PathNode> { startNode };
        closedList = new List<PathNode>();

        InitializeAllPathNodes();
        startNode.gCost = 0;
        startNode.hCost = CalculateDistance(startNode, endNode);
        startNode.CalculateFCost();

        while(openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode)
            {
                List<PathNode> tempPath = CalculatePathNodes(endNode, movementCost);
                if (tempPath != null)
                {
                    return tempPath;
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            List<PathNode> allNeighbor = FindNeighbor(currentNode);
            foreach(PathNode neighbor in allNeighbor)
            {
                if (!closedList.Contains(neighbor))
                {
                    if (!neighbor.isBlocked)
                    {
                        int newGCost = currentNode.gCost + CalculateGDistance(currentNode, neighbor);
                         
                        if (newGCost < neighbor.gCost)
                        {
                            neighbor.cameFromNode = currentNode;
                            neighbor.gCost = newGCost;
                            neighbor.hCost = CalculateDistance(neighbor, endNode);
                            neighbor.CalculateFCost();

                            if (!openList.Contains(neighbor))
                            {
                                openList.Add(neighbor);
                            }
                        }
                    }
                    else
                    {
                        closedList.Add(neighbor);
                    }
                }
            }
        }

        return null;
    }

    private List<PathNode> FindNeighbor(PathNode currentNode)
    {
        List<PathNode> allNeighbor = new List<PathNode>();
        if (currentNode.x - 1 >= 0)
        {
            allNeighbor.Add(GetNode(currentNode.x - 1, currentNode.y));
        }

        if (currentNode.x + 1 < grid.GetWidth())
        {
            allNeighbor.Add(GetNode(currentNode.x + 1, currentNode.y));
        }

        if (currentNode.y - 1 >= 0)
        {
            allNeighbor.Add(GetNode(currentNode.x, currentNode.y - 1));
        }

        if (currentNode.y + 1 < grid.GetHeight())
        {
            allNeighbor.Add(GetNode(currentNode.x, currentNode.y + 1));
        }
        return allNeighbor;
    }

    public PathNode GetNode(int x, int y)
    {
        return grid.GetGridObject(x, y);
    }

    public Grid<PathNode> GetGrid()
    {
        return grid;
    }

    private List<PathNode> CalculatePathNodes(PathNode endNode, int? movementCost = null)
    {
        List<PathNode> finalPath = new List<PathNode>();
        finalPath.Add(endNode);
        PathNode currentNode = endNode;
        while (currentNode.cameFromNode != null)
        {
            finalPath.Add(currentNode.cameFromNode);
            if (movementCost != null)
                movementCost -= currentNode.tileValue;
            currentNode = currentNode.cameFromNode;
        }
        if (movementCost < 0 && movementCost != null)
        {
            //Debug.Log("Out of movement cost...");
            return null;
        }
        //if (movementCost != null)
        //    Debug.Log("Remaining Movement Cost: " + movementCost);

        finalPath.Reverse();
        return finalPath;
    }

    private void InitializeAllPathNodes()
    {
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                PathNode pathNode = grid.GetGridObject(x, y);
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }
        }
    }

    private int CalculateDistance(PathNode start, PathNode end)
    {
        int xDistance = Mathf.Abs(start.x - end.x);
        int yDistance = Mathf.Abs(start.y - end.y);
        int cost = MOVE_STRAIGHT_COST * (xDistance + yDistance);
        return cost;
    }

    private int CalculateGDistance(PathNode start, PathNode end)
    {
        int xDistance = Mathf.Abs(start.x - end.x);
        int yDistance = Mathf.Abs(start.y - end.y);
        int cost;
        if (end.tileValue != 1)
        {
            cost = MOVE_STRAIGHT_COST * (xDistance + yDistance + end.tileValue);
        }
        else
        {
            cost = MOVE_STRAIGHT_COST * (xDistance + yDistance);
        }
        
        return cost;
    }

    private PathNode GetLowestFCostNode(List<PathNode> checkPNodeList)
    {
        int smallestFcost = checkPNodeList[0].fCost;
        PathNode lowestFCostNode = checkPNodeList[0];
        for (int i = 1; i < checkPNodeList.Count; i++)
        {
            int currentFcost = checkPNodeList[i].fCost;
            if (currentFcost < smallestFcost)
            {
                smallestFcost = currentFcost;
                lowestFCostNode = checkPNodeList[i];
            }
        }
        return lowestFCostNode;
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public float GetCellSize()
    {
        return cellSize;
    }
}
