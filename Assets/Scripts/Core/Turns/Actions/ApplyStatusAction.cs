public class ApplyStatusAction : GameAction {
	private readonly EntityStats target;
	private readonly StatusEffectData status;
	private readonly int duration;

	public ApplyStatusAction(EntityStats target, StatusEffectData status, int duration) {
		this.target = target;
		this.status = status;
		this.duration = duration;
	}

	public override void Apply() {
		target.AddStatusEffect(status, duration);
		isDone = true;
	}
}
