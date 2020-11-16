using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class PlayerUnit : Unit
{
    public bool canMove;
    public bool canAttack;
    public bool selectAbility;
    public bool selected;
    public int ultRange;
    public bool bonusMove = false;
    public bool exfilled;

    public int damageDone;
    public int enemiesKilled;
    public int objectivesCompleted;
    public int attacksMissed;
    public int damageTaken;
    public int abilitiesUsed;
    public int ultimatesUsed;

    protected const int ULT_COOLDOWN = 3;
    protected const int ABILITY_COOLDOWN = 1;

    [SerializeField]
    protected int movement;
    protected float moveSpeed = 5f;

    public string[] abilityNames;
    public string[] abilityDescriptions;

    public int actionPoints = 2;

    private bool dragging = false;

    public UnitEvent OnPlayerSelected;
    public UnityEvent OnTurnCompleted;

    //private CollisionTile[] currentPath;
    private CollisionTile lastTile;

    public LineRenderer[] sightlines;
    public Material lineMaterial;

    public bool usingAbility1 = false;
    public bool usingAbility2 = false;
    public bool usingAbility3 = false;

    private bool showingHighlight = false;

    GameObject sightlineHolder;
    public GameObject projectedPosition;
    

    // Start is called before the first frame update
    protected override void Start()
    {
        abilityNames = new string[3];
        abilityDescriptions = new string[3];

        hpTypeFlesh = true;

        movement = 5;
        canMove = false;
        selectAbility = false;
        selected = false;
        base.Start();
        shaderControl.setColor(true);
        hasTurn = true;

        sightlineHolder = new GameObject("Sightline");
        sightlineHolder.transform.SetParent(transform);

        Color c = Color.white;
        c.a = 0.5f;
        projectedPosition.GetComponent<SpriteRenderer>().color = c;
        projectedPosition.GetComponent<Animator>().SetTrigger("Walking");
        projectedPosition.GetComponent<SpriteRenderer>().enabled = false;
    }

    //Trigger to detect when a player is clicked
    public void OnMouseDown()
    {
        //Ensure no other player unit is selected
        foreach (PlayerUnit player in Level.instance.playerUnits)
            if (player.selected)
                return;
        
        //If it's the player phase, then we select the unit
        if (GameManager.instance.playerPhase && hasTurn && !selected)
        {
            StartCoroutine(CameraBehavior.instance.follow(gameObject));

            shaderControl.showOutline();
            setAndLockHighIntensity();

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
        if (!GameManager.instance.playerPhase)
        {
            hasTurn = true;
            actionPoints = 2;
        }
        //We always want the character to be moving towards the spot they're supposed to be at, represented by the movePoint
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);

        if (GameManager.instance.playerPhase && hasTurn)
        {
            //If we're allowed to move, on a mouse click we move to that position
            if (canMove)
            {
                if (!showingHighlight)
                {
                    showingHighlight = true;
                    MapBehavior.instance.setColor('b');
                    MapBehavior.instance.highlightTilesInRange(transform.position, movement, equippedWeapon.minRange, equippedWeapon.maxRange);
                }
                setArrowPath();
                if (Input.GetMouseButtonDown(0))
                {
                    move();
                    PathArrowControl.instance.destroyAllArrows();
                    removeSightlines();
                    projectedPosition.GetComponent<SpriteRenderer>().enabled = false;
                }
            } 
            else if (showingHighlight)
            {
                showingHighlight = false;
                removeSightlines();
                projectedPosition.GetComponent<SpriteRenderer>().enabled = false;
                MapBehavior.instance.deleteHighlightTiles();
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

        takingCover = false;

        //Begin movement along that path
        StartCoroutine(moveAlongPath(path));
        MapBehavior.instance.deleteHighlightTiles();
        showingHighlight = false;
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
                setWalkingPosition(path[index].coordinate);
                movePoint.position = path[index].coordinate;
                index++;
            }
            //So we don't infinite loop, we pause this coroutine at the end of each iteration
            yield return null;
        }
        while (Vector3.Distance(transform.position, movePoint.position) != 0)
            yield return null;

        takeCover();

        setWalkingPosition(new Vector3(transform.position.x, transform.position.y - 1, 0));

        //Wait 1 frame
        yield return new WaitForSeconds(0.1f);

        //Once we've moved, reduce our action points
        useActionPoint(1);

        

        //Update the tiles for collision
        MapBehavior.instance.unitMoved(start, transform.position);
        yield return null;
    }

    //Tells the unit to become deselected
    public void deselected()
    {
        animator.SetTrigger("Stopped");
        PathArrowControl.instance.destroyAllArrows();
        setLowIntensity();
        hideOutline();
        selected = false;
        canMove = false;
        canAttack = false;
    }

    private void setArrowPath()
    {
        CollisionTile newTile = MapBehavior.instance.getTileAtPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        if (newTile != null)
        {
            if (newTile == lastTile)
            {
                return;
            }
        }
        else
            return;

        lastTile = newTile;
        PathArrowControl.instance.destroyAllArrows();
        removeSightlines();
        projectedPosition.GetComponent<SpriteRenderer>().enabled = false;

        Vector3 destination = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CollisionTile[] path = MapBehavior.instance.getPathTo(transform.position, destination, movement);
        if (path != null)
        {
            PathArrowControl.instance.setPathArrow(path);
            drawSightlines(lastTile.coordinate);
            projectedPosition.transform.position = lastTile.coordinate;
            projectedPosition.GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    //Used to select a valid target to attack
    public IEnumerator selectTarget(List<Unit> enemiesInRange)
    {
        drawSightlines(transform.position);
        Unit target = null;
        while (target == null)
        {
            if (!canAttack)
            {
                removeSightlines();
                yield break;
            }
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mp.z = 0f;
                foreach (Unit u in enemiesInRange)
                {
                    if (Vector3.Distance(mp, u.transform.position) <= .5f)
                    {
                        target = u;
                        break;
                    }
                }
            }
            yield return null;
        }
        UIManager.instance.targetChosen(target.gameObject);
        removeSightlines();
        yield break;
    }


    override
    public void attack(Unit enemy)
    {
        StartCoroutine(playAttack(enemy));
    }

    private IEnumerator playAttack(Unit enemy)
    {
        float addX = (enemy.transform.position.x - transform.position.x) / 2;
        float addY = (enemy.transform.position.y - transform.position.y) / 2;

        Vector3 moveTo = transform.position;
        moveTo.x += addX;
        moveTo.y += addY;

        yield return StartCoroutine(CameraBehavior.instance.panCameraTo(moveTo, 1));

        Vector3 originalPos = transform.position;
        if (takingCover && !isFlankedBy(enemy))
        {
            CollisionTile stepInto = MapBehavior.instance.stepOutInto(transform.position, enemy, equippedWeapon.maxRange, equippedWeapon.minRange);
            if (stepInto != null)
            {
                Vector3 stepPos = new Vector3((transform.position.x - stepInto.coordinate.x) / 2f, (transform.position.y - stepInto.coordinate.y) / 2f, 0);
                movePoint.transform.position -= stepPos;
                while (transform.position != movePoint.transform.position)
                    yield return null;
            }
        }

        if (CombatCalculator.instance.doesHit)
        {
            damageDone += CombatCalculator.instance.damageDone;
            if (enemy.hpRemaining <= CombatCalculator.instance.damageDone)
                enemiesKilled++;
            enemy.hit(CombatCalculator.instance.damageDone);
        }
        else
        {
            UIManager.instance.attackMissed(enemy.transform.position);
            attacksMissed++;
        }

        while (enemy.healthBar.movingBar)
            yield return null;

        if (takingCover)
        {
            movePoint.transform.position = originalPos;
            while (transform.position != movePoint.transform.position)
                yield return null;
        }

        yield return new WaitForSeconds(.1f);

        useActionPoint(1);
        canAttack = false;
        yield break;
    }

    //Used by the UI to tell the unit the player selected a move
    public void moveSelected()
    {
        canMove = true;
    }

    public void useActionPoint(int cost)
    {
        actionPoints -= cost;
        if (actionPoints <= 0)
            turnCompleted();
        else
        {
            OnPlayerSelected?.Invoke(gameObject);
        }
    }

    private void turnCompleted()
    {
        OnTurnCompleted?.Invoke();
        //actionPoints = 2;
        setLowIntensity();
        hideOutline();
        hasTurn = false;
        selected = false;
        animator.SetTrigger("Stopped");
        MapBehavior.instance.deleteHighlightTiles();
        PathArrowControl.instance.destroyAllArrows();
    }

    override
    public void hit(int damage)
    {
        StartCoroutine(playHit(damage));
    }

    IEnumerator playHit(int damage)
    {

        //play hit animation

        if (damage >= 0)
        {
            healthBar.takeDamage(damage);
            damageTaken += damage;
            StartCoroutine(CameraBehavior.instance.cameraShake());
        }
        else
        {
            healthBar.recieveHealing(-damage);
        }


        yield return null;

        while (healthBar.movingBar)
            yield return null;

        hpRemaining = healthBar.health;
        shieldRemaining = healthBar.shields;

        if (hpRemaining > hpMax)
            hpRemaining = hpMax;
        //check if death

        if (hpRemaining <= 0)
            Level.instance.levelFailed();
    }

    private void OnMouseDrag()
    {
        if (!Level.instance.donePlanning && !dragging)
        {
            StartCoroutine(planningDrag());
        }
    }

    private IEnumerator planningDrag()
    {
        if (dragging)
            yield break;

        dragging = true;
        animator.SetTrigger("Walking");

        Vector3 temp = transform.position;

        while (Input.GetMouseButton(0))
        {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            movePoint.transform.position = transform.position;
            yield return null;
        }

        Vector3 endPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        endPoint.z = 0;
        foreach (Vector3 position in Level.instance.startPositions)
        {
            if (Mathf.Abs(Vector3.Distance(endPoint, position)) <= 0.7f)
            {
                Unit switchWith = Level.instance.getUnitAtLoc(position);
                if (switchWith != null)
                {
                    switchWith.movePoint.transform.position = temp;
                    switchWith.transform.position = temp;

                    transform.position = position;
                    movePoint.transform.position = position;
                    animator.SetTrigger("Stopped");
                    dragging = false;
                    yield break;
                }
                else
                {
                    transform.position = position;
                    movePoint.position = position;
                    animator.SetTrigger("Stopped");
                    dragging = false;
                    yield break;
                }
            }
        }

        transform.position = temp;
        movePoint.position = temp;
        animator.SetTrigger("Stopped");


        dragging = false;
    }

    private void removeSightlines()
    {
        Destroy(sightlineHolder);
        sightlineHolder = new GameObject("Sightline Holder");
        sightlineHolder.transform.SetParent(transform);
    }
    
    private void setUpSightline(int numLines, Vector3 position)
    {
        Color sightColor = Color.white;
        sightColor.a = 0.8f;

        position.y += 0.4f;

        for (int x = 0; x < numLines; x++)
        {
            GameObject sightline = new GameObject("New Sightline");
            sightline.transform.SetParent(sightlineHolder.transform);

            sightline.AddComponent<LineRenderer>();
        }

        sightlines = sightlineHolder.GetComponentsInChildren<LineRenderer>();

        for (int lineNum = 0; lineNum < numLines; lineNum++)
        {
            sightlines[lineNum].startColor = sightColor;
            sightlines[lineNum].endColor = sightColor;
            sightlines[lineNum].startWidth = .05f;
            sightlines[lineNum].endWidth = .05f;

            sightlines[lineNum].loop = false;
            sightlines[lineNum].material = lineMaterial;

            sightlines[lineNum].positionCount = 2;
            sightlines[lineNum].SetPosition(0, position);

            sightlines[lineNum].sortingOrder = -1;
        }
    }

    private void drawSightlines(Vector3 position)
    {
        List<Unit> enemiesInRange = MapBehavior.instance.getUnitsInRange(position, equippedWeapon.maxRange, equippedWeapon.minRange);

        setUpSightline(enemiesInRange.Count, position);

        int index = 0;
        foreach (Unit enemy in enemiesInRange)
        {
            sightlines[index].SetPosition(1, enemy.transform.position + new Vector3(0, 0.5f, 0));
            index++;
        }
    }

    public abstract void ability1();
    public abstract void ability2();
    public abstract void ability3();

}
