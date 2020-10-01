using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Unit
{
    //Boolean to tell the unit if it has control
    private bool hasControl;
    //Boolean to only try to move while it can still move
    private bool canMove;

    private int movement;

    //Mode affects what actions the enemy will take
    public string mode;
    //Possible modes: Guard, Patrol, Investigate, Attack (+ maybe more)

    // Start is called before the first frame update
    protected override void Start()
    {
        hasControl = true;
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        //If it's the player's phase, give this unit a turn and allow it to move
        if (GameManager.instance.playerPhase)
        {
            hasTurn = true;
            canMove = true;
        }

        //We only want one unit to have control at a time, so unit actions can only occur while the unit has control
        if (hasControl)
        {
            //If it's enemy phase and the unit can move, have it move
            if (GameManager.instance.enemyPhase && canMove)
            {
                move();
            }
        }
    }

    //Each unit will move in a different way depending on it's mode
    override
    public void move()
    {
        canMove = false;
        if(mode.Equals("Patrol"))
            StartCoroutine(moveAlongPatrol());
    }

    //This method gives control to the unit so only one unit has control at once
    //Currently not used, will be used by the Enemy Controller 
    public void giveControl()
    {
        hasControl = true;
    }

    //WIP - Will move a unit along a patrol path
    IEnumerator moveAlongPatrol()
    {
        //Grab the start of our move
        Vector3 start = transform.position;

        animator.SetTrigger("Walking");
        yield return new WaitForSeconds(1f);
        animator.SetTrigger("Stopped");

        //Update tiles for collision
        MapBehavior.instance.unitMoved(start, transform.position);
        hasTurn = false;
    }

    //WIP - Used to have a unit attack an enemy
    override
    public void attack(Unit enemy)
    {

    }
}
