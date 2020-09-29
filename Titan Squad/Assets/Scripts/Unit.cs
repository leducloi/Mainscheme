using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    protected Animator animator;
    public Transform movePoint;
    protected bool hasTurn;

    // Start is called before the first frame update
    virtual protected void Start()
    {
        animator = GetComponent<Animator>();
        movePoint.SetParent(null);
        hasTurn = false;
    }

    // Update is called once per frame
    void Update()
    {
        
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

    
}
