using UnityEngine;

/// <summary>
/// Extends base stat with player specific ones
/// </summary>
public class PlayerStats : EntityStats
{
    [SerializeField] protected int maxEnergy;
	[SerializeField] protected int antechamberHeal;
	[SerializeField, UnityEngine.Serialization.FormerlySerializedAs("combatRoomHeal"), Tooltip("Healed when the player wins a combat")] protected int combatVictoryHeal;
    protected int currentEnergy;

	/// <summary>
	/// Fired when the current energy changes
	/// </summary>
	public event System.Action EnergyChanged;

	/// <summary>
	/// Fired with the energy cost of the hovered move or selected artifact, 0 when there is none
	/// </summary>
	public event System.Action<int> EnergyCostPreviewed;

	private void OnEnable() {
		GameEvents.RoomEntered += OnRoomEntered;
		GameEvents.CombatEnded += OnCombatEnded;
	}

	private void OnDisable() {
		GameEvents.RoomEntered -= OnRoomEntered;
		GameEvents.CombatEnded -= OnCombatEnded;
	}

	public override void OnTurnLaunch() {
		base.OnTurnLaunch();
		SetEnergy(maxEnergy);
	}

	public override void OnCombatEnd() {
		base.OnCombatEnd();
		SetEnergy(maxEnergy);
	}

	private void SetEnergy(int energy) {
		currentEnergy = energy;
		EnergyChanged?.Invoke();
	}

	public void PreviewEnergyCost(int cost) => EnergyCostPreviewed?.Invoke(cost);

	/// <summary>
	/// Spends an amount of energy if able
	/// </summary>
	/// <param name="amount"></param>
	/// <exception cref="System.Exception">Throws an exception if the amount of energy is invalid</exception>
	public void UseEnergy(int amount) {
		if (amount < 0 || amount > currentEnergy)
			throw new System.Exception("Unable to use " + amount + " energy when " + currentEnergy + " remains.");
		SetEnergy(currentEnergy - amount);
	}

	public override void UseMovement(int distance) {
		UseEnergy(distance);
	}

	public override int GetMovementDistance() {
		return currentEnergy;
	}

	protected override void Die() {
        base.Die();
		GameEvents.EndRun(false);
	}

	/// <summary>
	/// Heals the player on the first visit of an antechamber
	/// </summary>
	private void OnRoomEntered(Room room, bool firstVisit) {
		if (!firstVisit) return;
		if (room.type == RoomType.ANTECHAMBER) Heal(antechamberHeal);
	}

	/// <summary>
	/// Heals the player after each combat won
	/// </summary>
	private void OnCombatEnded() {
		if (!IsDead) Heal(combatVictoryHeal);
	}

	public int MaxEnergy => maxEnergy;
    public int CurrentEnergy => currentEnergy;
}
