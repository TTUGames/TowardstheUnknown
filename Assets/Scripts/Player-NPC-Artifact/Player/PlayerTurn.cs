using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTurn : EntityTurn
{
    private PlayerMove   playerMove;
    private PlayerAttack playerAttack;
    private PlayerStats  playerStats;
    private Timer        playerTimer;
    private Inventory inventory;
    private bool         isScriptTurn;

    public enum PlayerState {
        ATTACK, MOVE
	}

	private void Start() 
    {
        
        playerMove  = GetComponent<PlayerMove>();
        playerAttack = GetComponent<PlayerAttack>();
        playerTimer = GetComponent<Timer>();
        playerStats = GetComponent<PlayerStats>();
        inventory = GetComponent<Inventory>();

        turnSystem = FindObjectOfType<TurnSystem>();
        turnSystem.RegisterPlayer(this);

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
        isScriptTurn = true;
        playerStats.OnTurnLaunch();
        playerMove.SetPlayingState(true);
        playerTimer.LaunchTimer();
        inventory.TurnStart();
    }

    /// <summary>
    /// Stop the turn
    /// </summary>
    public override void OnTurnStop ()
    {
        playerTimer.StopTimer();
        playerStats.OnTurnStop();
        playerMove.SetPlayingState(false);
        playerAttack.SetAttackingState(false);
        isScriptTurn = false;
    }

    /// <summary>
    /// Sets the player's state among the <c>PlayerState</c>
    /// </summary>
    /// <param name="state">The player's new state</param>
    /// <param name="artifact">If attacking, the artifact's index</param>
    public void SetState(PlayerState state, int artifact = 0) {
        if (!isScriptTurn) return;
        if (!turnSystem.IsCombat && state != PlayerState.MOVE) return;

        playerAttack.SetAttackingState(state == PlayerState.ATTACK);
        playerMove.SetPlayingState(state == PlayerState.MOVE);
        if (playerAttack.GetAttackingState()) playerAttack.SetAttackingArtifact(artifact);

    }

    /// <summary>
    /// On combat end, sets the player's state to move
    /// </summary>
    public void OnCombatEnd() {
        SetState(PlayerState.MOVE);
	}
}
