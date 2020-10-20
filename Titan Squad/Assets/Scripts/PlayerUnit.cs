using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerUnit : Unit
{
    public bool canMove;
    public bool canAttack;
    public bool selected;

    [SerializeField]
    private int movement;
    private float moveSpeed = 5f;
    //[SerializeField]
    public GameObject maskFilter;
    private EnemyUnit targetedEnemy;

    //Keep track of weapons held by the unit
    private Weapon[] weapons;
    public Weapon equippedWeapon;

    public UnitEvent OnPlayerSelected;
    public UnityEvent OnTurnCompleted;

    // Start is called before the first frame update
    protected override void Start()
    {
        movement = 5;
        canMove = false;
        selected = false;
        base.Start();
        hasTurn = true;

        weapons = new Weapon[2];
        weapons[0] = new Weapon("Pistol");
        weapons[1] = new Weapon("Rifle");
        equippedWeapon = weapons[0];
        if (maskFilter)
        {
            maskFilter = Instantiate(maskFilter);
            maskFilter.transform.position = transform.position;
        }
        targetedEnemy = null;
        
    }

    //Trigger to detect when a player is clicked
    void OnMouseDown()
    {
        foreach (Unit u in MapBehavior.instance.getUnitsInRange(transform.position, equippedWeapon.maxRange))
            Debug.Log(u + " is in range");
        //Ensure no other player unit is selected
        foreach (PlayerUnit player in Level.instance.playerUnits)
            if (player.selected)
                return;
        
        //If it's the player phase, then we select the unit
        if (GameManager.instance.playerPhase && hasTurn && !selected)
        {
            selected = true;
            //Right now, all we do is enable them to walk. In the future this will pull open the selection menu
            animator.SetTrigger("Walking");

            OnPlayerSelected?.Invoke(gameObject);
        }
    }

    // Update is called once per frame
    override
    protected void Update()
    {
        base.Update();
        //If it's the enemy's phase, give this unit a turn for when it becomes the player phase
        if (GameManager.instance.enemyPhase)
            hasTurn = true;
        //We always want the character to be moving towards the spot they're supposed to be at, represented by the movePoint
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);

        if (GameManager.instance.playerPhase && hasTurn)
        {
            //If we're allowed to move, on a mouse click we move to that position
            if (canMove)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    move();
                }
            }
            
        }
    }

    override
    public void move()
    {
        //Grab the destination location
        Vector3 destination = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //Construct a path from the character selected to the destination
        CollisionTile[] path = MapBehavior.instance.getPathTo(transform.position, destination, movement);

        //Begin movement along that path
        StartCoroutine(moveAlongPath(path));
    }

    //This coroutine is to move the movePoint along the path for the sprite to follow
    private IEnumerator moveAlongPath(CollisionTile[] path)
    {
        int index = 0;
        //If the path was invalid
        if (path == null)
            yield break;
        
        //Remove movement permission
        canMove = false;

        //Hold onto the start position
        Vector3 start = transform.position;

        while (index < path.Length)
        {
            //We only want to move the movePoint when our character has made it to the point
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

        if (maskFilter)
            maskFilter.transform.position = transform.position;

        //Once we've moved, we stop the moving animation
        turnCompleted();

        //Update the tiles for collision
        MapBehavior.instance.unitMoved(start, transform.position);
        yield return null;
    }

    //Tells the unit to become deselected
    public void deselected()
    {
        animator.SetTrigger("Stopped");
        selected = false;
        canMove = false;
        canAttack = false;
    }

    //Used to select a valid target to attack
    public IEnumerator selectTarget(List<Unit> enemiesInRange)
    {
        Unit target = null;
        while (target == null)
        {
            if (!canAttack)
                yield break;
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mp.z = 0f;
                foreach (Unit u in enemiesInRange)
                {
                    if (Vector3.Distance(mp, u.transform.position) <= .5f)
                    {
                        target = u;
                        Debug.Log("Target found");
                        break;
                    }
                }
            }
            yield return null;
        }
        UIManager.instance.targetChosen(target.gameObject);
        yield break;
    }


    override
    public void attack(Unit enemy)
    {
        StartCoroutine(playAttack(enemy));
    }

    private IEnumerator playAttack(Unit enemy)
    {
        yield return new WaitForSeconds(.5f);
        enemy.hit(equippedWeapon.damage);
        turnCompleted();
        yield break;
    }

    //Used by the UI to tell the unit the player selected a move
    public void moveSelected()
    {
        canMove = true;
    }

    //Used by the UI to tell the unit the player selected an attack
    public void attackSelected()
    {

    }

    private void turnCompleted()
    {
        OnTurnCompleted?.Invoke();
        hasTurn = false;
        selected = false;
        animator.SetTrigger("Stopped");
    }

    override
    public void hit(int damage)
    {

    }
}
