﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This script controls camera behaviors. Anything that the camera does should be controlled through here.
 * It has an instance so it can be accessed by any class.
 */

public class CameraBehavior : MonoBehaviour
{
    public static CameraBehavior instance = null;
    private float moveSpeed = 10f;
    //Controls the boundaries of our camera
    private int minX, minY, maxX, maxY;

    public Transform movePoint;


    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        movePoint.SetParent(null);
        int boundsX = MapBehavior.instance.tilemap.cellBounds.size.x;
        int boundsY = MapBehavior.instance.tilemap.cellBounds.size.y;
        minX = 12;
        minY = 6;
        maxX = boundsX - 12;
        maxY = boundsY - 6;
    }

    // Update is called once per frame
    void Update()
    {
        //Always move the camera towards the move point, this handles camera panning
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);
        
        //This controls the movement of the camera
        if (Vector3.Distance(transform.position, movePoint.position) <= 0.05f)
        {
            //If there's a horizontal input, move the camera that amount horizontally (1 for right, -1 for left)
            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) == 1f)
            {
                Vector3 attempted = movePoint.position + new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f);
                if (inBounds(attempted))
                    movePoint.position = attempted;
            }
            //If there's a vertical input, move the camera that amount vertically (1 for up, -1 for down)
            if (Mathf.Abs(Input.GetAxisRaw("Vertical")) == 1f)
            {
                Vector3 attempted = movePoint.position + new Vector3(0f, Input.GetAxisRaw("Vertical"), 0f);
                if (inBounds(attempted))
                    movePoint.position = attempted;
            }
        }
    }

    private bool inBounds(Vector3 attemptedPosition)
    {
        return (attemptedPosition.x <= maxX && attemptedPosition.x >= minX && attemptedPosition.y <= maxY && attemptedPosition.y >= minY);
    }

    //Used to pan the camera to a specific point
    public void panCameraTo(Vector3 destination)
    {
        movePoint.position = destination;
    }

    //Used to snap the camera immediately to a specific point
    public void snapCameraTo(Vector3 destination)
    {
        transform.position = destination;
        movePoint.position = destination;
    }
}
