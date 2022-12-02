using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTurn : EntityTurn
{
    private PlayerMove   playerMove;
    private PlayerAttack playerAttack;
    private Timer        playerTimer;
    private Inventory inventory;

    public enum PlayerState {
        ATTACK, MOVE
	}

    protected override void Init() {
        playerMove = GetComponent<PlayerMove>();
        playerAttack = GetComponent<PlayerAttack>();
        playerTimer = GetComponent<Timer>();
        inventory = GetComponent<Inventory>();
    }

    public override void TurnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && !playerMove.isMoving) SetState(PlayerState.ATTACK, 0);
        if (Input.GetKeyDown(KeyCode.Alpha2) && !playerMove.isMoving) SetState(PlayerState.ATTACK, 1);
        if (Input.GetKeyDown(KeyCode.Alpha3) && !playerMove.isMoving) SetState(PlayerState.ATTACK, 2);
        if (Input.GetKeyDown(KeyCode.Alpha4) && !playerMove.isMoving) SetState(PlayerState.ATTACK, 3);
        if (Input.GetKey(KeyCode.Space) && !playerMove.isMoving) SetState(PlayerState.MOVE);
    }

    /// <summary>
    /// Launch the turn
    /// </summary>
    public override void OnTurnLaunch()
    {
        base.OnTurnLaunch();
        playerMove.SetPlayingState(true);
        if (turnSystem.IsCombat) {
            playerTimer.LaunchTimer();
            inventory.TurnStart();
        }
    }

    /// <summary>
    /// Stop the turn
    /// </summary>
    public override void OnTurnStop ()
    {
        playerTimer.StopTimer();
        playerMove.SetPlayingState(false);
        playerAttack.SetAttackingState(false);
        base.OnTurnStop();
    }

    /// <summary>
    /// Sets the player's state among the <c>PlayerState</c>
    /// </summary>
    /// <param name="state">The player's new state</param>
    /// <param name="artifact">If attacking, the artifact's index</param>
    public void SetState(PlayerState state, int artifact = 0) {
        switch (state) {
            case PlayerState.MOVE:
                if (!playerMove.IsPlaying) {
                    playerAttack.SetAttackingState(false);
                    playerMove.SetPlayingState(true);
                }
                else playerMove.FindSelectibleTiles();
                break;
            case PlayerState.ATTACK:
                if (!turnSystem.IsCombat) return;
                if (!playerAttack.GetAttackingState()) {
                    playerMove.SetPlayingState(false);
                    playerAttack.SetAttackingState(true);
				}
                playerAttack.SetAttackingArtifact(artifact);
                break;
		}
    }

    /// <summary>
    /// On combat end, sets the player's state to move
    /// </summary>
    public void OnCombatEnd() {
        SetState(PlayerState.MOVE);
        playerTimer.StopTimer();
	}
}
