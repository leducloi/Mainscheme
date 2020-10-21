using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This script controls camera behaviors. Anything that the camera does should be controlled through here.
 * It has an instance so it can be accessed by any class.
 */

public class CameraBehavior : MonoBehaviour
{
    public static CameraBehavior instance = null;
    [SerializeField]
    private float moveSpeed = 0;
    private float moveUnit = 24f;
    //Controls the boundaries of our camera
    private float mapHeight, mapWidth;
    private float cameraHalfHeight, cameraHalfWidth;
    private float cellSize = 1f;
    private float[] cameraLimitX;
    private float[] cameraLimitY;

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void setup()
    {
        //Setup 
        instance = this;
        int boundsX = MapBehavior.instance.getMapwidth();
        int boundsY = MapBehavior.instance.getMapHeigth();
        cellSize = MapBehavior.instance.getGridCellSize();
        //Set moveUnit, moveUnit is how many tiles to move when press WASD
        moveUnit = cellSize;
        //Get and set height and width of the grid
        mapHeight = boundsY;
        mapWidth = boundsX;
        //Get camera width and height
        cameraHalfHeight = Camera.main.orthographicSize;
        cameraHalfWidth = cameraHalfHeight * Camera.main.aspect;
        //Set the limit of width and height of the map
        cameraLimitX = new float[] { cameraHalfWidth, mapWidth * cellSize - cameraHalfWidth };
        cameraLimitY = new float[] { cameraHalfHeight, mapHeight * cellSize - cameraHalfHeight };
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 cameraPos = transform.position;
        //This controls the movement of the camera
        cameraPos = moveCamera(cameraPos);
        transform.position = cameraPos;
    }

    //Moves the movePoint of the camera on certain criteria
    private Vector3 moveCamera(Vector3 cameraPos)
    {
        // For mouse movement when it is over the top of the map
        if (Input.mousePosition.y >= Screen.height - moveUnit)
        {
            cameraPos.y += moveSpeed * Time.deltaTime;
        }

        if (Input.GetKeyDown("w"))
        {
            cameraPos.y += moveUnit;
        }

        // For mouse movement when it is below the bottom of the map
        if (Input.mousePosition.y <= moveUnit)
        {
            cameraPos.y -= moveSpeed * Time.deltaTime;
        }

        if (Input.GetKeyDown("s"))
        {
            cameraPos.y -= moveUnit;
        }

        // For mouse movement when it is over the right side of the map
        if (Input.mousePosition.x >= Screen.width - moveUnit)
        {
            cameraPos.x += moveSpeed * Time.deltaTime;
        }

        if (Input.GetKeyDown("d"))
        {
            cameraPos.x += moveUnit;
        }

        // For mouse movement when it is over the left side of the map
        if (Input.mousePosition.x <= moveUnit)
        {
            cameraPos.x -= moveSpeed * Time.deltaTime;
        }

        if (Input.GetKeyDown("a"))
        {
            cameraPos.x -= moveUnit;
        }

        //Set limit of camera width and height
        //CameraLimitX[0] is bottom bounce, CameraLimitX[1] is top bounce, same for CameraLimitY
        cameraPos.x = Mathf.Clamp(cameraPos.x, cameraLimitX[0], cameraLimitX[1]);
        cameraPos.y = Mathf.Clamp(cameraPos.y, cameraLimitY[0], cameraLimitY[1]);

        return cameraPos;
    }

    //Not working yet
    //Used to pan the camera to a specific point
    //public void panCameraTo(Vector3 destination)
    //{
    //    movePoint.position = destination;
    //}

    ////Used to snap the camera immediately to a specific point
    //public void snapCameraTo(Vector3 destination)
    //{
    //    transform.position = destination;
    //    movePoint.position = destination;
    //}
}
