public class GolemAI : EnemyAI
{

	protected override void SetAttackPatterns() {
		attack.AddPattern(new ShockWavePattern());
		attack.AddPattern(new RockFallPattern());
	}

	protected override void SetTargetting() {
		targetting = new PlayerTargetting(1);
	}
}
