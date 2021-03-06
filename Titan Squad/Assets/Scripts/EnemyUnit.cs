﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyUnit : Unit
{
    //Set of patrol path patterns
    [SerializeField]
    private GameObject patrolPath = null;

    //Set fasle if enemy does not move
    [SerializeField]
    public bool patrolEnemy;

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

    protected int movement = 5;
    private float moveSpeed = 5f;
    //detectRange, rangeEnemy, weaponRange. Temporarily use for testing purpose only, remove when they are not needed.
    public float detectRange = 10f;
    private GameObject detectedPlayerObject;
    private Vector3 originalPos;

    public bool takesCover;

    //Mode affects what actions the enemy will take
    public string mode;
    //Possible modes: Guard, Patrol, Investigate, Attack (+ maybe more)

    // Start is called before the first frame update
    protected override void Start()
    {
        if (patrolPath == null)
            mode = "Guard";

        if (mode.Equals("Patrol"))
            patrolEnemy = true;
        else
            patrolEnemy = false;

        if (patrolEnemy)
        {
            originalPos = transform.position;
            numberOfPatternChild = patrolPath.transform.childCount;
            allPathPatterns = patrolPath.transform;
        }
        hasControl = false;

        //weapons[0] = new Weapon("Krimbar Power Sword");
        //weapons[1] = new Weapon("--");

        base.Start();
        shaderControl.setColor(false);

        isEnemy = true;
        
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

            yield return new WaitForSeconds(0.5f);
            actionPoints--;
        }
        setFinishMove(transform.position);
    }

    private void detectPlayerInRange(Vector3 currentPosition)
    {
        GameObject playerObject = MapBehavior.instance.getClosestPlayerObject(currentPosition);
        Vector3 playerPosition = playerObject.transform.position;
        float actualDistance = Mathf.Abs(playerPosition.x - currentPosition.x) + Mathf.Abs(playerPosition.y - currentPosition.y);
        if (actualDistance <= detectRange && !playerObject.GetComponent<PlayerUnit>().isCloaked)
        {
            mode = "Chase";
            detectRange = int.MaxValue;
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
        detectPlayerInRange(transform.position);
        CollisionTile[] path = null;
        if (!takesCover || MapBehavior.instance.hasLineTo(currentPosition, detectedPlayerObject.transform.position, (int)detectRange, 0))
        {
            path = MapBehavior.instance.getPathTo(currentPosition, detectedPlayerObject.transform.position, int.MaxValue, true);
        }

        if (takesCover)
        {
            CollisionTile coverDestination = scanCoverTile(currentPosition, detectedPlayerObject.transform.position);
            if (coverDestination != null)
            {
                Debug.Log("Found cover at..." + coverDestination.coordinate);
                //If cover is within the weapon range, move to cover
                if (isInWeaponRange(coverDestination.coordinate, detectedPlayerObject.transform.position))
                {
                    Debug.Log("Moving to " + coverDestination.coordinate);
                    path = MapBehavior.instance.getPathTo(currentPosition, coverDestination.coordinate, int.MaxValue, true);
                }
                else
                {
                    //If cover is not within the weapon range, keep moving toward player
                    path = MapBehavior.instance.getPathTo(currentPosition, detectedPlayerObject.transform.position, int.MaxValue, true);
                }
            }
            else
            {
                //If not cover is found, walk toward player
                path = MapBehavior.instance.getPathTo(currentPosition, detectedPlayerObject.transform.position, int.MaxValue, true);
            }
        }

        yield return StartCoroutine(moveAlongPath(path, true));
        MapBehavior.instance.unitMoved(currentPosition, transform.position);
        yield return null;
    }

    private bool isInWeaponRange(Vector3 enemyPos, Vector3 playerPos)
    {
        //Check if 2 points are within the weapon range
        bool inRange = false;
        float distance = Mathf.Abs(playerPos.x - enemyPos.x) + Mathf.Abs(playerPos.y - enemyPos.y);
        if (distance <= equippedWeapon.maxRange)
        {
            inRange = true;
        }
        return inRange;
    }

    //private CollisionTile scanCoverTile(Vector3 enemyPos, Vector3 playerPos)
    //{
    //    int xDiff = (int)(playerPos.x - enemyPos.x);
    //    int yDiff = (int)(playerPos.y - enemyPos.y);
    //    bool positiveX = xDiff > 0;
    //    bool positiveY = yDiff > 0;
    //    int xAbs = Mathf.Abs(xDiff);
    //    int yAbs = Mathf.Abs(yDiff);
    //    int actualI = 0, actualJ = 0;
    //    bool thereIsCover = false;
    //    CollisionTile currentScanTile = MapBehavior.instance.getTileAtPos(enemyPos);
    //    if (yAbs >= xAbs)
    //    {
    //        //loop through rectangle with enemy and player are at opposite corner
    //        for (int i = 0; i < xAbs; i++)
    //        {
    //            for (int j = 0; j < yAbs; j++)
    //            {
    //                if (positiveY)
    //                    actualJ++;
    //                else
    //                    actualJ--;
    //                CollisionTile checkingTile = MapBehavior.instance.getTileAtPos(new Vector3(enemyPos.x + actualI, enemyPos.y + actualJ));
    //                //when impassible tile is used to take cover
    //                if (!checkingTile.passable)
    //                {
    //                    thereIsCover = true;
    //                    break;
    //                }
    //                //When the previous tile does not have enemy, use it as the tile to move to
    //                if(!checkingTile.hasEnemy)
    //                    currentScanTile = checkingTile;
    //            }
    //            if (thereIsCover)
    //                break;
    //            if (positiveX)
    //                actualI++;
    //            else
    //                actualI--;
    //        }
    //    }
    //    else
    //    {
    //        for (int i = 0; i < yAbs; i++)
    //        {
    //            for (int j = 0; j < xAbs; j++)
    //            {
    //                if (positiveX)
    //                    actualI++;
    //                else
    //                    actualI--;
    //                CollisionTile checkingTile = MapBehavior.instance.getTileAtPos(new Vector3(enemyPos.x + actualI, enemyPos.y + actualJ));
    //                //when impassible tile is used to take cover
    //                if (checkingTile == null)
    //                    continue;
    //                if (!checkingTile.passable)
    //                {
    //                    thereIsCover = true;
    //                    break;
    //                }
    //                //When the previous tile does not have enemy, use it as the tile to move to
    //                if (!checkingTile.hasEnemy)
    //                    currentScanTile = checkingTile;
    //            }
    //            if (thereIsCover)
    //                break;
    //            if (positiveY)
    //                actualJ++;
    //            else
    //                actualJ--;
    //        }
    //    }
    //    if (thereIsCover) {
    //        return currentScanTile;
    //    }   
    //    else
    //        return null;
    //}


    private CollisionTile scanCoverTile(Vector3 enemyPos, Vector3 playerPos)
    {
        int xDiff = (int)(playerPos.x - enemyPos.x);
        int yDiff = (int)(playerPos.y - enemyPos.y);
        bool positiveX = xDiff > 0;
        bool positiveY = yDiff > 0;
        int xAbs = Mathf.Abs(xDiff);
        int yAbs = Mathf.Abs(yDiff);
        int actualI = 0, actualJ = 0;
        bool thereIsCover = false;
        CollisionTile currentScanTile = MapBehavior.instance.getTileAtPos(enemyPos);
        if (yAbs >= xAbs)
        {
            //loop through rectangle with enemy and player are at opposite corner
            for (int i = 0; i < xAbs; i++)
            {
                for (int j = 0; j < yAbs; j++)
                {
                    if (positiveY)
                        actualJ++;
                    else
                        actualJ--;
                    CollisionTile checkingTile = MapBehavior.instance.getTileAtPos(new Vector3(enemyPos.x + actualI, enemyPos.y + actualJ));
                    //when impassible tile is used to take cover
                    if (checkingTile.passable && (checkingTile.lowCover || checkingTile.highCover) && !checkingTile.passableNS)
                    {
                        if (positiveY)
                        {
                            currentScanTile = checkingTile;
                            thereIsCover = true;
                            break;
                        }
                        else if (!positiveY)
                        {
                            CollisionTile northTile = MapBehavior.instance.getTileAtPos(new Vector3(enemyPos.x + actualJ, enemyPos.y + actualI + 1f));
                            currentScanTile = northTile;
                            thereIsCover = true;
                            break;
                        }
                    }
                    //When the previous tile does not have enemy, use it as the tile to move to
                    //if(!checkingTile.hasEnemy)
                    //    currentScanTile = checkingTile;
                }
                actualJ = 0;
                if (thereIsCover)
                    break;
                if (positiveX)
                    actualI++;
                else
                    actualI--;
            }
        }
        else
        {
            for (int i = 0; i < yAbs; i++)
            {
                for (int j = 0; j < xAbs; j++)
                {
                    if (positiveX)
                        actualJ++;
                    else
                        actualJ--;
                    CollisionTile checkingTile = MapBehavior.instance.getTileAtPos(new Vector3(enemyPos.x + actualJ, enemyPos.y + actualI));
                    //when impassible tile is used to take cover
                    Debug.Log(checkingTile.toString());
                    if (checkingTile == null)
                        continue;
                    if (checkingTile.passable && (checkingTile.lowCover || checkingTile.highCover) && !checkingTile.passableEW)
                    {
                        if (!positiveX)
                        {
                            CollisionTile eastTile = MapBehavior.instance.getTileAtPos(new Vector3(enemyPos.x + actualJ + 1f, enemyPos.y + actualI));
                            currentScanTile = eastTile;
                            thereIsCover = true;
                            break;
                        }
                        else if (positiveX)
                        {
                            currentScanTile = checkingTile;
                            thereIsCover = true;
                            break;
                        }
                    }

                    //When the previous tile does not have enemy, use it as the tile to move to
                    //if (!checkingTile.hasEnemy)
                    //    currentScanTile = checkingTile;
                }
                actualJ = 0;
                if (thereIsCover)
                    break;
                if (positiveY)
                    actualI++;
                else
                    actualI--;
            }
        }
        if (thereIsCover)
        {
            return currentScanTile;
        }
        else
            return null;
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
            Vector3 tempTargetPosition = allPathPatterns.GetChild(patternIndex).position + originalPos;
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
        //animator.SetTrigger("Walking");

        int movementLeft = movement;
        int maxIndex = 1;
        foreach(CollisionTile tile in path)
        {
            if (tile.coordinate == transform.position)
                continue;

            movementLeft -= tile.tileCost;
            if (movementLeft < 0)
                break;

            maxIndex++;
        }

        StartCoroutine(playFootsteps());
        while (index < maxIndex)
        {
            if (Vector3.Distance(transform.position, movePoint.position) == 0)
            {
                setWalkingPosition(path[index].coordinate);
                movePoint.position = path[index].coordinate;
                index++;
                if (index < path.Length && path[index].hasEnemy)
                {
                    if (index + 1 == maxIndex)
                        break;
                }
            }
            //So we don't infinite loop, we pause this coroutine at the end of each iteration
            yield return null;
        }

        while (Vector3.Distance(transform.position, movePoint.position) != 0)
            yield return null;

        moving = false;

        takeCover();
        
        setWalkingPosition(new Vector3(transform.position.x, transform.position.y - 1, 0));
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
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            animator.SetTrigger("Stopped");

        hpTypeFlesh = true;
    }


    //Used to have a unit attack an enemy
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
        playInjured();
    }

    IEnumerator playHit(int damage)
    {
        //play hit animation


        playHitFlash();
        healthBar.takeDamage(damage);

        StartCoroutine(CameraBehavior.instance.cameraShake());

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
        float addX = (target.transform.position.x - transform.position.x) / 2;
        float addY = (target.transform.position.y - transform.position.y) / 2;

        Vector3 moveTo = transform.position;
        moveTo.x += addX;
        moveTo.y += addY;

        yield return StartCoroutine(CameraBehavior.instance.panCameraTo(moveTo, 1));

        setAttackAnimation(target.transform.position);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length * 0.5f);

        if (equippedWeapon.maxRange > 1)
            playShoot();
        else
            playSwing();
        

        //Calculate results
        CombatCalculator.instance.calculate(this, target);

        //Do damage
        if (CombatCalculator.instance.doesHit)
            target.hit(CombatCalculator.instance.damageDone);
        else
            UIManager.instance.attackMissed(target.transform.position);


        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length * 0.5f);
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            animator.SetTrigger("Stopped");

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
        if ((UIManager.instance.currUnit != null && UIManager.instance.currUnit.selected) || GameManager.instance.enemyPhase || CameraBehavior.instance.pauseMovement)
            return;
        StartCoroutine(showAggroRadius());
    }
    
    IEnumerator showAggroRadius()
    {
        if (mode != "Chase")
        {
            MapBehavior.instance.setColor('r');
            MapBehavior.instance.highlightTilesWithin(transform.position, (int)detectRange);
        }
        else
        {
            MapBehavior.instance.setColor('b');
            MapBehavior.instance.highlightTilesInRange(transform.position, movement, equippedWeapon.minRange, equippedWeapon.maxRange, true);
        }

        while (Input.GetMouseButton(0))
            yield return null;

        MapBehavior.instance.deleteHighlightTiles();
    }
}