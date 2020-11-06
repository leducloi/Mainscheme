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

    public GameObject UIMan;

    //Booleans to control player and enemy phases
    public bool playerPhase;
    public bool enemyPhase;
    //Tracker to keep track of what map we are on
    public int currMap = 0;

    public int turnCount = 0;

    public GameObject cursor;
   

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

        Instantiate(UIMan);

        playerPhase = false;
        enemyPhase = false;

        loadNextMap();
    }

    void loadNextMap()
    {
        mapMan.loadMap(currMap++);
        playerPhase = false;
        enemyPhase = false;
        turnCount = 1;
    }

    // Update is called once per frame
    void Update()
    {
        //CollisionTile tile = MapBehavior.instance.getTileAtPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }

    //Called to end the player's turn
    public IEnumerator endPlayerTurn()
    {
        playerPhase = false;
        UIManager.instance.ShowEnemyMessage();
        foreach (Unit u in Level.instance.enemyUnits)
        {
            if (u != null && u.isActiveAndEnabled)
                u.healthBar.resetShields();
        }
        yield return new WaitForSeconds(2);
        
        enemyPhase = true;
        turnCount++;
    }

    //Called to end the enemy's turn
    public IEnumerator endEnemyTurn()
    {
        enemyPhase = false;
        UIManager.instance.ShowPlayerMessage();
        foreach (Unit u in Level.instance.playerUnits)
        {
            if (u != null && u.isActiveAndEnabled)
                u.healthBar.resetShields();
        }
        StartCoroutine(CameraBehavior.instance.panCameraTo(Level.instance.selectedUnits.ToArray()[0].transform.position, 1f));
        yield return new WaitForSeconds(2);
        
        playerPhase = true;
    }

    public void levelFinished()
    {
        loadNextMap();
    }
}
