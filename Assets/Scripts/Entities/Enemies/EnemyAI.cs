using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Enemy turn: moves towards the target (<see cref="EnemyTargeting"/>) then uses the first pattern that can reach it
/// </summary>
public class EnemyAI : EntityTurn
{
    [SerializeField, ShowIf(nameof(UsesPatternSet)), InlineProperty, HideLabel, BoxGroup("Patterns")]
    private EnemyPatternSet patternSet = new EnemyPatternSet();
    [SerializeField, Tooltip("Ends its turns without moving nor attacking: the training dummy")] private bool passive;

    protected int targetDistance;
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

    // How the current pattern set picks the target
    private EnemyTargeting targeting = EnemyTargeting.Default;

    /// <summary>
    /// The entity the enemy goes for this turn, picked by its pattern set's targeting as the turn's steps start (the player
    /// by default)
    /// </summary>
    protected EntityStats CurrentTarget { get; private set; }

    /// <summary>
    /// Every pattern the enemy may use, read from its serialized data: available as soon as it spawns
    /// </summary>
    public virtual IEnumerable<EnemyPatternData> AllPatterns => patternSet.patterns;

    /// <summary>
    /// Replaces the enemy's distance to its target and attack patterns
    /// </summary>
    protected void UsePatternSet(EnemyPatternSet set) {
        targetDistance = set.targetDistance;
        targeting = set.targeting ?? EnemyTargeting.Default;
        attack.SetPatterns(set.patterns);
    }

	/// <summary>
	/// Launch the turn: each step waits for the actions of the previous one
	/// </summary>
	public override void OnTurnLaunch()
    {
        base.OnTurnLaunch();
        PlayTurn();
    }

    // async void: Unity logs the exceptions of the steps
    private async void PlayTurn() {
        if (!await WaitForActions()) return;
        CurrentTarget = targeting.Pick(this, EnemyTargeting.Hostiles());
        await PlaySteps();
    }

    /// <summary>
    /// Waits for the action queue to be empty. False if the turn or the combat ended meanwhile, the turn then stopping there
    /// </summary>
    protected async Awaitable<bool> WaitForActions() {
        await ActionManager.WaitFree();
        return this != null && turnSystem.IsCurrentTurn(this);
    }

    /// <summary>
    /// Moves towards the target, then attacks, then ends the turn
    /// </summary>
    protected virtual async Awaitable PlaySteps() {
        if (passive) {
            EndTurn();
            return;
        }
        if (CurrentTarget == null) {
            EndTurn();
            return;
        }
        DoMovement();
        if (!await WaitForActions()) return;
        // Out of reach, it ends its turn facing its target rather than where its last step led (EditionProfile.faceTargetAfterMove)
        if (!DoAttack() && Edition.Profile.faceTargetAfterMove && CurrentTarget != null) {
            ActionManager.AddToBottom(new TurnTowardsAction(transform, CurrentTarget.transform.position, FaceTargetSpeed));
        }
        if (!await WaitForActions()) return;
        EndTurn();
    }

    // Degrees per second of the turn towards the target, unhurried
    private const float FaceTargetSpeed = 540;

    protected void EndTurn() {
        // A breath before the next enemy, so that each turn reads apart (EditionProfile.enemyTurnGap)
        float gap = Edition.Profile.enemyTurnGap;
        EntityTurn next = TurnSystem.Instance.Next;
        if (gap > 0 && next != null && next != this && next is EnemyAI) ActionManager.AddToBottom(new WaitAction(gap));
        ActionManager.AddToBottom(TurnSystem.Instance.GoToNextTurn);
    }

    /// <summary>
    /// Does this turn's movement action; an enemy without any pattern stays where it is
    /// </summary>
    private void DoMovement() {
        EnemyPattern favorite = attack.GetFavoritePattern();
        if (favorite == null) return;
        movement.MoveTowardsTarget(CurrentTarget.Tile, favorite.Range, targetDistance);
    }

    /// <summary>
    /// Does this turn's attack action
    /// </summary>
    private bool DoAttack() {
        return attack.TryAttack(CurrentTarget);
	}
}
