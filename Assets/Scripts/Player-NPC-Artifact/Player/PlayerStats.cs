using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Extends base stat with player specific ones
/// </summary>
public class PlayerStats : EntityStats
{
    [SerializeField] protected int maxEnergy;
	protected int currentEnergy;
	public int CurrentEnergy { get { return currentEnergy; } }

	public override void OnTurnLaunch() {
		base.OnTurnLaunch();
		currentEnergy = maxEnergy;
	}

	public override void OnTurnStop() {
		base.OnTurnStop();
		currentEnergy = 0;
	}

	/// <summary>
	/// Spends an amount of energy if able
	/// </summary>
	/// <param name="amount"></param>
	/// <exception cref="System.Exception">Throws an exception if the amount of energy is invalid</exception>
	public void UseEnergy(int amount) {
		if (amount < 0 || amount > currentEnergy) throw new System.Exception("Unable to use " + amount + " energy when " + currentEnergy + " remains.");
		else currentEnergy -= amount;
	}

	public override void UseMovement() {
		UseEnergy(1);
	}

	public override int GetMovementDistance() {
		return currentEnergy;
	}

	protected override void Die() {
		Debug.Log("Player is dead");
		base.Die();
	}
}
