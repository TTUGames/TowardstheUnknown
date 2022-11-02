using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurn : EntityTurn
{
	private void Start() {
        turnSystem = FindObjectOfType<TurnSystem>();
        turnSystem.RegisterEnemy(this);
    }

	/// <summary>
	/// Launch the turn
	/// </summary>
	public override void OnTurnLaunch()
    {
        Debug.Log("Entity : " + transform.name + " | Started his turn");
    }

    /// <summary>
    /// Stop the turn
    /// </summary>
    public override void OnTurnStop()
    {
        Debug.Log("Entity : " + transform.name + " | Ended his turn");
    }
}
