using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorControl : MonoBehaviour
{
    private CollisionTile lastTile;

    private float virtualZ = -1;
    void Start()
    {
        
    }

    void Update()
    {
        if (MapBehavior.instance == null)
            return;
        Vector3 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currentPos = FollowMouseCursor(currentPos);
        transform.position = currentPos;
    }

    private Vector3 FollowMouseCursor(Vector3 currentMousePos)
    {
        CollisionTile mouseTile = MapBehavior.instance.getTileAtPos(currentMousePos);
        float cellSize = MapBehavior.instance.getGridCellSize();

        if (mouseTile != null)
        {
            if (lastTile == null || lastTile != mouseTile)
                GetComponent<AudioSource>().Play();
            lastTile = mouseTile;
            float cursorX = mouseTile.coordinate.x;
            float cursorY = mouseTile.coordinate.y;
            cursorX = Mathf.Clamp(cursorX, cellSize / 2, MapBehavior.instance.getMapwidth() * cellSize - cellSize / 2);
            cursorY = Mathf.Clamp(cursorY, cellSize / 2, MapBehavior.instance.getMapHeigth() * cellSize - cellSize / 2);
            Vector3 newPos = new Vector3(cursorX, cursorY, virtualZ);
            return newPos;
        }
        return transform.position;
    }
}
