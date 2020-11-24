using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    public Animator animator;
    public Transform movePoint;
    public bool hasTurn;
    public bool isCloaked = false;
    public bool hpTypeFlesh;
    protected ShaderController shaderControl;

    public int hpMax;
    public int hpRemaining;

    public int shieldMax;
    public int shieldRemaining;
    
    public HealthBarControl healthBar;

    //Keep track of weapons held by the unit
    protected Weapon[] weapons = new Weapon[2];
    public Weapon equippedWeapon;

    public int combatTraining = 0;
    public int evasiveTactics = 0;
    public int bionicEnhancement = 0;
    public int luck = 0;
    public int criticalTargeting = 0;
    public int advancedShielding = 0;

    public bool isEnemy = false;
    public bool takingCover = false;

    public bool cbDrugs = false;
    public int bonusDodge = 0;

    public CombatData cbData;

    private AudioSource unitAudio = null;
    protected bool moving = false;

    private bool intensityLock = false;

    // Start is called before the first frame update
    virtual protected void Start()
    {
        animator = GetComponent<Animator>();
        movePoint.SetParent(MapBehavior.instance.transform);
        hasTurn = false;
        shaderControl = GetComponent<ShaderController>();
        
        equippedWeapon = weapons[0];

        cbData = gameObject.AddComponent<CombatData>();
        cbData.unit = this;

        healthBar.displayUnit = this;

        unitAudio = gameObject.AddComponent<AudioSource>();
        unitAudio.playOnAwake = false;
        unitAudio.clip = Resources.Load("Audio/Footsteps", typeof(AudioClip)) as AudioClip;
    }

    // Update is called once per frame
    virtual protected void Update()
    {
        if (!hasTurn && !shaderControl.currGreyscale)
            shaderControl.makeGreyscale(true);
        else if (hasTurn && shaderControl.currGreyscale)
            shaderControl.makeGreyscale(false);

        if (MapBehavior.instance != null)
            cbData.calculateData();
    }

    public bool hasMoved()
    {
        return !hasTurn;
    }

    public void endTurn()
    {
        hasTurn = false;
    }

    protected void OnMouseEnter()
    {
        if (!GameManager.instance.playerPhase)
            return;

        healthBar.showBar();

        if (!hasTurn && !shaderControl.outlineShowing)
            return;
        

        if (!shaderControl.outlineShowing)
            shaderControl.showOutline();
        else if (!shaderControl.highIntensity)
            shaderControl.setHighIntensity();
    }

    protected void OnMouseExit()
    {
        if (!healthBar.movingBar)
            healthBar.hideBar();
        if (intensityLock || !shaderControl.outlineShowing)
            return;
        if (shaderControl.highIntensity)
            shaderControl.setLowIntensity();
        else
            shaderControl.hideOutline();
        
    }

    public void showOutline()
    {
        shaderControl.showOutline();
        intensityLock = false;
    }

    public void hideOutline()
    {
        shaderControl.hideOutline();
        intensityLock = false;
    }

    public void setLowIntensity()
    {
        shaderControl.setLowIntensity();
        intensityLock = false;
    }

    public void setHighIntensity()
    {
        shaderControl.setHighIntensity();
        intensityLock = false;
    }

    public void setAndLockHighIntensity()
    {
        shaderControl.setHighIntensity();
        intensityLock = true;
    }

    public void takeCover()
    {
        CollisionTile currTile = MapBehavior.instance.getTileAtPos(transform.position);
        if (!currTile.passableEW || !currTile.passableNS)
        {
            takingCover = true;
            return;
        }

        List<CollisionTile> adjacentTiles = MapBehavior.instance.findNeighborTiles(currTile);
        foreach(CollisionTile tile in adjacentTiles)
        {
            if (!tile.passable)
            {
                Debug.Log("Taking Cover");
                takingCover = true;
                return;
            }
            if (!tile.passableNS && tile.coordinate.y < currTile.coordinate.y)
            {
                Debug.Log("Taking Cover");
                takingCover = true;
                return;
            }
            if (!tile.passableEW && tile.coordinate.x < currTile.coordinate.x)
            {
                Debug.Log("Taking Cover");
                takingCover = true;
                return;
            }
        }
        takingCover = false;
    }

    public bool isFlankedBy(Unit enemy)
    {
        if (!takingCover)
            return true;

        if (MapBehavior.instance.hasLineTo(transform.position, enemy.transform.position, enemy.equippedWeapon.maxRange, enemy.equippedWeapon.minRange))
        {
            Vector3 difference = enemy.transform.position - transform.position;

            //If either difference is zero, we have a straight shot at the enemy
            if (difference.x == 0 || difference.y == 0)
                return true;

            Vector2 direction = new Vector2(difference.x / Mathf.Abs(difference.x), difference.y / Mathf.Abs(difference.y));

            if (difference.x > 0)
            {
                CollisionTile tile = MapBehavior.instance.getTileAtPos(transform.position);
                if (!tile.passableEW)
                    return false;
            }
            if (difference.y > 0)
            {
                CollisionTile tile = MapBehavior.instance.getTileAtPos(transform.position);
                if (!tile.passableNS)
                    return false;
            }
            if (difference.x < 0)
            {
                CollisionTile tile = MapBehavior.instance.getTileAtPos(new Vector3(transform.position.x - 1, transform.position.y, 0));
                if (!tile.passableEW)
                    return false;
            }
            if (difference.y < 0)
            {
                CollisionTile tile = MapBehavior.instance.getTileAtPos(new Vector3(transform.position.x, transform.position.y - 1, 0));
                if (!tile.passableNS)
                    return false;
            }
            return true;
        }
        return false;
    }

    protected void setWalkingPosition(Vector3 newPos)
    {
        float differenceX = transform.position.x - newPos.x;
        float differenceY = transform.position.y - newPos.y;
        if (Mathf.Abs(differenceX) > Mathf.Abs(differenceY))
        {
            if(!animator.GetCurrentAnimatorStateInfo(0).IsName("Side"))
                animator.SetTrigger("Side");
            if (differenceX > 0)
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
                healthBar.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                transform.localRotation = Quaternion.Euler(0, 180, 0);
                healthBar.transform.localRotation = Quaternion.Euler(0, 180, 0);
            }
        }
        else
        {
            transform.localRotation = Quaternion.Euler(0, 0, 0);
            healthBar.transform.localRotation = Quaternion.Euler(0, 0, 0);
            if (differenceY > 0)
            {
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
                    animator.SetTrigger("Walking");
            }
            else if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Back"))
                animator.SetTrigger("Back");
        }
    }

    protected IEnumerator playFootsteps()
    {
        moving = true;
        while (moving)
        {
            unitAudio.Play();
            yield return new WaitForSeconds(0.25f);
        }
    }

    public abstract void attack(Unit enemy);

    public abstract void move();

    public abstract void hit(int damage);

    

    
}
