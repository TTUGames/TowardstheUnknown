using System.Collections.Generic;

public class EnemyAttack : TacticsAttack
{
	protected List<EnemyPattern> patterns = new List<EnemyPattern>();
	protected EnemyStats stats;

	protected override void Init() {
		base.Init();
		stats = GetComponent<EnemyStats>();
	}

	/// <summary>
	/// Adds a pattern to the enemy's pattern list. The first added is defined as the main pattern
	/// </summary>
	/// <param name="pattern"></param>
	public void AddPattern(EnemyPattern pattern) {
		patterns.Add(pattern);
	}

	public void ClearPatterns() {
		patterns.Clear();
	}

	/// <summary>
	/// Tries to use the first pattern possible, in the order they were added
	/// </summary>
	/// <param name="target"></param>
	public void TryAttack(EntityStats target) {
		EnemyPattern pattern = patterns.Find(p => p.CanTarget(CurrentTile, target));
		if (pattern != null) UsePattern(pattern, target);
	}

	protected void UsePattern(EnemyPattern pattern, EntityStats target) {
		pattern.Use(stats, target);
		pattern.PlayAnimation(CurrentTile, target.GetComponent<TacticsMove>().CurrentTile, gameObject);
		pattern.PlaySound(gameObject);
	}

	/// <summary>
	/// Returns the enemy's favorite pattern
	/// </summary>
	/// <returns></returns>
	public EnemyPattern GetFavoritePattern() {
		return patterns[0];
	}
}
