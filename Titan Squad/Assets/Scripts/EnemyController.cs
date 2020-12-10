using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This script is entirely WIP. The purpose of this script is to control enemy behaviors and give control to individual units
 */

public class EnemyController : MonoBehaviour
{
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
    }

    // Update is called once per frame
    void Update()
    {
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
        bool enemies = false;

        //yield return new WaitForSeconds(0.33f);

        foreach (EnemyUnit enemy in Level.instance.enemyUnits)
        {
            //If the slot is null, it means the enemy has been defeated and should be skipped
            if (enemy == null)
                continue;


            GameObject playerObject = MapBehavior.instance.getClosestPlayerObject(enemy.transform.position);
            Vector3 playerPosition = playerObject.transform.position;
            float actualDistance = Mathf.Abs(playerPosition.x - enemy.transform.position.x) + Mathf.Abs(playerPosition.y - enemy.transform.position.y);
            if (!(actualDistance <= enemy.detectRange && !playerObject.GetComponent<PlayerUnit>().isCloaked))
            {
                enemy.giveControl();
                enemy.endTurn();
                continue;
            }

            enemies = true;

            enemy.showOutline();

            yield return StartCoroutine(CameraBehavior.instance.panCameraTo(enemy.transform.position, 1f));

            StartCoroutine(CameraBehavior.instance.follow(enemy.gameObject));

            //Give control to the current enemy
            enemy.giveControl();

            //Wait until the unit gives up control
            while (enemy.hasControl)
                yield return null;

            enemy.hideOutline();

            //Add slight pause between units moving
            yield return new WaitForSeconds(0.33f);
        }
        //In order to give the player their turn, add a slight pause if all enemies are defeated
        if (!enemies)
            yield return new WaitForSeconds(.5f);

        controllingEnemies = false;
    }
    
}
