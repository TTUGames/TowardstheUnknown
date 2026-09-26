using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The enemy's side of the hover: while <see cref="EntityInfoPanel"/> marks it hovered, shows its info in the panel,
/// following its stats, and, in combat while the player isn't attacking, the tiles it can hit this turn
/// </summary>
public class InfoEntity : MonoBehaviour
{
    private string entityName;
    private EnemyStats enemyStats;
    private EnemyAttack enemyAttack;
    private bool hovered;
    //The tiles it can hit this turn, shown while hovered
    private readonly List<Tile> threat = new();

    public void Start()
    {
        //Only the enemies show their info (the player of the test scenes carries one)
        if (!TryGetComponent(out enemyStats))
        {
            enabled = false;
            return;
        }
        enemyAttack = GetComponent<EnemyAttack>();
        entityName = Localization.Entity(enemyStats.ID);
        enemyStats.StatsChanged += Refresh;
    }

    /// <summary>
    /// Called by the panel when the pointer enters or leaves the enemy (its tile, its model or its timeline item)
    /// </summary>
    public void SetHovered(bool value)
    {
        hovered = value;
        if (!hovered) GameScene.UI.Hud.EntityInfo.Hide();
        Refresh();
    }

    /// <summary>
    /// Shows the info and the threatened tiles while hovered, hides them if the entity died or a menu opened
    /// </summary>
    public void Refresh()
    {
        HideThreat();
        if (!hovered) return;
        EntityInfoPanel panel = GameScene.UI.Hud.EntityInfo;
        if (GameScene.IsGameplayBlocked || enemyStats.IsDead)
        {
            panel.Hide();
            return;
        }
        panel.Show(transform.position, enemyStats, entityName, enemyStats.maxMovementPoints);
        //While the player aims an artifact, the targets and the damage preview are what matters
        if (TurnSystem.Instance.IsCombat && !GameScene.Player.IsAttacking)
        {
            threat.AddRange(enemyAttack.GetThreatenedTiles());
            foreach (Tile tile in threat) tile.IsThreat = true;
        }
    }

    private void HideThreat()
    {
        foreach (Tile tile in threat)
            if (tile != null) tile.IsThreat = false;
        threat.Clear();
    }

    private void OnDestroy()
    {
        HideThreat();
        if (enemyStats != null) enemyStats.StatsChanged -= Refresh;
    }
}
