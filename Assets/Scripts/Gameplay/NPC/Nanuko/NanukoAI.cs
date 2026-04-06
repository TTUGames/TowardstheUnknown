public class NanukoAI : EnemyAI
{

	protected override void SetAttackPatterns() {
		attack.AddPattern(new NanukoStrikePattern());
		attack.AddPattern(new NanukoHauntingPattern());
	}

	protected override void SetTargetting() {
		targetting = new PlayerTargetting(1);
	}
}
