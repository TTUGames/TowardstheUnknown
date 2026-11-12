using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Enemy turn: moves towards the player then uses the first pattern that can reach it
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
    /// What the AI keeps that its next turns depend on, for the state snapshot (<see cref="StateSnapshot"/>): nothing for most
    /// </summary>
    public virtual string LogicalState => "";

    /// <summary>
    /// The patterns used when the enemy spawns
    /// </summary>
    protected virtual EnemyPatternSet InitialPatternSet => patternSet;

    protected virtual bool UsesPatternSet => true;

    /// <summary>
    /// The entity the enemy goes for: the player
    /// </summary>
    protected EntityStats CurrentTarget => GameScene.Player.Stats;

    /// <summary>
    /// Every pattern the enemy may use, read from its serialized data: available as soon as it spawns
    /// </summary>
    public virtual IEnumerable<EnemyPatternData> AllPatterns => patternSet.patterns;

    /// <summary>
    /// Replaces the enemy's distance to its target and attack patterns
    /// </summary>
    protected void UsePatternSet(EnemyPatternSet set) {
        targetDistance = set.targetDistance;
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
        if (passive || CurrentTarget == null) {
            EndTurn();
            return;
        }
        // An enemy without any pattern stays where it is
        EnemyPattern favorite = attack.FavoritePattern;
        if (favorite != null) movement.MoveTowardsTarget(CurrentTarget.Tile, favorite.Range, targetDistance);
        if (!await WaitForActions()) return;
        // Out of reach, it ends its turn facing its target rather than where its last step led (EditionProfile.faceTargetAfterMove)
        if (!attack.TryAttack(CurrentTarget) && Edition.Profile.faceTargetAfterMove) {
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
}
