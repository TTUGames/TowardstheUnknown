using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Enemy turn: moves towards the target then uses the first pattern that can reach it
/// </summary>
public class EnemyAI : EntityTurn
{
    [SerializeField, ShowIf(nameof(UsesPatternSet)), InlineProperty, HideLabel, BoxGroup("Patterns")]
    private EnemyPatternSet patternSet = new EnemyPatternSet();

    protected AbstractTargetting targetting;
    protected EntityStats currentTarget;
    protected EnemyMove movement;
    protected EnemyAttack attack;

    protected bool hasMoved;
    protected bool hasAttacked;

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
    /// Replaces the enemy's targetting and attack patterns
    /// </summary>
    protected void UsePatternSet(EnemyPatternSet set) {
        targetting = new PlayerTargetting(set.targetDistance);
        attack.ClearPatterns();
        foreach (EnemyPatternData pattern in set.patterns)
            attack.AddPattern(new EnemyPattern(pattern));
    }

	/// <summary>
	/// Launch the turn
	/// </summary>
	public override void OnTurnLaunch()
    {
        base.OnTurnLaunch();
        if (currentTarget == null) currentTarget = targetting.GetTarget(stats);
        hasMoved = false;
        hasAttacked = false;
    }

    /// <summary>
    /// Called every frame during the enemy's turn, tries to move then attack then end turn
    /// </summary>
	public override void TurnUpdate() {
        if (ActionManager.IsBusy) return;
        if (!hasMoved) {
            DoMovement();
        }
        else if (!hasAttacked) {
            DoAttack();
        }
        else {
            ActionManager.AddToBottom(new EndTurnAction());
        }
    }

    /// <summary>
    /// Does this turn's movement action
    /// </summary>
    private void DoMovement() {
        movement.SetPlayingState(true);
        movement.MoveTowardsTarget(currentTarget.GetComponent<TacticsMove>().CurrentTile, attack.GetFavoritePattern().GetRange(), targetting.GetDistance());
        hasMoved = true;
    }

    /// <summary>
    /// Does this turn's attack action
    /// </summary>
    private void DoAttack() {
        movement.SetPlayingState(false);
        attack.TryAttack(currentTarget);
        hasAttacked = true;
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
