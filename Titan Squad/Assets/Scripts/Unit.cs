using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    protected Animator animator;
    public Transform movePoint;

    // Start is called before the first frame update
    virtual protected void Start()
    {
        animator = GetComponent<Animator>();
        movePoint.SetParent(null);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public abstract void attack(Unit enemy);

    public abstract void move();
}
