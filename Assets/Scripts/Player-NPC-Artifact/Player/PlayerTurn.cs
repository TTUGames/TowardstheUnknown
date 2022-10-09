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

    public enum PlayerState {
        ATTACK, MOVE
	}

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
            if (Input.GetKeyDown(KeyCode.Alpha1) && !playerMove.isMoving) SetState(PlayerState.ATTACK, 0);
            if (Input.GetKeyDown(KeyCode.Alpha2) && !playerMove.isMoving) SetState(PlayerState.ATTACK, 1);
            if (Input.GetKeyDown(KeyCode.Alpha3) && !playerMove.isMoving) SetState(PlayerState.ATTACK, 2);
            if (Input.GetKeyDown(KeyCode.Alpha4) && !playerMove.isMoving) SetState(PlayerState.ATTACK, 3);
            if (Input.GetKey(KeyCode.Space) && !playerMove.isMoving) SetState(PlayerState.MOVE);
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

    public void SetState(PlayerState state, int artifact = 0) {
        if (!isScriptTurn) return;
        playerAttack.SetAttackingState(state == PlayerState.ATTACK);
        playerMove.SetPlayingState(!playerAttack.GetAttackingState());
        if (playerAttack.GetAttackingState()) playerAttack.SetAttackingArtifact(artifact);

    }
}
