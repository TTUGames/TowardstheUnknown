using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTurn : EntityTurn
{
    private PlayerMove   playerMove;
    private PlayerAttack playerAttack;
    private PlayerStats  playerStats;
    private Timer        playerTimer;
    private bool         isScriptTurn;

    protected override void Init()
    {
        playerMove  = GetComponent<PlayerMove>();
        playerAttack = GetComponent<PlayerAttack>();
        playerTimer = GetComponent<Timer>();
        playerStats = GetComponent<PlayerStats>();
        isScriptTurn = false;
    }

    private void Update()
    {
        if(isScriptTurn)
        {
            if (Input.GetKeyDown(KeyCode.P) && !playerMove.isMoving)
            {
                playerMove.SetPlayingState(!playerMove.IsPlaying);
                playerAttack.SetAttackingState(!playerAttack.GetAttackingingState());

                if(playerAttack.GetAttackingingState())
                    playerAttack.SetAttackingArtifact(0);
            }
        }
    }

    /// <summary>
    /// Launch the turn
    /// </summary>
    public override void OnTurnLaunch()
    {
        playerStats.OnTurnLaunch();
        playerMove.SetPlayingState(true);
        isScriptTurn = true;
        playerTimer.LaunchTimer();
    }

    /// <summary>
    /// Stop the turn
    /// </summary>
    public override void OnTurnStop ()
    {
        playerStats.OnTurnStop();
        playerMove.RepaintMap();
        playerMove.SetPlayingState(false);
        playerAttack.SetAttackingState(false);
        playerTimer.StopTimer();
        isScriptTurn = false;
    }

    public bool GetIsMoving()
    {
        return playerMove.isMoving;
    }
}
