﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * -----------------SCRIPT INFORMATION-----------------
 * The point of this script is to manage the operations of the game as a generality. This means that 
 * this script is going to decide when maps need to change and whose turn it is. There should only be one, and so it is
 * implemented as a singleton. 
 * ----------------------------------------------------
 * Examples of script behavior: Ending turns, changing maps, loading title screen, selecting units/tiles.
 * Examples of how not to use the script: Moving the player units, any AI, loading specific maps.
 */

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    //Contains the Map Manager
    public static MapManager mapMan;

    //Booleans to control player and enemy phases
    public bool playerPhase;
    public bool enemyPhase;
    //Tracker to keep track of what map we are on
    public int currMap = 0;
<<<<<<< HEAD
=======
   
>>>>>>> Ready-Jared

    void Awake()
    {
        //If there are no instances of GameManager, instantiate it
        if (instance == null)
            instance = this;
        //If there is more than one instance of GameManager, destroy the copy
        else if (instance != this)
            Destroy(gameObject);

        //To ensure this object is not lost between scenes
        DontDestroyOnLoad(gameObject);

        mapMan = GetComponent<MapManager>();
        mapMan.loadMap(currMap);
        playerPhase = true;
        enemyPhase = false;
    }

    // Update is called once per frame
    void Update()
    {
<<<<<<< HEAD
        /*
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 start = new Vector3(1, 1, 0);
            CollisionTile[] path = MapBehavior.instance.getPathTo(start, Camera.main.ScreenToWorldPoint(Input.mousePosition), 5);
            if (path == null)
                Debug.Log("No valid path");
            else
                foreach (CollisionTile tile in path)
                    Debug.Log(tile.toString());
        }
        */
=======

>>>>>>> Ready-Jared
    }

    //Called to end the player's turn
    public void endPlayerTurn()
    {
        playerPhase = false;
        enemyPhase = true;
    }

    //Called to end the enemy's turn
    public void endEnemyTurn()
    {
        playerPhase = true;
        enemyPhase = false;
    }
}
