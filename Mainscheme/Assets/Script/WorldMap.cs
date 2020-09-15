using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMap : MonoBehaviour
{
    [SerializeField] 
    private BaseGrid baseGrid;
    [SerializeField]
    private PlayerMovement playerMovement;
    private PathFinding pathFinding;
    private int width;
    private int height;
    private float cellSize;
    private void Start()
    {
        pathFinding = baseGrid.GetPathFinding();
        width = pathFinding.GetWidth();
        height = pathFinding.GetHeight();
        cellSize = pathFinding.GetCellSize();
    }

    private void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    Vector3 mouseWorldPosition = UtilsClass.GetMouseWorldPosition();
        //    pathFinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
        //    if (x >= 0 && y >= 0 && x < width && y < height)
        //    {
        //        if (!pathFinding.GetNode(x, y).isBlocked)
        //        {
        //            Vector3 playerPosition = playerMovement.GetPositon();
        //            List<PathNode> path = pathFinding.FindPath(0, 0, x, y);
        //            if (path != null)
        //            {
        //                for (int i = 0; i < path.Count - 1; i++)
        //                {
        //                    PathNode currentNode = path[i];
        //                    PathNode nextNode = path[i + 1];
        //                    Vector3 currentPosition = new Vector3(currentNode.x, currentNode.y) * cellSize + new Vector3(cellSize, cellSize) * 0.5f;
        //                    Vector3 nextPosition = new Vector3(nextNode.x, nextNode.y) * cellSize + new Vector3(cellSize, cellSize) * 0.5f;
        //                    Debug.DrawLine(currentPosition, nextPosition, Color.green, 10f);
        //                }
        //            }
        //            //playerMovement.SetCharacterPosition(mouseWorldPosition);
        //        }
        //    }
        //}
    }
}
