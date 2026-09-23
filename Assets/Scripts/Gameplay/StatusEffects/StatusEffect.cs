public abstract class StatusEffect
{
    protected int duration;
    protected EntityStats owner;
    protected string id;

    public StatusEffect(int duration) {
        this.duration = duration;
	}

    public void OnTurnStart() {
        --duration;
        if (duration <= 0) owner.QueueStatusEffectForRemoval(this);
	}

    public virtual void OnApply(EntityStats owner) {
        this.owner = owner;
	}
    public abstract void OnRemove();

    public string ID { get => id; }
    public int Duration { get => duration; set => duration = value; }
}

/// <summary>
/// Changes a damage multiplier while active. Applying it on an entity having the opposite status cancels both.
/// </summary>
public abstract class StatModifierStatus : StatusEffect {
	private readonly string oppositeId;
	private readonly bool affectsDamageDealt;
	private readonly float delta;

	protected StatModifierStatus(int duration, string id, string oppositeId, bool affectsDamageDealt, float delta) : base(duration) {
		this.id = id;
		this.oppositeId = oppositeId;
		this.affectsDamageDealt = affectsDamageDealt;
		this.delta = delta;
	}

	public override void OnApply(EntityStats owner) {
		base.OnApply(owner);
		Modify(delta);
		if (owner.HasStatusEffect(oppositeId)) {
			owner.RemoveStatusEffect(owner.GetStatusEffect(oppositeId));
			owner.RemoveStatusEffect(this);
		}
	}

	public override void OnRemove() {
		Modify(-delta);
	}

	private void Modify(float amount) {
		if (affectsDamageDealt) owner.DamageDealtMultiplier += amount;
		else owner.DamageReceivedMultiplier += amount;
	}
}

public class AttackUpStatus : StatModifierStatus {
	public AttackUpStatus(int duration) : base(duration, "AttackUp", "AttackDown", true, 0.25f) { }
}

public class AttackDownStatus : StatModifierStatus {
	public AttackDownStatus(int duration) : base(duration, "AttackDown", "AttackUp", true, -0.25f) { }
}

public class DefenseUpStatus : StatModifierStatus {
	public DefenseUpStatus(int duration) : base(duration, "DefenseUp", "DefenseDown", false, -0.25f) { }
}

public class DefenseDownStatus : StatModifierStatus {
	public DefenseDownStatus(int duration) : base(duration, "DefenseDown", "DefenseUp", false, 0.25f) { }
}
