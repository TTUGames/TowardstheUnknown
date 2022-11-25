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
        SetTargetting();
        SetAttackPatterns();
        movement.SetAttackRange(attack.GetFavoritePattern().GetRange());
    }

    /// <summary>
    /// Sets the initial targetting method
    /// </summary>
    protected abstract void SetTargetting();

    /// <summary>
    /// Sets the enemy's attack patterns
    /// </summary>
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

    /// <summary>
    /// Called every frame during the enemy's turn, tries to move then attack then end turn
    /// </summary>
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

    /// <summary>
    /// Does this turn's movement action
    /// </summary>
    private void DoMovement() {
        movement.SetPlayingState(true);
        movement.MoveTowardsTarget(currentTarget.GetComponent<TacticsMove>().CurrentTile, targetting.GetDistance());
        hasMoved = true;
    }

    /// <summary>
    /// Does this turn's attack action
    /// </summary>
    private void DoAttack() {
        movement.SetPlayingState(false);
        attack.TryAttack(currentTarget);
        hasAttacked = true;
	}

    /// <summary>
    /// Stops the turn
    /// </summary>
    public override void OnTurnStop()
    {
        movement.SetPlayingState(false);
        Debug.Log("Entity : " + transform.name + " | Ended his turn");
        base.OnTurnStop();
    }
}
