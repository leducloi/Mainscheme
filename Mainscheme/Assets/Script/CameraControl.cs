using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraControl : MonoBehaviour
{
    private float moveUnit = 4f;
    private float moveSpeed = 40f;
    private Vector3 cameraFirstPos;
    private float cameraHalfHeight;
    private float cameraHalfWidth;
    private float gridHeight;
    private float gridWidth;
    private float gridCellSize;
    private float[] cameraLimitX;
    private float[] cameraLimitY;
    void Start()
    {
        gridHeight = BaseGrid.Instance.GetHeight();
        gridWidth = BaseGrid.Instance.GetWidth();
        gridCellSize = BaseGrid.Instance.GetCellSize();
        cameraHalfHeight = Camera.main.orthographicSize;
        cameraHalfWidth = cameraHalfHeight * Camera.main.aspect;
        cameraFirstPos = transform.position;
        cameraLimitX = new float[] {cameraHalfWidth, gridWidth*gridCellSize - cameraHalfWidth};
        cameraLimitY = new float[] {cameraHalfHeight, gridHeight * gridCellSize - cameraHalfHeight};
    }

    void Update()
    {
        Vector3 cameraPos = transform.position;
        moveUnit = BaseGrid.Instance.GetCellSize();

        cameraPos = SetCameraPosition(cameraPos); 

        transform.position = cameraPos;
    }

    private Vector3 SetCameraPosition(Vector3 cameraPos)
    {
        if (Input.mousePosition.y >= Screen.height - moveUnit)
        {
            cameraPos.y += moveSpeed * Time.deltaTime;
        }

        if (Input.GetKeyDown("w"))
        {
            cameraPos.y += moveUnit;
        }

        if (Input.mousePosition.y <= moveUnit)
        {
            cameraPos.y -= moveSpeed * Time.deltaTime;
        }

        if (Input.GetKeyDown("s"))
        {
            cameraPos.y -= moveUnit;
        }

        if (Input.mousePosition.x >= Screen.width - moveUnit)
        {
            cameraPos.x += moveSpeed * Time.deltaTime;
        }

        if (Input.GetKeyDown("d"))
        {
            cameraPos.x += moveUnit;
        }

        if (Input.mousePosition.x <= moveUnit)
        {
            cameraPos.x -= moveSpeed * Time.deltaTime;
        }

        if (Input.GetKeyDown("a"))
        {
            cameraPos.x -= moveUnit;
        }
        cameraPos.x = Mathf.Clamp(cameraPos.x, cameraLimitX[0], cameraLimitX[1]);
        cameraPos.y = Mathf.Clamp(cameraPos.y, cameraLimitY[0], cameraLimitY[1]);

        return cameraPos;
    }
}
