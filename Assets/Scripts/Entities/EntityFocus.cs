using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// What the game points at an entity for, which its visuals answer (<see cref="EntityRing"/>, <see cref="EnemyGlow"/>): its
/// turn in a combat (<see cref="TurnSystem.TurnChanged"/>), the pointer over it (<see cref="BoardPointer.EntityHovered"/>: its
/// tile, its model or its timeline item) and the selected artifact about to hit it (<see cref="PlayerAttack.TargetsPreviewed"/>).
/// None for a dead entity. Built by each visual, which listens while it is enabled
/// </summary>
public class EntityFocus
{
    private readonly EntityStats stats;
    private readonly EntityTurn turn;
    private readonly TacticsMove move;
    private PlayerAttack playerAttack;
    private bool active, hovered, targeted;

    public bool Active => active && Alive;
    public bool Hovered => hovered && Alive;
    public bool Targeted => targeted && Alive;

    private bool Alive => stats == null || !stats.IsDead;

    public EntityFocus(GameObject entity)
    {
        stats = entity.GetComponent<EntityStats>();
        turn = entity.GetComponent<EntityTurn>();
        move = entity.GetComponent<TacticsMove>();
    }

    /// <summary>
    /// Follows the game, from its state now: the turn and the hover may have come before
    /// </summary>
    public void Listen()
    {
        TurnSystem.Instance.TurnChanged += OnTurnChanged;
        BoardPointer.EntityHovered += OnEntityHovered;
        GameEvents.CombatEnded += OnCombatEnded;
        if (GameScene.Player != null && GameScene.Player.TryGetComponent(out playerAttack))
            playerAttack.TargetsPreviewed += OnTargetsPreviewed;
        OnTurnChanged();
        OnEntityHovered(BoardPointer.HoveredEntity);
    }

    public void Stop()
    {
        if (TurnSystem.Instance != null) TurnSystem.Instance.TurnChanged -= OnTurnChanged;
        BoardPointer.EntityHovered -= OnEntityHovered;
        GameEvents.CombatEnded -= OnCombatEnded;
        if (playerAttack != null) playerAttack.TargetsPreviewed -= OnTargetsPreviewed;
        playerAttack = null;
    }

    private void OnTurnChanged() => active = turn != null && TurnSystem.Instance.IsCurrentTurn(turn);

    private void OnCombatEnded()
    {
        active = false;
        targeted = false;
    }

    private void OnEntityHovered(TacticsMove entity) => hovered = entity != null && entity == move;

    private void OnTargetsPreviewed(Artifact artifact, IReadOnlyList<EntityStats> targets)
    {
        targeted = false;
        if (stats == null) return;
        foreach (EntityStats entity in targets)
            if (entity == stats) targeted = true;
    }
}
