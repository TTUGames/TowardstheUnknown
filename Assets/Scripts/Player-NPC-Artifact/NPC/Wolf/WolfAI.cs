public class WolfAI : EnemyAI
{

	protected override void SetAttackPatterns() {
		attack.AddPattern(new WolfClawPattern());
		attack.AddPattern(new WolfHowlPattern());
	}

	protected override void SetTargetting() {
		targetting = new PlayerTargetting(1);
	}
}
