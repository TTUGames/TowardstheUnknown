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

    /// <summary>
    /// Launch the turn
    /// </summary>
    public void LaunchTurn()
    {
        playerMove.isPlaying = true;
        playerTimer.LaunchTimer();
    }

    /// <summary>
    /// Stop the turn
    /// </summary>
    public void StopTurn()
    {
        playerMove.isPlaying = false;
        playerTimer.StopTimer();
    }
}
