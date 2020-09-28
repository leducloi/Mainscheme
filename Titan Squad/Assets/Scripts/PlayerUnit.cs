﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : Unit
{
    private Animator animator;
    private bool canMove;
    private int movement;
    private float moveSpeed = 5f;


    public Transform movePoint;

    // Start is called before the first frame update
    void Start()
    {
        movePoint.SetParent(null);
        animator = GetComponent<Animator>();
        movement = 5;
        canMove = false;
    }

    //Trigger to detect when a player is clicked
    void OnMouseDown()
    {
        //If it's the player phase, then we select the unit
        if (GameManager.instance.playerPhase)
        {
            //Right now, all we do is enable them to walk. In the future this will pull open the selection menu
            animator.SetTrigger("Walking");
            StartCoroutine(wait());
        }
    }

    //This coroutine is used to add a slight pause
    IEnumerator wait()
    {
        yield return new WaitForSeconds(.1f);
        canMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        //We always want the character to be moving towards the spot they're supposed to be at, represented by the movePoint
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);

        //If we're allowed to move, on a mouse click we move to that position
        if (canMove)
        {
            if (Input.GetMouseButtonDown(0))
            {
                move();
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

    private IEnumerator moveAlongPath(CollisionTile[] path)
    {
        int index = 0;
        //If the path was invalid
        if (path == null)
            yield break;

        //Remove movement permission
        canMove = false;

        
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
        //Once we've moved, we stop the moving animation
        animator.SetTrigger("Stopped");
        yield return null;
    }

    override
    public void attack(Unit enemy)
    {
        //TODO
    }
}