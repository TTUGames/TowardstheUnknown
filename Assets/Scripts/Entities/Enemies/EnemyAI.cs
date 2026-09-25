using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Enemy turn: moves towards the target then uses the first pattern that can reach it
/// </summary>
public class EnemyAI : EntityTurn
{
    [SerializeField, ShowIf(nameof(UsesPatternSet)), InlineProperty, HideLabel, BoxGroup("Patterns")]
    private EnemyPatternSet patternSet = new EnemyPatternSet();

    protected int targetDistance;
    protected EntityStats currentTarget;
    protected EnemyMove movement;
    protected EnemyAttack attack;

	protected override void Init() {
        movement = GetComponent<EnemyMove>();
        attack = GetComponent<EnemyAttack>();
        UsePatternSet(InitialPatternSet);
    }

    /// <summary>
    /// The patterns used when the enemy spawns
    /// </summary>
    protected virtual EnemyPatternSet InitialPatternSet => patternSet;

    protected virtual bool UsesPatternSet => true;

    /// <summary>
    /// Replaces the enemy's distance to its target and attack patterns
    /// </summary>
    protected void UsePatternSet(EnemyPatternSet set) {
        targetDistance = set.targetDistance;
        attack.ClearPatterns();
        foreach (EnemyPatternData pattern in set.patterns)
            attack.AddPattern(new EnemyPattern(pattern));
    }

	/// <summary>
	/// Launch the turn: each step waits for the actions of the previous one
	/// </summary>
	public override void OnTurnLaunch()
    {
        base.OnTurnLaunch();
        if (currentTarget == null) currentTarget = GameScene.Player.Stats;
        NextStep(PlayTurn);
    }

    /// <summary>
    /// Runs a step of this turn once the action queue is empty, unless the turn or the combat ended meanwhile
    /// </summary>
    protected void NextStep(System.Action step) {
        ActionManager.WhenFree(() => {
            if (this != null && turnSystem.IsCurrentTurn(this)) step();
        });
    }

    /// <summary>
    /// Moves towards the target, then attacks, then ends the turn
    /// </summary>
    protected virtual void PlayTurn() {
        DoMovement();
        NextStep(() => {
            DoAttack();
            NextStep(EndTurn);
        });
    }

    protected void EndTurn() {
        ActionManager.AddToBottom(new EndTurnAction());
    }

    /// <summary>
    /// Does this turn's movement action
    /// </summary>
    private void DoMovement() {
        movement.SetPlayingState(true);
        movement.MoveTowardsTarget(currentTarget.GetComponent<TacticsMove>().CurrentTile, attack.GetFavoritePattern().GetRange(), targetDistance);
    }

    /// <summary>
    /// Does this turn's attack action
    /// </summary>
    private void DoAttack() {
        movement.SetPlayingState(false);
        attack.TryAttack(currentTarget);
	}

    /// <summary>
    /// Stops the turn
    /// </summary>
    public override void OnTurnStop()
    {
        movement.SetPlayingState(false);
        base.OnTurnStop();
    }
}
