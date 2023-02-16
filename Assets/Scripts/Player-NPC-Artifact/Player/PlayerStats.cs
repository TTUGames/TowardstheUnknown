using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Extends base stat with player specific ones
/// </summary>
public class PlayerStats : EntityStats
{
    [SerializeField] protected int maxEnergy;
	[SerializeField] protected int antechamberHeal;
	[SerializeField] protected int combatRoomHeal;
    [SerializeField] protected Color playerColor;
	[SerializeField] private BuffDebuff buffDebuff;
    protected int currentEnergy;
	
	public override void OnTurnLaunch() {
		base.OnTurnLaunch();
		currentEnergy = maxEnergy;
		buffDebuff.DisplayBuffDebuff();
	}

	public override void AddStatusEffect(StatusEffect effect) {
		base.AddStatusEffect(effect);
		buffDebuff.DisplayBuffDebuff();
	}

	public override void OnTurnStop() {
		base.OnTurnStop();
	}

	public override void OnCombatEnd() {
		base.OnCombatEnd();
		currentEnergy = maxEnergy;
	}

	/// <summary>
	/// Spends an amount of energy if able
	/// </summary>
	/// <param name="amount"></param>
	/// <exception cref="System.Exception">Throws an exception if the amount of energy is invalid</exception>
	public void UseEnergy(int amount) {
		if (amount < 0 || amount > currentEnergy)
			throw new System.Exception("Unable to use " + amount + " energy when " + currentEnergy + " remains.");
		currentEnergy -= amount;
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
		GameObject.Find("UI").GetComponent<Results>().DisplayResultCanvas(false);
		SteamAchievements.IncrementStat("death", 1);
	}
	public void OnFirstTimeRoomEnter(Room room) {
		if (room.type == RoomType.ANTECHAMBER) {
			Heal(antechamberHeal);
		}
		if (room.type == RoomType.COMBAT) {
			Heal(combatRoomHeal);
		}
	}
	public int MaxEnergy { get { return maxEnergy; } }
    public int CurrentEnergy { get { return currentEnergy; } }
}
