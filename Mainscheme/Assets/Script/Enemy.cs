using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private CharacterMovement enemy;
    [SerializeField]
    private int movementCost = 10;
    [SerializeField]
    private GameObject patternParentObject;
    [SerializeField]
    private bool followPattern = true;
    [SerializeField]
    private bool staticEnemy = true;
    [SerializeField]
    private Transform castPoint;
    [SerializeField]
    private int castDistance = 1000;
    [SerializeField]
    private GameObject characterLight;
    private int numberOfPatternChild;
    private int movementIndex = 1;
    private Transform patternParent;
    private float realCastDistance;
    private GameObject[] allPlayers;    
    private bool moveForward = true;
    private bool detectPlayer = false;
    private Player _player;

    void Start()
    {
        //Debug.Log(patternParent.transform.position);
        enemy.SetMovementCost(movementCost);
        realCastDistance = castDistance * BaseGrid.Instance.GetCellSize() + BaseGrid.Instance.GetCellSize()/2;
        allPlayers = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log(realCastDistance);
        if (!staticEnemy)
        {
            numberOfPatternChild = patternParentObject.transform.childCount;
            patternParent = patternParentObject.transform;
        }
    }

    void Update()
    {
        FollowPattern();
        CheckDetectPlayer();
        if (detectPlayer)
        {
            
            TriggerDetectPlayer();
        }
        else
        {
            BackToDefault();
        }
    }

    private void FixedUpdate()
    {
        
    }

    private void TriggerDetectPlayer()
    {
        characterLight.SetActive(true);
        followPattern = false;
    }

    private void BackToDefault()
    {
        characterLight.SetActive(false);
        followPattern = true;
    }

    private void FollowPattern()
    {
        if (followPattern && (enemy.GetIsMoving() == false) && !staticEnemy)
        {
            enemy.SetCharacterPosition(patternParent.GetChild(movementIndex).position);
            if (movementIndex == numberOfPatternChild - 1)
            {
                moveForward = false;
            }
            
            if (movementIndex == 0)
            {
                moveForward = true;
            }

            if (moveForward)
            {
                movementIndex++;
            }
            else
            {
                movementIndex--;
            }
        }
    }

    private void CheckDetectPlayer()
    {
        Vector2 faceDirection = enemy.GetFaceDirection();
        Vector2 endPoint = castPoint.position + new Vector3(faceDirection.x*realCastDistance, faceDirection.y*realCastDistance);
        RaycastHit2D hit = Physics2D.Raycast(castPoint.position, faceDirection, realCastDistance);
        if (hit.collider != null)
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                _player = hit.collider.gameObject.GetComponent<Player>();
                _player.SetDetected(true);
                detectPlayer = true;
            }
        }
        else{
            detectPlayer = false;
        }
    }
    
}
