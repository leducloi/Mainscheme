using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    protected Animator animator;
    public Transform movePoint;
    protected bool hasTurn;
    protected ShaderController shaderControl;

    public int hpMax;
    public int hpRemaining;

    //Keep track of weapons held by the unit
    protected Weapon[] weapons = new Weapon[2];
    public Weapon equippedWeapon;

    public int combatTraining = 0;
    public int evasiveTactics = 0;
    public int bionicEnhancement = 0;
    public int luck = 0;


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
        Debug.Log("Trying to show outline");
        shaderControl.showOutline();
    }

    protected void OnMouseExit()
    {
        shaderControl.hideOutline();
    }

    

    public abstract void attack(Unit enemy);

    public abstract void move();

    public abstract void hit(int damage);

    public abstract bool isHiddenFrom(Unit enemy);

    
}
