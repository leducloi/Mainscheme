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

    private float horizontalResolution = 1920;
    private float storedResolution;

    public bool pauseWASD = false;

    private int ScreenSizeX = 0;
    private int ScreenSizeY = 0;

    void Awake()
    {
        RescaleCamera();
    }
    // Start is called before the first frame update
    void Start()
    {
        //If there are no instances of CameraBehavior, just set it to this
        if (instance == null)
            instance = this;
        //If there is more than one instance of CameraBehavior, destroy the copy and reset it
        else if (instance != this)
        {
            Destroy(instance);
            instance = this;
        }
    }

    public void setup()
    {
        //Setup 
        storedResolution = (float)Screen.width / (float)Screen.height;
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
        readjustCamera();
        

        Vector3 cameraPos = transform.position;
        //This controls the movement of the camera
        cameraPos = moveCamera(cameraPos);
        transform.position = cameraPos;
    }

    //Moves the movePoint of the camera on certain criteria
    private Vector3 moveCamera(Vector3 cameraPos)
    {
        Vector3 mousePosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        // For mouse movement when it is over the top of the map
        if (mousePosition.y >= 1f)
        {
            cameraPos.y += moveSpeed * Time.deltaTime;
        }

        if (Input.GetKeyDown("w") && !pauseWASD)
        {
            cameraPos.y += moveUnit;
        }

        //For mouse movement when it is below the bottom of the map
        if (mousePosition.y <= 0f)
        {
            cameraPos.y -= moveSpeed * Time.deltaTime;
        }

        if (Input.GetKeyDown("s") && !pauseWASD)
        {
            cameraPos.y -= moveUnit;
        }

        //For mouse movement when it is over the right side of the map
        if (mousePosition.x >= 1f)
        {
            cameraPos.x += moveSpeed * Time.deltaTime;
        }

        if (Input.GetKeyDown("d") && !pauseWASD)
        {
            cameraPos.x += moveUnit;
        }

        //For mouse movement when it is over the left side of the map
        if (mousePosition.x <= 0f)
        {
            cameraPos.x -= moveSpeed * Time.deltaTime;
        }

        if (Input.GetKeyDown("a") && !pauseWASD)
        {
            cameraPos.x -= moveUnit;
        }

        //Set limit of camera width and height
        //CameraLimitX[0] is bottom bounce, CameraLimitX[1] is top bounce, same for CameraLimitY
        cameraPos.x = Mathf.Clamp(cameraPos.x, cameraLimitX[0], cameraLimitX[1]);
        cameraPos.y = Mathf.Clamp(cameraPos.y, cameraLimitY[0], cameraLimitY[1]);

        return cameraPos;
    }

    void readjustCamera()
    {
        float currentAspect = (float)Screen.width / (float)Screen.height;
        if (currentAspect != storedResolution)
        {
            Camera.main.orthographicSize = horizontalResolution / currentAspect / 200;
            setup();
        }
    }

    private void RescaleCamera()
    {

        if (Screen.width == ScreenSizeX && Screen.height == ScreenSizeY) return;

        //RESOLUTION
        float targetaspect = 16.0f / 9.0f;
        float windowaspect = (float)Screen.width / (float)Screen.height;
        float scaleheight = windowaspect / targetaspect;
        Camera camera = GetComponent<Camera>();

        if (scaleheight < 1.0f)
        {
            Rect rect = camera.rect;

            rect.width = 1.0f;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1.0f - scaleheight) / 2.0f;

            camera.rect = rect;
        }
        else
        {
            float scalewidth = 1.0f / scaleheight;

            Rect rect = camera.rect;

            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;

            camera.rect = rect;
        }

        ScreenSizeX = Screen.width;
        ScreenSizeY = Screen.height;
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
