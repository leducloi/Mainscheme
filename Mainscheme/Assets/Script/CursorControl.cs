using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorControl : MonoBehaviour
{
    private float virtualZ = -1;
    void Start()
    {
        
    }

    void Update()
    {
        Vector3 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currentPos = FollowMouseCursor(currentPos);
        transform.position = new Vector3(currentPos.x, currentPos.y, -1);
    }

    private Vector3 FollowMouseCursor(Vector3 currentMousePos)
    {
        BaseGrid.Instance.GetXY(currentMousePos, out int x, out int y);
        float cellSize = BaseGrid.Instance.GetCellSize();
        float cursorX = x * cellSize + cellSize / 2;
        float cursorY = y * cellSize + cellSize / 2;
        cursorX = Mathf.Clamp(cursorX, cellSize / 2, BaseGrid.Instance.GetWidth() * cellSize - cellSize / 2);
        cursorY = Mathf.Clamp(cursorY, cellSize / 2, BaseGrid.Instance.GetHeight() * cellSize - cellSize / 2);
        Vector3 newPos = new Vector3(cursorX, cursorY, virtualZ);
        return newPos;
    }
}
