public class KameikoAI : EnemyAI
{

	protected override void SetAttackPatterns() {
		attack.AddPattern(new KameikoSlashAttackPattern());
	}

	protected override void SetTargetting() {
		targetting = new PlayerTargetting(1);
	}
}
