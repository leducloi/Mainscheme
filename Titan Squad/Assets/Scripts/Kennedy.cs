using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kennedy : PlayerUnit
{
    private const int DODGE_AMOUNT = 50;
    private const int DODGE_DURATION = 1;

    [SerializeField]
    public GameObject depositSpot = null;
    [SerializeField]
    private GameObject teleportMask = null;
    [SerializeField]
    private GameObject blur1 = null;
    [SerializeField]
    private GameObject blur2 = null;
    [SerializeField]
    private GameObject BFGBeam = null;

    private bool depositSpotDown = false;

    private float teleportSpeed = 6f;

    // Start is called before the first frame update
    override
    protected void Start()
    {
        weapons[0] = new Weapon("Railgun");
        weapons[1] = new Weapon("Cloak and Dagger");

        base.Start();

        blur1.GetComponent<SpriteRenderer>().enabled = false;
        blur2.GetComponent<SpriteRenderer>().enabled = false;
        BFGBeam.GetComponent<SpriteRenderer>().enabled = false;

        ultRange = 1000;

        abilityNames[0] = "Sixth Sense";
        abilityNames[1] = "Escape Clause";
        abilityNames[2] = "BFG";

        abilityDescriptions[0] = "Kennedy focuses her reflexes for " + DODGE_DURATION + " turn, increasing her dodge chance by " + DODGE_AMOUNT + "%";
        abilityDescriptions[1] = "Kennedy places down a deposit location. She can later either move this location or teleport to it.";
        abilityDescriptions[2] = "Kennedy fires a charged shot from her Railgun, hitting all targets in a 3-tile wide line.";

        //Set durability
        hpMax = 10;
        hpRemaining = 10;
        shieldMax = 3;
        shieldRemaining = 3;
    }

    // Update is called once per frame
    override
    protected void Update()
    {
        base.Update();
    }

    //Sixth Sense
    override
    public void ability1()
    {
        StartCoroutine(sixthSense());
    }

    //Escape Clause
    override
    public void ability2()
    {
        if (!depositSpotDown)
        {
            placeDepositSpot();
        }
        else
        {
            UIManager.instance.escapeClauseSelect();
        }
    }

    public void placeDepositSpot()
    {
        StartCoroutine(placeDeposit());
    }

    public void teleportToSpot()
    {
        StartCoroutine(teleport());
    }

    //BFG
    override
    public void ability3()
    {
        StartCoroutine(bfg());
    }

    IEnumerator blurAnimation()
    {
        Vector3 leftPos = new Vector3(-.5f, 0, 0);
        Vector3 rightPos = new Vector3(.5f, 0, 0);

        blur1.GetComponent<SpriteRenderer>().enabled = true;
        blur2.GetComponent<SpriteRenderer>().enabled = true;
        blur1.GetComponent<Animator>().SetTrigger("Walking");
        blur2.GetComponent<Animator>().SetTrigger("Walking");

        while (usingAbility1)
        {
            while (blur1.transform.localPosition != leftPos && blur2.transform.localPosition != rightPos && usingAbility1)
            {
                float blurMove = 2 * (rightPos.x - blur2.transform.localPosition.x) + 0.2f;
                blur1.transform.localPosition = Vector3.MoveTowards(blur1.transform.localPosition, leftPos, blurMove * Time.deltaTime);
                blur2.transform.localPosition = Vector3.MoveTowards(blur2.transform.localPosition, rightPos, blurMove * Time.deltaTime);
                
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                {
                    blur1.GetComponent<Animator>().SetTrigger("Stopped");
                    blur2.GetComponent<Animator>().SetTrigger("Stopped");
                }
                if (hasTurn)
                {
                    blur1.GetComponent<SpriteRenderer>().material.SetFloat("_GrayscaleAmount", 1f);
                    blur2.GetComponent<SpriteRenderer>().material.SetFloat("_GrayscaleAmount", 1f);
                }
                else
                {
                    blur1.GetComponent<SpriteRenderer>().material.SetFloat("_GrayscaleAmount", 0f);
                    blur2.GetComponent<SpriteRenderer>().material.SetFloat("_GrayscaleAmount", 0f);
                }
                yield return null;
            }
            while (blur1.transform.localPosition != rightPos && blur2.transform.localPosition != leftPos && usingAbility1)
            {
                float blurMove = 2 * (rightPos.x - blur1.transform.localPosition.x) + 0.2f;
                blur1.transform.localPosition = Vector3.MoveTowards(blur1.transform.localPosition, rightPos, blurMove * Time.deltaTime);
                blur2.transform.localPosition = Vector3.MoveTowards(blur2.transform.localPosition, leftPos, blurMove * Time.deltaTime);
                
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                {
                    blur1.GetComponent<Animator>().SetTrigger("Stopped");
                    blur2.GetComponent<Animator>().SetTrigger("Stopped");
                }
                if (hasTurn)
                {
                    blur1.GetComponent<SpriteRenderer>().material.SetFloat("_GrayscaleAmount", 1f);
                    blur2.GetComponent<SpriteRenderer>().material.SetFloat("_GrayscaleAmount", 1f);
                }
                else
                {
                    blur1.GetComponent<SpriteRenderer>().material.SetFloat("_GrayscaleAmount", 0f);
                    blur2.GetComponent<SpriteRenderer>().material.SetFloat("_GrayscaleAmount", 0f);
                }
                yield return null;
            }
            yield return null;
        }
        Vector3 origin = new Vector3(0, 0, 0);
        while (blur1.transform.localPosition != origin && blur2.transform.localPosition != origin)
        {
            blur1.transform.localPosition = Vector3.MoveTowards(blur1.transform.localPosition, origin, 1.5f * Time.deltaTime);
            blur2.transform.localPosition = Vector3.MoveTowards(blur2.transform.localPosition, origin, 1.5f * Time.deltaTime);
            yield return null;
        }
        blur1.GetComponent<SpriteRenderer>().enabled = false;
        blur2.GetComponent<SpriteRenderer>().enabled = false;
    }

    IEnumerator sixthSense()
    {
        usingAbility1 = true;
        int endTurn = GameManager.instance.turnCount + DODGE_DURATION;

        bonusDodge += DODGE_AMOUNT;

        StartCoroutine(blurAnimation());

        useActionPoint(1);

        while (endTurn != GameManager.instance.turnCount)
            yield return null;
        while (!GameManager.instance.playerPhase)
            yield return null;

        bonusDodge -= DODGE_AMOUNT;
        usingAbility1 = false;
    }

    IEnumerator placeDeposit()
    {
        usingAbility2 = true;

        if (depositSpotDown)
            depositSpot.transform.position = transform.position;
        else
        {
            depositSpot = Instantiate(depositSpot, transform.position, Quaternion.identity) as GameObject;
            depositSpotDown = true;
        }

        useActionPoint(1);

        int endTurn = GameManager.instance.turnCount + ABILITY_COOLDOWN;
        while (GameManager.instance.turnCount != endTurn)
            yield return null;

        usingAbility2 = false;
    }

    IEnumerator teleport()
    {
        usingAbility2 = true;

        MapBehavior.instance.unitMoved(transform.position, depositSpot.transform.position);

        Vector3 targetLoc = new Vector3(0, 0.5f, 0);

        while(teleportMask.transform.localPosition != targetLoc)
        {
            teleportMask.transform.localPosition = Vector3.MoveTowards(teleportMask.transform.localPosition, targetLoc, teleportSpeed * Time.deltaTime);
            yield return null;
        }

        movePoint.transform.position = depositSpot.transform.position;
        transform.position = depositSpot.transform.position;

        targetLoc = new Vector3(0, 2, 0);
        while (teleportMask.transform.localPosition != targetLoc)
        {
            teleportMask.transform.localPosition = Vector3.MoveTowards(teleportMask.transform.localPosition, targetLoc, teleportSpeed * Time.deltaTime);
            yield return null;
        }
        useActionPoint(1);

        int endTurn = GameManager.instance.turnCount + ABILITY_COOLDOWN;
        while (GameManager.instance.turnCount != endTurn)
            yield return null;

        usingAbility2 = false;
    }

    IEnumerator bfg()
    {
        //Wait 1 frame for the GetMouseButtonDown
        yield return null;

        usingAbility3 = true;

        CollisionTile target = MapBehavior.instance.getTileAtPos(new Vector3(0, 0, 0));
        CollisionTile prevTile = target;

        foreach (Unit u in UIManager.instance.enemiesToOutline)
        {
            if (u != null)
                u.hideOutline();
        }

        List<Unit> highlightUnits = null;

        //Highlight the shot path
        while (!Input.GetMouseButtonDown(0))
        {
            //Cancel selection
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                MapBehavior.instance.eraseBFGLine();
                if (highlightUnits != null)
                    foreach (Unit u in highlightUnits)
                        u.hideOutline();
                usingAbility3 = false;
                UIManager.instance.abilityMenu();
                yield break;
            }
            target = MapBehavior.instance.getTileAtPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (target != null && target != prevTile)
            {
                if (highlightUnits != null)
                    foreach (Unit u in highlightUnits)
                        u.hideOutline();
                MapBehavior.instance.eraseBFGLine();
                MapBehavior.instance.drawBFGLine(transform.position, target.coordinate);
                prevTile = target;
                highlightUnits = MapBehavior.instance.getBFGHits(transform.position, target.coordinate);
                foreach (Unit u in highlightUnits)
                    u.showOutline();
            }
            yield return null;
        }
        MapBehavior.instance.eraseBFGLine();
        Vector3 start = Camera.main.WorldToScreenPoint(transform.position);
        Vector3 destination = Camera.main.WorldToScreenPoint(target.coordinate);
        float angle = Mathf.Atan2(destination.y - start.y, destination.x - start.x) * Mathf.Rad2Deg;
        
        RectTransform BFGBounds = BFGBeam.GetComponent<RectTransform>();
        BFGBounds.Rotate(new Vector3(0, 0, angle - 90));

        BFGBeam.GetComponent<SpriteRenderer>().enabled = true;
        while (BFGBounds.localScale.y < 75)
        {
            BFGBounds.localScale += new Vector3(0, .3f, 0);
            yield return null;
        }

        List<Unit> targetsHit = MapBehavior.instance.getBFGHits(transform.position, target.coordinate);
        if (targetsHit != null)
        {
            foreach (Unit enemy in targetsHit)
            {
                enemy.hit(equippedWeapon.damage);
                enemy.hideOutline();
            }
        }

        while (BFGBounds.localScale.x > 0)
        {
            BFGBounds.localScale -= new Vector3(0.02f, 0, 0);
            yield return null;
        }
        BFGBeam.GetComponent<SpriteRenderer>().enabled = false;

        BFGBounds.localScale = new Vector3(3, 1, 0);
        

        

        selectAbility = false;
        useActionPoint(2);

        int endTurn = GameManager.instance.turnCount + ULT_COOLDOWN;
        while (GameManager.instance.turnCount != endTurn)
            yield return null;

        usingAbility3 = false;
    }
}
