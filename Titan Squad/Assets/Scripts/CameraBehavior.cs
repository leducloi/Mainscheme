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
    public bool pauseMovement = false;

    private bool following = false;

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
        if (pauseMovement)
            return;

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
        if (mousePosition.y >= .95f)
        {
            cameraPos.y += moveSpeed * Time.deltaTime;
            following = false;
        }

        if (Input.GetKeyDown("w") && !pauseWASD)
        {
            cameraPos.y += moveUnit;
            following = false;
        }

        //For mouse movement when it is below the bottom of the map
        if (mousePosition.y <= 0.05f)
        {
            cameraPos.y -= moveSpeed * Time.deltaTime;
            following = false;
        }

        if (Input.GetKeyDown("s") && !pauseWASD)
        {
            cameraPos.y -= moveUnit;
            following = false;
        }

        //For mouse movement when it is over the right side of the map
        if (mousePosition.x >= .95f)
        {
            cameraPos.x += moveSpeed * Time.deltaTime;
            following = false;
        }

        if (Input.GetKeyDown("d") && !pauseWASD)
        {
            cameraPos.x += moveUnit;
            following = false;
        }

        //For mouse movement when it is over the left side of the map
        if (mousePosition.x <= 0.05f)
        {
            cameraPos.x -= moveSpeed * Time.deltaTime;
            following = false;
        }

        if (Input.GetKeyDown("a") && !pauseWASD)
        {
            cameraPos.x -= moveUnit;
            following = false;
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

    public IEnumerator panCameraTo(Vector3 destination, float seconds = 0)
    {
        following = false;

        pauseMovement = true;

        destination.z = transform.position.z;

        seconds *= 60;

        //Check zero/negative time, if so snap immediately to target
        if (seconds <= 0)
        {
            destination.x = Mathf.Clamp(destination.x, cameraLimitX[0], cameraLimitX[1]);
            destination.y = Mathf.Clamp(destination.y, cameraLimitY[0], cameraLimitY[1]);
            transform.position = destination;
            pauseMovement = false;
            yield break;
        }

        //Calculate how far in 1 frame to move
        float moveFactor = Vector3.Distance(transform.position, destination) / seconds;
        
        for (int s = 0; s < seconds; s++)
        {
            yield return null;

            Vector3 pos = Vector3.MoveTowards(transform.position, destination, moveFactor);
            float x0 = pos.x;
            float y0 = pos.y;
            pos.x = Mathf.Clamp(pos.x, cameraLimitX[0], cameraLimitX[1]);
            pos.y = Mathf.Clamp(pos.y, cameraLimitY[0], cameraLimitY[1]);
            transform.position = pos;

            //If both x and y were clamped, we can move no further towards the target and should end
            if (x0 != pos.x && y0 != pos.y)
                break;
        }

        pauseMovement = false;
    }

    public IEnumerator follow(GameObject target)
    {
        if (following)
        {
            following = false;
            yield return null;
            yield return null;
        }

        following = true;
        
        while (following)
        {
            Vector3 pos = target.transform.position;
            pos.z = transform.position.z;

            pos.x = Mathf.Clamp(pos.x, cameraLimitX[0], cameraLimitX[1]);
            pos.y = Mathf.Clamp(pos.y, cameraLimitY[0], cameraLimitY[1]);

            transform.position = pos;
            yield return null;
        }
    }
    
}
