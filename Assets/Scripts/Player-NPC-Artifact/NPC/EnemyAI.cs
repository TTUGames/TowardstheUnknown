using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyAI : EntityTurn
{
    protected AbstractTargetting targetting;
    private EntityStats currentTarget;
    private EnemyMove movement;
    protected EnemyAttack attack;

    private bool hasMoved;
    private bool hasAttacked;

	protected override void Init() {
        movement = GetComponent<EnemyMove>();
        attack = GetComponent<EnemyAttack>();
        turnSystem.RegisterEnemy(this);
        SetTargetting();
        SetAttackPatterns();
    }

    /// <summary>
    /// Sets the initial targetting method
    /// </summary>
    protected abstract void SetTargetting();

    protected abstract void SetAttackPatterns();

	/// <summary>
	/// Launch the turn
	/// </summary>
	public override void OnTurnLaunch()
    {
        base.OnTurnLaunch();
        Debug.Log("Entity : " + transform.name + " | Started his turn");

        if (currentTarget == null) currentTarget = targetting.GetTarget(stats);
        hasMoved = false;
        hasAttacked = false;
    }

	public override void TurnUpdate() {
        if (ActionManager.IsBusy) return;
        if (!hasMoved) {
            DoMovement();
        }
        else if (!hasAttacked) {
            DoAttack();
        }
        else {
            ActionManager.AddToBottom(new EndTurnAction());
        }
    }

    private void DoMovement() {
        movement.SetPlayingState(true);
        movement.MoveTowardsTarget(currentTarget.GetComponent<TacticsMove>().CurrentTile, targetting.GetDistance());
        hasMoved = true;
    }

    private void DoAttack() {
        movement.SetPlayingState(false);
        attack.TryAttack(currentTarget);
        hasAttacked = true;
	}

    /// <summary>
    /// Stop the turn
    /// </summary>
    public override void OnTurnStop()
    {
        movement.SetPlayingState(false);
        Debug.Log("Entity : " + transform.name + " | Ended his turn");
        base.OnTurnStop();
    }
}
