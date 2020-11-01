﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    public Animator animator;
    public Transform movePoint;
    public bool hasTurn;
    public bool isCloaked = false;
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

    public bool takingCover = false;
    public int bonusDodge = 0;


    private bool intensityLock = false;

    // Start is called before the first frame update
    virtual protected void Start()
    {
        animator = GetComponent<Animator>();
        movePoint.SetParent(null);
        hasTurn = false;
        shaderControl = GetComponent<ShaderController>();
        
        weapons[0] = new Weapon("Pistol");
        weapons[1] = new Weapon("Rifle");
        equippedWeapon = weapons[0];


    }

    // Update is called once per frame
    virtual protected void Update()
    {
        if (!hasTurn && !shaderControl.currGreyscale)
            shaderControl.makeGreyscale(true);
        else if (hasTurn && shaderControl.currGreyscale)
            shaderControl.makeGreyscale(false);

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
        if ((!hasTurn && !shaderControl.outlineShowing) || GameManager.instance.enemyPhase)
            return;

        if (!shaderControl.outlineShowing)
            shaderControl.showOutline();
        else if (!shaderControl.highIntensity)
            shaderControl.setHighIntensity();
    }

    protected void OnMouseExit()
    {
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
        List<CollisionTile> adjacentTiles = MapBehavior.instance.findNeighborTiles(MapBehavior.instance.getTileAtPos(transform.position));
        foreach(CollisionTile tile in adjacentTiles)
        {
            if (!tile.passable)
            {
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

        if (MapBehavior.instance.hasLineTo(transform.position, enemy.transform.position, enemy.equippedWeapon.maxRange))
        {
            Vector3 difference = enemy.transform.position - transform.position;

            //If either difference is zero, we have a straight shot at the enemy
            if (difference.x == 0 || difference.y == 0)
                return true;

            Vector2 direction = new Vector2(difference.x / Mathf.Abs(difference.x), difference.y / Mathf.Abs(difference.y));

            //Get the two tiles that could possibly block our shot
            CollisionTile tileNS = MapBehavior.instance.getTileAtPos(new Vector3(transform.position.x, transform.position.y + direction.y, 0f));
            CollisionTile tileEW = MapBehavior.instance.getTileAtPos(new Vector3(transform.position.x + direction.x, transform.position.y, 0f));

            //If both tiles are passable, we flank
            if (tileNS.passable && tileEW.passable)
                return true;
        }
        return false;
    }

    public abstract void attack(Unit enemy);

    public abstract void move();

    public abstract void hit(int damage);

    

    
}
