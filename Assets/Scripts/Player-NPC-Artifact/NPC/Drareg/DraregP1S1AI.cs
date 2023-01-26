public class DraregP1S1AI : EnemyAI
{

	protected override void SetAttackPatterns() {
		attack.AddPattern(new DraregStrikePattern());
	}

	protected override void SetTargetting() {
		targetting = new PlayerTargetting(1);
	}
}
