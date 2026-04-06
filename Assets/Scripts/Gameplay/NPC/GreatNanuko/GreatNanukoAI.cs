public class GreatNanukoAI : EnemyAI
{

	protected override void SetAttackPatterns() {
		attack.AddPattern(new GreatNanukoRushPattern());
		attack.AddPattern(new GreatNanukoDefensiveFluidPattern());
	}

	protected override void SetTargetting() {
		targetting = new PlayerTargetting(1);
	}
}
