using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private PlayerMove playerMove;
    private Timer      playerTimer;

    private void Start()
    {
        playerMove  = GetComponent<PlayerMove>();
        playerTimer = GetComponent<Timer>();
    }
    public void LaunchTurn()
    {
        playerMove.LaunchMovementListener();
        playerTimer.LaunchTimer();
    }

    public void StopTurn()
    {
        playerTimer.StopTimer();
    }
}
