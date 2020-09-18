using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Grid<GridObject>
{
    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    
    private int width;
    private int height;
    private float cellSize;
    private GridObject[,] gridArray;
    public class OnGridObjectChangedEventArgs : EventArgs
    {
        public int x;
        public int y;
    }
    public Grid(int width, int height, float cellSize, Func<Grid<GridObject>, int, int, GridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;

        gridArray = new GridObject[width, height];
        
        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                gridArray[x, y] = createGridObject(this, x, y);
            }
        }
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        Vector3 position = new Vector3(x, y) * cellSize;
        return position;
    }

    //private void HandleGridDrawAndText()
    //{
    //    Boolean debugMode = false;
    //    if (debugMode)
    //    {
    //        float debugDuration = 1000f;
    //        TextMesh[,] debugValueArray = new TextMesh[width, height];
    //        for (int x = 0; x < gridArray.GetLength(0); x++)
    //        {
    //            for (int y = 0; y < gridArray.GetLength(1); y++)
    //            {
    //                Vector3 textPosition = GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * 0.5f;
    //                //debugValueArray[x, y] = UtilsClass.CreateWorldText(gridArray[x, y].ToString(), null, textPosition, 10, Color.white, TextAnchor.MiddleCenter);
    //                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, debugDuration);
    //                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, debugDuration);
    //            }
    //        }

    //        Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, debugDuration);
    //        Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, debugDuration);
    //        OnGridObjectChanged += (object sender, OnGridObjectChangedEventArgs eventArgs) =>
    //        {
    //            debugValueArray[eventArgs.x, eventArgs.y].text = gridArray[eventArgs.x, eventArgs.y]?.ToString();
    //        };
    //    }
    //}

    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt(worldPosition.x / cellSize);
        y = Mathf.FloorToInt(worldPosition.y / cellSize);
    }

    public void SetGridObject(int x, int y, GridObject value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
            OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, y = y });
        }
    }

    public void TriggerGridObjectChanged(int x, int y)
    {
        OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, y = y });
    }

    public void SetGridObject(Vector3 wordPosition, GridObject value)
    {
        int x, y;
        GetXY(wordPosition, out x, out y);
        SetGridObject(x, y, value);
    }

    public GridObject GetGridObject(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            return default(GridObject);
        }
    }

    public GridObject GetGridObject(Vector3 wordPosition)
    {
        int x, y;
        GetXY(wordPosition, out x, out y);
        return GetGridObject(x, y);
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

