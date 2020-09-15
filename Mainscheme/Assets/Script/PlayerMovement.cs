using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 10f;
    Vector3 targetPosition;
    private int currentIndex;
    private Vector3 characterPosition;
    private List<Vector3> pathFindingList;
    private Boolean isMoving;
    public Animator animator;
    void Start()
    {
        isMoving = false;
        characterPosition = transform.parent.position;
        targetPosition = new Vector3(characterPosition.x, characterPosition.y);
        Debug.Log(transform.position.x + " " + transform.position.y);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && (isMoving == false))
        {
            SetCharacterPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }

    public Vector3 GetPositon()
    {
        return transform.parent.position;
    }

    public void SetCharacterPosition(Vector3 toPosition)
    {
        currentIndex = 0;
        targetPosition = toPosition;
        pathFindingList = PathFinding.Instance.FindPath(GetPositon(), targetPosition);
        if (pathFindingList != null && pathFindingList.Count > 1)
        {
            isMoving = true;
            pathFindingList.RemoveAt(0);
        }
    }

    private void StopMovement()
    {
        pathFindingList = null;
        isMoving = false;
        animator.SetFloat("Speed", 0);
    }

    void FixedUpdate()
    {
        if (pathFindingList != null)
        {
            Vector3 tempTargetPosition = pathFindingList[currentIndex];
            targetPosition = new Vector3(tempTargetPosition.x, tempTargetPosition.y, GetPositon().z);
            if (GetPositon() != targetPosition)
            {
                Vector3 moveDir = (targetPosition - transform.parent.position).normalized;
                Vector2 moveDirV2 = moveDir;
                animator.SetFloat("Horizontal", moveDir.x);
                animator.SetFloat("Vertical", moveDir.y);
                animator.SetFloat("Speed", moveDirV2.sqrMagnitude);
                transform.parent.position = Vector3.MoveTowards(transform.parent.position, targetPosition, moveSpeed * Time.deltaTime);
            }
            else
            {
                currentIndex++;
                if (currentIndex >= pathFindingList.Count)
                {
                    StopMovement();
                }
            }
        }
    }
}
