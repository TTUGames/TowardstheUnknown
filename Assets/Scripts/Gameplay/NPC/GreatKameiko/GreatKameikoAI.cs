public class GreatKameikoAI : EnemyAI
{

	protected override void SetAttackPatterns() {
		attack.AddPattern(new GreatKameikoSlashAttackPattern());
		attack.AddPattern(new GreatKameikoOffensiveFluidPattern());
	}

	protected override void SetTargetting() {
		targetting = new PlayerTargetting(2);
	}
}
