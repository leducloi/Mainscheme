using System.Collections;
using System.Collections.Generic;
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
    private bool canMove;

    private int movement;
    private float moveSpeed = 5f;
    [SerializeField]
    private Rigidbody2D visibleBox;

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

        visibleBox = GetComponentInChildren<Rigidbody2D>();

        if (patrolEnemy)
        {
            numberOfPatternChild = patrolPath.transform.childCount;
            allPathPatterns = patrolPath.transform;
        }
        hasControl = false;
        base.Start();

        gameObject.GetComponent<SpriteRenderer>().enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Light")
            gameObject.GetComponent<SpriteRenderer>().enabled = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Light")
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
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
        if (GameManager.instance.playerPhase)
        {
            hasTurn = true;
            canMove = true;
        }

        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);
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
        if (mode.Equals("Patrol"))
            StartCoroutine(patroling());
        else if (mode.Equals("Guard"))
            StartCoroutine(guarding());
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
    IEnumerator moveAlongPath(CollisionTile[] path)
    {
        if (path == null)
            yield break;
        //Grab the start of our move
        int index = 0;
        Vector3 start = transform.position;
        animator.SetTrigger("Walking");
        while (index < path.Length)
        {
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
        hasTurn = false;
        hasControl = false;
    }


    //WIP - Used to have a unit attack an enemy
    override
        public void attack(Unit enemy)
    {

    }

    override
    public void hit(int damage)
    {
        hpRemaining -= damage;
    }

    private void unitDefeated()
    {
        MapBehavior.instance.getMap().unitDefeated(transform.position, true);
        Destroy(gameObject);
    }

    override
    public bool isHiddenFrom(Unit player)
    {
        return true;
    }
}