using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    protected Animator animator;
    public Transform movePoint;
    protected bool hasTurn;
    protected Greyscale greyscaleControl;

    public int hpMax;
    public int hpRemaining;
    [SerializeField]


    // Start is called before the first frame update
    virtual protected void Start()
    {
        animator = GetComponent<Animator>();
        movePoint.SetParent(null);
        hasTurn = false;
        greyscaleControl = GetComponent<Greyscale>();
    }

    // Update is called once per frame
    virtual protected void Update()
    {
        if (!hasTurn && !greyscaleControl.currGreyscale)
            greyscaleControl.makeGreyscale(true);
        else if (hasTurn && greyscaleControl.currGreyscale)
            greyscaleControl.makeGreyscale(false);

    }

    public bool hasMoved()
    {
        return !hasTurn;
    }

    public void endTurn()
    {
        hasTurn = false;
    }

    public abstract void attack(Unit enemy);

    public abstract void move();

    public abstract void hit(int damage);

    
}
