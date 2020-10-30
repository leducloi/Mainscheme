using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This script handles behavior that is common to all levels. All levels should inherit this class.
 * In the future there may be additions to this class but for now it is finished.
 * The only additions I can foresee is potentially objective behavior
 */

public abstract class Level : MonoBehaviour
{
    public static Level instance = null;
    public EnemyController enemyController;
    public Unit[] enemyUnits;
    public Unit[] playerUnits;
    public GameObject[] objectives;

    public bool pauseAutoEnd = false;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        //If there are no instances of Level, just set it to this
        if (instance == null)
            instance = this;
        //If there is more than one instance of Level, destroy the copy and reset it
        else if (instance != this)
        {
            Destroy(instance);
            instance = this;
        }
        Instantiate(enemyController);

        
    }

    // Update is called once per frame
    virtual protected void Update()
    {
        //The level script handles automatically ending phases
        if (GameManager.instance.playerPhase && !pauseAutoEnd)
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
            if (unit != null && !unit.hasMoved())
                return;
        }
        endTurn();
    }


    //Checks if the enemy phase is over and ends it automatically
    void autoEndEnemyPhase()
    {
        //If each unit in the enemy team has acted, the turn is over
        foreach (Unit unit in enemyUnits)
        {
            if (unit != null && !unit.hasMoved())
                return;
        }
        endTurn();
    }

    public void endTurn()
    {
        if (GameManager.instance.playerPhase)
            StartCoroutine(GameManager.instance.endPlayerTurn());
        else
            StartCoroutine(GameManager.instance.endEnemyTurn());
    }

    public Unit getUnitAtLoc(Vector3 location)
    {
        foreach (EnemyUnit u in enemyUnits)
        {
            if (u == null)
                continue;
            if (u.transform.position.x == location.x && u.transform.position.y == location.y)
                return u;
        }
        foreach (PlayerUnit u in playerUnits)
        {
            if (u.transform.position.x == location.x && u.transform.position.y == location.y)
                return u;
        }
        return null;
    }

    public void levelSetup()
    {
        GameManager.instance.enemyPhase = true;
        foreach (Unit u in enemyUnits)
            MapBehavior.instance.unitMoved(u.transform.position, u.transform.position, true);
        GameManager.instance.enemyPhase = false;
        GameManager.instance.playerPhase = true;
        foreach (Unit u in playerUnits)
            MapBehavior.instance.unitMoved(u.transform.position, u.transform.position);
        GameManager.instance.playerPhase = false;
        StartCoroutine(GameManager.instance.endEnemyTurn());
        Instantiate(GameManager.instance.cursor);
    }

    public abstract void cutscene();


}
