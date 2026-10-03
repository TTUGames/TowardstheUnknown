using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Marks the tiles the hovered enemy can hit this turn (<see cref="Tile.IsThreat"/>), in combat while the player isn't aiming
/// an artifact (<see cref="EditionProfile.threatTiles"/>). One for the game, on <c>Gameplay</c>; it follows the hovered enemy
/// (<see cref="BoardPointer.EntityHovered"/>, the timeline's pointed enemy included) and its stats. While <c>ShowThreats</c> (Alt)
/// is held, the tiles every enemy can hit (<see cref="EditionProfile.allThreats"/>)
/// </summary>
public class ThreatTiles : MonoBehaviour
{
    private readonly List<Tile> threat = new();
    private EnemyStats hovered;
    private PlayerTurn player;
    // ShowThreats held: every enemy's threat
    private bool all;

    private void Start()
    {
        player = GameScene.Player;
        BoardPointer.EntityHovered += OnEntityHovered;
        player.SelectedArtifactChanged += OnSelectedArtifactChanged;
        GameEvents.CombatStarted += Refresh;
        GameEvents.CombatEnded += Refresh;
        GameEvents.EntityDied += OnEntityDied;
        GameInput.Controls.Gameplay.ShowThreats.performed += OnShowThreats;
        GameInput.Controls.Gameplay.ShowThreats.canceled += OnShowThreats;
        TurnSystem.Instance.TurnChanged += OnTurnChanged;
    }

    private void OnDestroy()
    {
        BoardPointer.EntityHovered -= OnEntityHovered;
        if (player != null) player.SelectedArtifactChanged -= OnSelectedArtifactChanged;
        GameEvents.CombatStarted -= Refresh;
        GameEvents.CombatEnded -= Refresh;
        GameEvents.EntityDied -= OnEntityDied;
        GameInput.Controls.Gameplay.ShowThreats.performed -= OnShowThreats;
        GameInput.Controls.Gameplay.ShowThreats.canceled -= OnShowThreats;
        if (TurnSystem.Instance != null) TurnSystem.Instance.TurnChanged -= OnTurnChanged;
        Watch(null);
        Hide();
    }

    private void OnEntityHovered(TacticsMove entity)
    {
        Watch(entity != null && entity.TryGetComponent(out EnemyStats stats) ? stats : null);
        Refresh();
    }

    private void OnEntityDied(EntityStats entity)
    {
        if (entity == hovered || all) Refresh();
    }

    private void OnShowThreats(InputAction.CallbackContext context)
    {
        all = context.performed && Edition.Profile.allThreats;
        Refresh();
    }

    // The enemies moved: every enemy's threat follows
    private void OnTurnChanged()
    {
        if (all) Refresh();
    }

    // The threatened tiles are hidden while the player aims an artifact
    private void OnSelectedArtifactChanged(int artifact) => Refresh();

    // Follows the stats of the hovered enemy only: its movement changes its reach
    private void Watch(EnemyStats enemy)
    {
        if (enemy == hovered) return;
        if (hovered is not null) hovered.StatsChanged -= Refresh;
        hovered = enemy;
        if (hovered != null) hovered.StatsChanged += Refresh;
    }

    private void Refresh()
    {
        Hide();
        //While the player aims an artifact, the targets and the damage preview are what matters
        if (GameScene.IsGameplayBlocked || !Edition.Profile.threatTiles || !TurnSystem.Instance.IsCombat || GameScene.Player.IsAttacking) return;
        if (all)
        {
            var union = new HashSet<Tile>();
            foreach (EntityTurn turn in TurnSystem.Instance.Turns)
                if (turn != null && turn.stats is EnemyStats enemy && !enemy.IsDead) union.UnionWith(enemy.GetComponent<EnemyAttack>().GetThreatenedTiles());
            threat.AddRange(union);
        }
        else if (hovered != null && !hovered.IsDead) threat.AddRange(hovered.GetComponent<EnemyAttack>().GetThreatenedTiles());
        foreach (Tile tile in threat) tile.IsThreat = true;
    }

    private void Hide()
    {
        foreach (Tile tile in threat)
            if (tile != null) tile.IsThreat = false;
        threat.Clear();
    }
}
