using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This script is entirely WIP. The purpose of this script is to control enemy behaviors and give control to individual units
 */

public class EnemyController : MonoBehaviour
{
<<<<<<< HEAD
    // Start is called before the first frame update
    void Start()
    {
        
=======
    public EnemyController instance = null;

    private bool controllingEnemies = false;

    // Start is called before the first frame update
    void Start()
    {
        //If there are no instances of EnemyController, just set it to this
        if (instance == null)
            instance = this;
        //If there is more than one instance of EnemyController, destroy the copy and reset it
        else if (instance != this)
        {
            Destroy(instance);
            instance = this;
        }
>>>>>>> Ready-Jared
    }

    // Update is called once per frame
    void Update()
    {
<<<<<<< HEAD
        
    }

    void updateUnitList()
    {

    }
=======
        //We only want to start controlling enemies if we aren't already controlling enemies
        if (GameManager.instance.enemyPhase && !controllingEnemies)
        {
            StartCoroutine(controlEnemies());
        }
    }

    //Ensures that only a single enemy is acting at one time
    IEnumerator controlEnemies()
    {
        controllingEnemies = true;
        foreach (EnemyUnit enemy in Level.instance.enemyUnits)
        {
            //If the slot is null, it means the enemy has been defeated and should be skipped
            if (enemy == null)
                continue;

            //Give control to the current enemy
            enemy.giveControl();

            //Wait until the unit gives up control
            while (enemy.hasControl)
                yield return null;

            //Add slight pause between units moving
            yield return new WaitForSeconds(0.25f);
        }
        controllingEnemies = false;
    }
    
>>>>>>> Ready-Jared
}
