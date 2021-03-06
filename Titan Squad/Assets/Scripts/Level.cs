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
    public static Level instance = null;

    [SerializeField]
    protected TextAsset levelScript = null;

    public EnemyController enemyController;
    public EnemyUnit[] enemyUnits;
    public Unit[] playerUnits;
    [SerializeField]
    protected GameObject[] objectives;
    public Vector3[] startPositions;
    public GameObject unitSelectMenu;

    public bool victory = false;

    public int numUnitsSelected = 0;

    private List<CollisionTile> startTiles;
    public List<PlayerUnit> selectedUnits;

    public bool pauseAutoEnd = false;

    public bool donePlanning = false;
    public bool levelDone = false;
    public bool continuePlay = false;

    public List<GameObject> activeObjectives;

    public int unitsExfilled = 0;

    public bool isTutorial;

    public int turnLimit = int.MaxValue;

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
        Instantiate(enemyController, transform);

        startTiles = new List<CollisionTile>();
        selectedUnits = new List<PlayerUnit>();

        playerUnits = GetComponentsInChildren<PlayerUnit>();

        if (isTutorial)
            selectedUnits.Add((PlayerUnit)playerUnits[0]);

        foreach (GameObject objective in activeObjectives)
        {
            objective.GetComponent<Objective>().beginObjective();
        }
        enemyUnits = GetComponentsInChildren<EnemyUnit>();
        
    }

    // Update is called once per frame
    virtual protected void Update()
    {
        if (donePlanning && (unitsExfilled == selectedUnits.Count || Input.GetKeyDown(KeyCode.Tab)))
        {
            levelCompleted();
            pauseAutoEnd = true;
            unitsExfilled = 0;
        }
        else if (GameManager.instance.turnCount >= turnLimit)
        {
            levelFailed();
        }

        //The level script handles automatically ending phases
        if (GameManager.instance.playerPhase && !pauseAutoEnd)
            autoEndPlayerPhase();
        if (GameManager.instance.enemyPhase && !pauseAutoEnd)
            autoEndEnemyPhase();
    }

    public void updateObjectives()
    {
        List<GameObject> updatedList = new List<GameObject>();
        foreach (GameObject objective in activeObjectives)
        {
            if (objective.GetComponent<Objective>().completed)
            {
                if (objective.GetComponent<Objective>().type == "Exfill")
                {
                    unitsExfilled = 0;
                    foreach (PlayerUnit u in playerUnits)
                        if (u.exfilled)
                            unitsExfilled++;
                }
                continue;
            }
            updatedList.Add(objective);
        }
        if (updatedList.Count == 0)
        {
            foreach (GameObject obj in objectives)
            {
                if (obj.GetComponent<Objective>().type == "Exfill")
                {
                    obj.GetComponent<Objective>().beginObjective();
                    updatedList.Add(obj);
                }
            }
        }

        activeObjectives.Clear();
        activeObjectives = updatedList;
    }

    public int getObjectiveCount(string id)
    {
        int ret = 0;
        if (id == "Route")
        {
            foreach (EnemyUnit u in enemyUnits)
                if (u != null)
                    ret++;

            return ret;
        }
        foreach (GameObject o in objectives)
            if (o.GetComponent<Objective>() != null && o.GetComponent<Objective>().type == id)
                ret++;

        return ret;
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
        //GameManager.instance.enemyPhase = true;
        foreach (Unit u in enemyUnits)
            MapBehavior.instance.unitMoved(u.transform.position, u.transform.position, true);
        GameManager.instance.enemyPhase = false;
        GameManager.instance.playerPhase = true;
        foreach (Unit u in playerUnits)
            MapBehavior.instance.unitMoved(u.transform.position, u.transform.position);
        GameManager.instance.playerPhase = false;
        StartCoroutine(GameManager.instance.endEnemyTurn());

    }


    public IEnumerator planning()
    {
        foreach (Unit u in enemyUnits)
        {
            CollisionTile tileOn = MapBehavior.instance.getTileAtPos(u.transform.position);
            u.transform.position = tileOn.coordinate;
            u.movePoint.transform.position = tileOn.coordinate;
        }

        CameraBehavior.instance.pauseMovement = true;
        yield return StartCoroutine(cutscene());
        CameraBehavior.instance.pauseMovement = false;

        Instantiate(GameManager.instance.cursor, transform);
        if (!isTutorial)
        {
            unitSelectMenu = Instantiate(unitSelectMenu, transform);

            for (int index = 0; index < startPositions.Length; index++)
            {
                startTiles.Add(MapBehavior.instance.getTileAtPos(startPositions[index]));
            }

            Vector3 pos = new Vector3(-1, -1, 0);
            foreach (Unit u in playerUnits)
            {
                u.movePoint.transform.position = pos;
                u.transform.position = pos;
            }

            MapBehavior.instance.setColor('b');
            GameObject startingHighlight = MapBehavior.instance.highlightObjectiveTiles(startTiles);


            while (!donePlanning)
            {
                yield return null;
            }

            MapBehavior.instance.deleteObjectiveTiles(startingHighlight);

            Destroy(unitSelectMenu);
        }
        else
        {
            foreach (Unit u in selectedUnits)
                u.GetComponent<SpriteRenderer>().enabled = true;
            finishPlanning();
        }

        MapBehavior.instance.setPlayerArray();
        levelSetup();
    }

    public abstract IEnumerator cutscene();

    public void finishPlanning()
    {
        Level.instance.donePlanning = true;
    }

    public void selectUnit(PlayerUnit selected)
    {
        if (selectedUnits.Count >= 3)
            selectedUnits.RemoveAt(0);
        selectedUnits.Add(selected);
    }

    protected virtual void levelCompleted()
    {
        victory = true;

        pauseAutoEnd = true;
        GameManager.instance.enemyPhase = false;
        GameManager.instance.playerPhase = false;
        if (!levelDone)
            StartCoroutine(postMapScreen());
        levelDone = true;
    }

    public virtual void levelFailed()
    {
        unitsExfilled = 0;

        pauseAutoEnd = true; GameManager.instance.enemyPhase = false;
        GameManager.instance.playerPhase = false;
        if (!levelDone)
            StartCoroutine(postMapScreen());
        levelDone = true;
    }

    IEnumerator postMapScreen()
    {
        UIManager.instance.showEndCard();
        while (!continuePlay)
            yield return null;

        GameManager.instance.levelFinished();
    }
}
