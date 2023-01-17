public class DraregP1S2AI : EnemyAI
{

	protected override void SetAttackPatterns() {
		attack.AddPattern(new DraregHauntingPattern());
	}

	protected override void SetTargetting() {
		targetting = new PlayerTargetting(2);
	}
}
