using UnityEngine;

public class DamageAction : GameAction {
	EntityStats source;
	EntityStats target;
	private int minAmount;
	private int maxAmount;
	private bool ignoreArmor;
	public DamageAction(EntityStats source, EntityStats target, int minAmount, int maxAmount, bool ignoreArmor = false){
		this.ignoreArmor = ignoreArmor;
		this.source = source;
		this.target = target;
		this.minAmount = minAmount;
		this.maxAmount = maxAmount;
	}

	public override void Apply() {
		target.TakeDamage(Mathf.CeilToInt(Random.Range(minAmount, maxAmount+1) * source.DamageDealtMultiplier * target.DamageReceivedMultiplier), ignoreArmor);
		isDone = true;
	}
}
