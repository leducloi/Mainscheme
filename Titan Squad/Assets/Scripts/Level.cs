﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This script handles behavior that is common to all levels. All levels should inherit this class.
 * In the future there may be additions to this class but for now it is finished.
 * The only additions I can foresee is potentially objective behavior
 */

public abstract class Level : MonoBehaviour
{
    public Unit[] enemyUnits;
    public Unit[] playerUnits;
    public GameObject[] objectives;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    virtual protected void Update()
    {
        //The level script handles automatically ending phases
        if (GameManager.instance.playerPhase)
            autoEndPlayerPhase();
        if (GameManager.instance.enemyPhase)
            autoEndEnemyPhase();
    }

    //Checks if the player phase is over and ends it automatically
    void autoEndPlayerPhase()
    {
        //If each unit in the player team has acted, the turn is over
        foreach (Unit unit in playerUnits)
        {
            if (!unit.hasMoved())
                return;
        }
        GameManager.instance.endPlayerTurn();
    }

    //Checks if the enemy phase is over and ends it automatically
    void autoEndEnemyPhase()
    {
        //If each unit in the enemy team has acted, the turn is over
        foreach (Unit unit in enemyUnits)
        {
            if (!unit.hasMoved())
                return;
        }
        GameManager.instance.endEnemyTurn();
    }

    public abstract void cutscene();


}
