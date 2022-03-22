using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private PlayerMove   playerMove;
    private PlayerAttack playerAttack;
    private Timer        playerTimer;
    private bool         isScriptTurn;

    private void Start()
    {
        playerMove  = GetComponent<PlayerMove>();
        playerAttack = GetComponent<PlayerAttack>();
        playerTimer = GetComponent<Timer>();
        isScriptTurn = false;
    }

    private void Update()
    {
        if(isScriptTurn)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                playerMove.isPlaying = !playerMove.isPlaying;
                playerMove.RepaintMapWithZero();
                playerAttack.isAttacking = !playerAttack.isAttacking;
            }
        }
    }

    /// <summary>
    /// Launch the turn
    /// </summary>
    public void LaunchTurn()
    {
        playerMove.SetMovementDistanceToMax();
        playerMove.isPlaying = isScriptTurn = true;
        playerTimer.LaunchTimer();
    }

    /// <summary>
    /// Stop the turn
    /// </summary>
    public void StopTurn()
    {
        playerMove.SetMovementDistanceToZero();
        playerMove.RepaintMap();
        playerMove.isPlaying = isScriptTurn = false;
        playerTimer.StopTimer();
    }
}
