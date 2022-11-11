using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurn : EntityTurn
{
    private EntityStats target;
    private EnemyMove movement;
    private EnemyAttack attack;

    private bool hasMoved;
    private bool hasAttacked;

	protected override void Init() {
        movement = GetComponent<EnemyMove>();
        attack = GetComponent<EnemyAttack>();
        turnSystem.RegisterEnemy(this);
        target = GetComponent<AbstractTargetting>().GetTarget();
    }

	/// <summary>
	/// Launch the turn
	/// </summary>
	public override void OnTurnLaunch()
    {
        base.OnTurnLaunch();
        Debug.Log("Entity : " + transform.name + " | Started his turn");
        movement.SetPlayingState(true);

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
        movement.MoveTowardsTarget(target.GetComponent<TacticsMove>().CurrentTile);
        hasMoved = true;
    }

    private void DoAttack() {
        attack.TryAttack(target);
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
