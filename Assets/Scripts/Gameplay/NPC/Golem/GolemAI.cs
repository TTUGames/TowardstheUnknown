public class GolemAI : EnemyAI
{

	protected override void SetAttackPatterns() {
		attack.AddPattern(new GolemShockWavePattern());
		attack.AddPattern(new GolemRockFallPattern());
	}

	protected override void SetTargetting() {
		targetting = new PlayerTargetting(1);
	}
}
