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
	
	public override void OnTurnLaunch() {
		base.OnTurnLaunch();
		currentEnergy = maxEnergy;
	}

	public override void OnTurnStop() {
		base.OnTurnStop();
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

	public override void UseMovement(int distance) {
		UseEnergy(distance);
	}

	public override int GetMovementDistance() {
		return currentEnergy;
	}

	protected override void Die() {
		Debug.Log("Player is dead");
        currentHealth = 0;
        base.Die();
	}

    public int MaxEnergy { get { return maxEnergy; } }
    public int CurrentEnergy { get { return currentEnergy; } }
}
