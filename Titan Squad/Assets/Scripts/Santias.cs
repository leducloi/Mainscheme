using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Santias : PlayerUnit
{
    const int CLOAK_DURATION = 1;
    const int GRAPPLE_DISTANCE = 5;

    [SerializeField]
    private GameObject teleportMask = null;
    [SerializeField]
    private GameObject hook = null;

    private float teleportSpeed = 6f;

    // Start is called before the first frame update
    override
    protected void Start()
    {
        weapons[0] = new Weapon("Energy Blade");
        weapons[1] = new Weapon("Grappling Hook");

        base.Start();

        abilityNames[0] = "Grapple Jump";
        abilityNames[1] = "Cloak";
        abilityNames[2] = "Full Force";

        abilityDescriptions[0] = "Santias fires out his Grappling Hook to an obstacle within " + GRAPPLE_DISTANCE + " tiles and lands in a tile adjacent to it.";
        abilityDescriptions[1] = "Santias cloaks himself for " + CLOAK_DURATION + " turn, rendering him untargetable by enemies.";
        abilityDescriptions[2] = "Santias teleports to and attacks a nearby enemy with his Energy Blade. If this attack kills the enemy, Santias can teleport to a nearby location and make another attack.";

        hook.GetComponent<SpriteRenderer>().enabled = false;
        hook.transform.SetParent(MapBehavior.instance.transform);

        hpMax = 10;
        hpRemaining = 10;
        shieldMax = 5;
        shieldRemaining = 5;

        combatTraining = 20;
        evasiveTactics = 0;
        bionicEnhancement = 6;
        luck = 2;
        criticalTargeting = 3;
        advancedShielding = 2;
    }

    // Update is called once per frame
    override
    protected void Update()
    {
        base.Update();
    }

    //Grapple Jump
    override
    public void ability1()
    {
        StartCoroutine(grappleJump());
    }

    //Cloak
    override
    public void ability2()
    {
        StartCoroutine(cloak());
    }

    //Full Force
    override
    public void ability3()
    {
        StartCoroutine(fullForce());
    }

    IEnumerator grappleJump()
    {
        usingAbility1 = true;

        Vector3 startPosition = transform.position;

        List<CollisionTile> validTiles = MapBehavior.instance.getJumpableTiles(transform.position, GRAPPLE_DISTANCE);
        MapBehavior.instance.hightlightCustomTiles(validTiles, 'b');

        CollisionTile validSelection = null;
        while (validSelection == null)
        {
            yield return null;

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                usingAbility1 = false;
                MapBehavior.instance.deleteHighlightTiles();
                UIManager.instance.abilityMenu();
                yield break;
            }
            if (Input.GetMouseButtonDown(0))
            {
                CollisionTile selection = MapBehavior.instance.getTileAtPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                foreach (CollisionTile tile in validTiles)
                {
                    if (selection == tile)
                    {
                        validSelection = selection;
                        break;
                    }
                }
            }
        }

        
        CollisionTile pullTo = null;
        foreach (CollisionTile tile in MapBehavior.instance.findNeighborTiles(validSelection))
        {
            if (!tile.passable)
            {
                pullTo = tile;
                break;
            }
        }

        MapBehavior.instance.deleteHighlightTiles();
        hook.transform.position = transform.position;

        //Increase movement speed to give illusion of quick pull
        moveSpeed = 15f;

        Vector3 start = Camera.main.WorldToScreenPoint(transform.position);
        Vector3 destination = Camera.main.WorldToScreenPoint(validSelection.coordinate);
        float angle = Mathf.Atan2(destination.y - start.y, destination.x - start.x) * Mathf.Rad2Deg;

        hook.transform.Rotate(new Vector3(0, 0, angle - 90));
        LineRenderer rope = hook.GetComponent<LineRenderer>();
        rope.SetPosition(0, transform.position);

        hook.GetComponent<SpriteRenderer>().enabled = true;
        hook.GetComponent<LineRenderer>().enabled = true;

        while (hook.transform.position != pullTo.coordinate)
        {
            hook.transform.position = Vector3.MoveTowards(hook.transform.position, pullTo.coordinate, 35f * Time.deltaTime);
            rope.SetPosition(1, hook.transform.position);
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        movePoint.transform.position = pullTo.coordinate;

        //Allow unit time to "jump" to the tile
        while (transform.position != movePoint.transform.position)
        {
            rope.SetPosition(0, transform.position);
            yield return null;
        }

        hook.GetComponent<LineRenderer>().enabled = false;
        hook.GetComponent<SpriteRenderer>().enabled = false;

        //Reset movement speed
        moveSpeed = 5f;

        movePoint.transform.position = validSelection.coordinate;

        while (transform.position != movePoint.transform.position)
            yield return null;


        MapBehavior.instance.unitMoved(startPosition, transform.position);

        abilitiesUsed++;
        useActionPoint(1);

        int endTurn = GameManager.instance.turnCount + 1;
        while (endTurn != GameManager.instance.turnCount)
            yield return null;

        usingAbility1 = false;
    }

    IEnumerator cloak()
    {
        usingAbility2 = true;

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();

        Color prevColor = sprite.material.GetColor("_OverlayColor");
        float prevAmount = sprite.material.GetFloat("_OverlayAmount");

        Color spriteColor;
        ColorUtility.TryParseHtmlString("#00FFE0", out spriteColor);
        if (cbDrugs)
            spriteColor = Color.red;

        Vector2 shrinkTo = new Vector2(1f, 1f);
        while (shrinkTo.x > 0)
        {
            shrinkTo.x -= .1f;
            shrinkTo.x = Mathf.Clamp(shrinkTo.x, 0, 1);
            transform.localScale = shrinkTo;
            yield return null;
        }

        //Hide sprite
        sprite.color = new Color(1f, 1f, 1f, 0f);
        shrinkTo.x = 1;
        transform.localScale = shrinkTo;

        yield return new WaitForSeconds(0.35f);

        //Set Color
        sprite.material.SetColor("_OverlayColor", spriteColor);
        sprite.material.SetFloat("_OverlayAmount", 0.3f);

        Color fadeIn = new Color(1f, 1f, 1f, 0f);
        float fadeAmount = 0f;
        while (fadeAmount < 150)
        {
            fadeAmount += 3f;
            fadeIn.a = fadeAmount / 255f;
            sprite.color = fadeIn;
            yield return new WaitForSecondsRealtime(1f/60f);
        }

        abilitiesUsed++;
        useActionPoint(1);
        isCloaked = true;

        int endTurn = CLOAK_DURATION + GameManager.instance.turnCount;
        while (GameManager.instance.turnCount != endTurn)
            yield return null;
        while (!GameManager.instance.playerPhase)
            yield return null;
        

        if (cbDrugs)
        {
            sprite.material.SetColor("_OverlayColor", Color.red);
            sprite.material.SetFloat("_OverlayAmount", 0.25f);
        }

        float overlayAmount = 0.3f;
        while (fadeAmount < 255)
        {
            if (overlayAmount > 0 && !cbDrugs)
            {
                overlayAmount -= 0.1f;
                sprite.material.SetFloat("_OverlayAmount", overlayAmount);
            }
            fadeAmount += .5f;
            fadeIn.a = fadeAmount / 255f;
            fadeIn.a = Mathf.Clamp(fadeIn.a, 0, 1);
            sprite.color = fadeIn;
            yield return new WaitForSecondsRealtime(1f / 60f);
        }

        isCloaked = false;
        usingAbility2 = false;
    }

    IEnumerator fullForce()
    {
        usingAbility3 = true;

        List<Unit> enemiesInRange = null;
        CollisionTile location = MapBehavior.instance.getTileAtPos(transform.position);
        List<CollisionTile> selectableTiles = new List<CollisionTile>();
        List<CollisionTile> allTiles = new List<CollisionTile>();
        allTiles = MapBehavior.instance.getTilesWithin(location, movement, allTiles);

        foreach (CollisionTile tile in allTiles)
        {
            if (tile.isWalkable() && !tile.hasPlayer)
                selectableTiles.Add(tile);
        }

        MapBehavior.instance.hightlightCustomTiles(selectableTiles, 'b');

        bool hasMove = true;
        bool firstSelection = true;
        while (hasMove)
        {
            yield return null;

            if (Input.GetKeyDown(KeyCode.Escape) && firstSelection)
            {
                usingAbility3 = false;
                MapBehavior.instance.deleteHighlightTiles();
                UIManager.instance.abilityMenu();
                yield break;
            }

            //Teleport and Attack
            if (Input.GetMouseButtonDown(0))
            {
                //If the click position is invalid, skip
                CollisionTile clicked = MapBehavior.instance.getTileAtPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                if (clicked == null || !selectableTiles.Contains(clicked))
                    continue;
                

                //Click position was valid, so this is no longer our first selection
                firstSelection = false;
                
                //Clear tiles
                MapBehavior.instance.deleteHighlightTiles();

                //Teleport
                MapBehavior.instance.unitMoved(transform.position, clicked.coordinate);

                Vector3 targetLoc = new Vector3(0, 0.5f, 0);


                while (teleportMask.transform.localPosition != targetLoc)
                {
                    teleportMask.transform.localPosition = Vector3.MoveTowards(teleportMask.transform.localPosition, targetLoc, teleportSpeed * Time.deltaTime);
                    yield return null;
                }

                movePoint.transform.position = clicked.coordinate;
                transform.position = clicked.coordinate;
                StartCoroutine(CameraBehavior.instance.follow(gameObject));

                targetLoc = new Vector3(0, 2, 0);
                while (teleportMask.transform.localPosition != targetLoc)
                {
                    teleportMask.transform.localPosition = Vector3.MoveTowards(teleportMask.transform.localPosition, targetLoc, teleportSpeed * Time.deltaTime);
                    yield return null;
                }

                List<CollisionTile> tilesToHighlight = new List<CollisionTile>();

                enemiesInRange = MapBehavior.instance.getUnitsInRange(transform.position, 1, 1);

                //If there are no enemies once we teleport, end the ability
                if (enemiesInRange.Count == 0)
                {
                    hasMove = false;
                    continue;
                }

                //If there are enemies, highlight them
                foreach (Unit u in enemiesInRange)
                {
                    if (u == null)
                        continue;

                    CollisionTile tile = MapBehavior.instance.getTileAtPos(u.transform.position);
                    tilesToHighlight.Add(tile);
                    u.showOutline();
                }
                MapBehavior.instance.hightlightCustomTiles(tilesToHighlight, 'r');

                //Make valid attack selection
                Unit target = null;
                while (target == null)
                {
                    yield return null;

                    if (Input.GetMouseButtonDown(0))
                    {
                        clicked = MapBehavior.instance.getTileAtPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                        if (!tilesToHighlight.Contains(clicked))
                            continue;

                        target = Level.instance.getUnitAtLoc(clicked.coordinate);
                        if (target == null)
                            continue;

                        //CombatCalculator.instance.doesHit = true;
                        //CombatCalculator.instance.damageDone = equippedWeapon.damage;
                        target.hit(equippedWeapon.damage);
                        damageDone += equippedWeapon.damage;
                        foreach (Unit u in enemiesInRange)
                        {
                            if (u != null)
                                u.hideOutline();
                        }
                        MapBehavior.instance.deleteHighlightTiles();

                        while (target.healthBar.movingBar)
                            yield return null;

                        //If unit was killed, we get another move
                        if (target.hpRemaining <= 0)
                        {
                            yield return null;
                            enemiesKilled++;

                            selectableTiles.Clear();
                            allTiles.Clear();
                            location = MapBehavior.instance.getTileAtPos(transform.position);
                            allTiles = MapBehavior.instance.getTilesWithin(location, movement, allTiles);

                            foreach (CollisionTile tile in allTiles)
                            {
                                if (tile.isWalkable() && !tile.hasPlayer)
                                    selectableTiles.Add(tile);
                            }

                            MapBehavior.instance.hightlightCustomTiles(selectableTiles, 'b');
                            break;
                        }
                        else
                        {
                            hasMove = false;
                        }
                        
                    }
                }
            }
        }


        abilitiesUsed++;
        ultimatesUsed++;
        useActionPoint(2);

        int endTurn = GameManager.instance.turnCount + ULT_COOLDOWN;
        while (endTurn != GameManager.instance.turnCount)
            yield return null;

        usingAbility3 = false;
    }
}
