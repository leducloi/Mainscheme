using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private CharacterMovement player;
    public int movementCost = 10;

    private void Start()
    {
        player.SetMovementCost(this.movementCost);
    }
    void Update()
    {
        if (GameManager.instance.playerPhase)
        {
            if (Input.GetMouseButtonDown(0) && (player.GetIsMoving() == false))
            {
                player.SetCharacterPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
        }
        
    }
}
