﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.MemoryProfiler;
using UnityEngine;

public class EnemyUnit : Unit
{
    //Set of patrol path patterns
    [SerializeField]
    private GameObject patrolPath = null;

    //Set fasle if enemy does not move
    [SerializeField]
    private bool patrolEnemy;

    //Variables to control patron patterns
    private int numberOfPatternChild;
    private Transform allPathPatterns;
    //patternIndex starts with 1 since 0 is the first location
    private int patternIndex = 1;
    //moveFoward to check if enemy is moving up the list of pattern or moving back the list of pattern
    private bool moveFoward = true;

    //Boolean to tell the unit if it has control
    public bool hasControl;
    //Boolean to only try to move while it can still move
    private bool takingTurn = false;
    private bool performingAction = false;
    private int actionPoints = 2;

    private int movement = 5;
    private float moveSpeed = 5f;
    private float detectRange = 10f;
    private GameObject detectedPlayerObject;

    //Mode affects what actions the enemy will take
    public string mode;
    //Possible modes: Guard, Patrol, Investigate, Attack (+ maybe more)

    // Start is called before the first frame update
    protected override void Start()
    {
        hpMax = 8;
        hpRemaining = 8;

        shieldMax = 3;
        shieldRemaining = 3;


        if (patrolPath == null)
            mode = "Guard";

        if (mode.Equals("Patrol"))
            patrolEnemy = true;
        else
            patrolEnemy = false;


        if (patrolEnemy)
        {
            numberOfPatternChild = patrolPath.transform.childCount;
            allPathPatterns = patrolPath.transform;
        }
        hasControl = false;

        weapons[0] = new Weapon("Krimbar Rifle");
        weapons[1] = new Weapon("--");

        base.Start();
        shaderControl.setColor(false);

    }

    // Update is called once per frame
    override
    protected void Update()
    {
        base.Update();

        if (hpRemaining <= 0)
        {
            unitDefeated();
            return;
        }
        //If it's the player's phase, give this unit a turn and allow it to move
        if (!GameManager.instance.enemyPhase)
        {
            hasTurn = true;
            takingTurn = false;
            actionPoints = 2;
        }

        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);
        //We only want one unit to have control at a time, so unit actions can only occur while the unit has control
        if (hasControl)
        {
            //If it's enemy phase and the unit can move, have it move
            if (GameManager.instance.enemyPhase && !takingTurn)
            {
                takingTurn = true;
                StartCoroutine(takeTurn());
            }
        }
    }

    IEnumerator takeTurn()
    {
        performingAction = false;
        showOutline();
        //Find the closest player and detect them if they are in range
        detectPlayerInRange(transform.position);

        //If the unit has not detected the player, have them patrol or guard and end their turn.
        if (mode != "Chase")
        {
            move();
            yield break;
        }
        while (actionPoints > 0)
        {
            yield return null;

            PlayerUnit target = selectTarget();
            if (target == null)
            {
                yield return StartCoroutine(chasingPlayer());
            }
            else
            {
                performingAction = true;
                attack(target);
            }
            while (performingAction)
                yield return null;

            yield return new WaitForSeconds(0.1f);
            actionPoints--;
        }
        setFinishMove(transform.position);
    }

    private void detectPlayerInRange(Vector3 currentPosition)
    {
        GameObject playerObject = MapBehavior.instance.getClosestPlayerObject(currentPosition);
        Vector3 playerPosition = playerObject.transform.position;
        float actualDistance = Mathf.Abs(playerPosition.x - currentPosition.x) + Mathf.Abs(playerPosition.y - currentPosition.y);
        if (actualDistance <= detectRange)
        {
            mode = "Chase";
            detectedPlayerObject = playerObject;
        }
    }

    //Each unit will move in a different way depending on it's mode
    override
    public void move()
    {
        if (mode.Equals("Patrol"))
            StartCoroutine(patroling());
        else if (mode.Equals("Guard"))
            StartCoroutine(guarding());
        else if (mode.Equals("Chase"))
            StartCoroutine(chasingPlayer());
    }

    IEnumerator chasingPlayer()
    {
        Vector3 currentPosition = transform.position;
        CollisionTile[] path = MapBehavior.instance.getPathTo(currentPosition, detectedPlayerObject.transform.position, null, true);
        if (path != null)
            path = path.Take(path.Length - 1).ToArray();
        yield return StartCoroutine(moveAlongPath(path, true));
        MapBehavior.instance.unitMoved(currentPosition, transform.position);
        yield return null;
    }

    IEnumerator guarding()
    {
        animator.SetTrigger("Walking");
        yield return new WaitForSeconds(0.5f);
        animator.SetTrigger("Stopped");
        setFinishMove(transform.position);
    }

    IEnumerator patroling()
    {
        if (patrolEnemy)
        {
            Vector3 start = transform.position;
            Vector3 tempTargetPosition = allPathPatterns.GetChild(patternIndex).position;
            CollisionTile[] path = MapBehavior.instance.getPathTo(transform.position, tempTargetPosition);

            //If the patternIndex is at the end of the list -> move backward
            if (patternIndex == numberOfPatternChild - 1)
                moveFoward = false;
            //patternIndex is at the beginning of the list -> move foward
            if (patternIndex == 0)
                moveFoward = true;
            //Depending on foward of backward, iterate the index as follow
            if (moveFoward)
                patternIndex++;
            else
                patternIndex--;
            yield return StartCoroutine(moveAlongPath(path));
            setFinishMove(start);
            yield return null;

        }
        else
        {
            yield return StartCoroutine(forNotPatrolEnemy());
        }

        yield return null;

    }

    //This method gives control to the unit so only one unit has control at once
    //Currently not used, will be used by the Enemy Controller 
    public void giveControl()
    {
        hasControl = true;
    }

    //move along the path of patrol or path when detects player
    IEnumerator moveAlongPath(CollisionTile[] path, bool withMomentCost = false)
    {
        if (path == null)
            yield break;
        //Grab the start of our move
        int index = 0;
        Vector3 start = transform.position;
        animator.SetTrigger("Walking");
        while (index < path.Length)
        {
            if (withMomentCost && index >= movement)
            {
                yield return null;
                break;
            }
            if (Vector3.Distance(transform.position, movePoint.position) == 0)
            {
                movePoint.position = path[index].coordinate;
                index++;
            }
            //So we don't infinite loop, we pause this coroutine at the end of each iteration
            yield return null;
        }

        while (Vector3.Distance(transform.position, movePoint.position) != 0)
            yield return null;
        animator.SetTrigger("Stopped");

        yield return null;
    }

    IEnumerator forNotPatrolEnemy()
    {
        Vector3 start = transform.position;
        animator.SetTrigger("Walking");
        yield return new WaitForSeconds(1f);
        animator.SetTrigger("Stopped");
        setFinishMove(start);
        yield return null;
    }

    //method to set the enemy unit after finishing the move
    private void setFinishMove(Vector3 startPosition)
    {
        MapBehavior.instance.unitMoved(startPosition, transform.position);
        actionPoints = 2;
        hideOutline();
        takingTurn = false;
        hasTurn = false;
        hasControl = false;

        hpTypeFlesh = true;
    }


    //WIP - Used to have a unit attack an enemy
    override
    public void attack(Unit enemy)
    {
        enemy.showOutline();
        StartCoroutine(playAttack(enemy));
    }

    override
    public void hit(int damage)
    {
        StartCoroutine(playHit(damage));
    }

    IEnumerator playHit(int damage)
    {
        //play hit animation
        healthBar.takeDamage(damage);

        yield return null;

        while (healthBar.movingBar)
            yield return null;

        hpRemaining = healthBar.health;
        shieldRemaining = healthBar.shields;

        if (hpRemaining > hpMax)
            hpRemaining = hpMax;
        //check if death

    }

    private void unitDefeated()
    {
        MapBehavior.instance.getMap().unitDefeated(transform.position, true);
        Destroy(gameObject);
    }

    IEnumerator playAttack(Unit target)
    {
        //Play attack animation
        yield return new WaitForSeconds(0.25f);

        //Calculate results
        CombatCalculator.instance.calculate(this, target);

        //Do damage
        if (CombatCalculator.instance.doesHit)
            target.hit(CombatCalculator.instance.damageDone);
        else
            UIManager.instance.attackMissed(target.transform.position);

        while (target.healthBar.movingBar)
            yield return null;

        performingAction = false;
        target.hideOutline();
    }

    private PlayerUnit selectTarget()
    {
        List<Unit> unitsInRange = MapBehavior.instance.getUnitsInRange(transform.position, equippedWeapon.maxRange, equippedWeapon.minRange);
        if (unitsInRange.Count == 0)
            return null;

        float[] damagePotentials = new float[unitsInRange.Count];

        int index = 0;
        /* Calculate potential for damage for each unit
         * Define damage potential as (Damage Done) * (Hit Chance)
         */
        foreach (Unit target in unitsInRange)
        {
            
            CombatCalculator.instance.calculate(this, target);
            damagePotentials[index] = (float)CombatCalculator.instance.damageDone * (float)CombatCalculator.instance.hitChanceDisplay;
            index++;
        }

        //Find the maximum damage potential
        int maxIndex = 0;
        for (index = 0; index < damagePotentials.Length; index++)
        {
            if (damagePotentials[index] > damagePotentials[maxIndex])
                maxIndex = index;
        }

        //Return the unit we have the biggest damage potential against
        PlayerUnit ret = (PlayerUnit)unitsInRange.ElementAt(maxIndex);

        return ret;
    }



    private void OnMouseDown()
    {
        if ((UIManager.instance.currUnit != null && UIManager.instance.currUnit.selected) || GameManager.instance.enemyPhase)
            return;
        StartCoroutine(showAggroRadius());
    }
    
    IEnumerator showAggroRadius()
    {
        MapBehavior.instance.setColor('r');
        MapBehavior.instance.highlightTilesWithin(transform.position, (int)detectRange);

        while (Input.GetMouseButton(0))
            yield return null;

        MapBehavior.instance.deleteHighlightTiles();
    }
}