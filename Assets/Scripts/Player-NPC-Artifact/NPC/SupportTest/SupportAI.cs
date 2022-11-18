public class SupportAI : EnemyAI {
	protected override void SetAttackPatterns() {
		attack.AddPattern(new SupportBuffPattern());
		attack.AddPattern(new SupportAttackPattern());
	}

	protected override void SetTargetting() {
		targetting = new ClosestEnemyTargetting(2, new PlayerTargetting(1));
	}
}
