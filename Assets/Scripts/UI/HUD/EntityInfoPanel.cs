using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The panel of the HUD showing the name and stats of the hovered enemy, shared by all of them. It follows the enemy
/// hovered on the board (<see cref="Room.EntityHovered"/>) and its stats and, in combat while the player isn't aiming
/// an artifact, marks the tiles the enemy can hit this turn
/// </summary>
public class EntityInfoPanel : System.IDisposable
{
    // Panel center, in screen height fractions from the entity's feet: above its model, or below its feet near the top of the screen
    private const float OffsetAbove = 0.2f;
    private const float OffsetBelow = 0.07f;

    private readonly VisualElement root;
    private readonly Label nameLabel;
    private readonly Label health;
    private readonly Label movement;
    private readonly Label armor;
    private readonly Label effects;
    private readonly PlayerTurn player;
    private EnemyStats hovered;
    // The tiles the hovered enemy can hit this turn
    private readonly List<Tile> threat = new();

    public EntityInfoPanel(VisualElement root, PlayerTurn player)
    {
        this.root = root;
        this.player = player;
        nameLabel = root.Q<Label>("EntityName");
        health = root.Q<Label>("EntityHealth");
        movement = root.Q<Label>("EntityMovement");
        armor = root.Q<Label>("EntityArmor");
        effects = root.Q<Label>("EntityEffects");
        Room.EntityHovered += OnEntityHovered;
        player.SelectedArtifactChanged += OnSelectedArtifactChanged;
        GameEvents.CombatStarted += Refresh;
        GameEvents.CombatEnded += Refresh;
    }

    public void Dispose()
    {
        Room.EntityHovered -= OnEntityHovered;
        if (player != null) player.SelectedArtifactChanged -= OnSelectedArtifactChanged;
        GameEvents.CombatStarted -= Refresh;
        GameEvents.CombatEnded -= Refresh;
        if (hovered is not null) hovered.StatsChanged -= Refresh;
        HideThreat();
    }

    // Also raised again when the same enemy goes from the timeline to the board or back: its info shows or hides
    private void OnEntityHovered(TacticsMove entity)
    {
        EnemyStats next = entity != null && entity.TryGetComponent(out EnemyStats stats) ? stats : null;
        if (next != hovered)
        {
            if (hovered is not null) hovered.StatsChanged -= Refresh;
            hovered = next;
            if (hovered != null) hovered.StatsChanged += Refresh;
        }
        Refresh();
    }

    // The threatened tiles are hidden while the player aims an artifact, and shown in combat only
    private void OnSelectedArtifactChanged(int artifact) => Refresh();

    /// <summary>
    /// Shows the info and the threatened tiles of the hovered enemy, hides them if none, if it died or if a menu opened.
    /// The info stays hidden while the timeline points at the enemy, whose tooltip shows the same
    /// </summary>
    private void Refresh()
    {
        HideThreat();
        bool shown = hovered != null && !hovered.IsDead && !GameScene.IsGameplayBlocked;
        if (shown && !Room.IsPointedFromUI) Show(hovered);
        else Hide();
        //While the player aims an artifact, the targets and the damage preview are what matters
        if (shown && TurnSystem.Instance.IsCombat && !GameScene.Player.IsAttacking)
        {
            threat.AddRange(hovered.GetComponent<EnemyAttack>().GetThreatenedTiles());
            foreach (Tile tile in threat) tile.IsThreat = true;
        }
    }

    private void HideThreat()
    {
        foreach (Tile tile in threat)
            if (tile != null) tile.IsThreat = false;
        threat.Clear();
    }

    /// <summary>
    /// Shows the panel above the enemy if it is in the lower half of the screen, below it otherwise, never over its tile
    /// </summary>
    private void Show(EnemyStats enemy)
    {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(enemy.transform.position);
        screenPosition.y += Screen.height * (screenPosition.y > Screen.height / 2f ? -OffsetBelow : OffsetAbove);
        Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(root.panel, new Vector2(screenPosition.x, Screen.height - screenPosition.y));
        Vector2 position = root.parent.WorldToLocal(panelPosition);
        root.style.left = position.x;
        root.style.top = position.y;

        nameLabel.text = Localization.Entity(enemy.ID);
        health.text = string.Format(Localization.UI("EntityInfoHealth"), enemy.CurrentHealth);
        movement.text = string.Format(Localization.UI("EntityInfoMovement"), enemy.maxMovementPoints);
        armor.text = "+" + enemy.Armor;
        armor.EnableInClassList("hidden", enemy.Armor <= 0);
        effects.text = HudTooltip.StatusLine(enemy);
        effects.EnableInClassList("hidden", effects.text.Length == 0);
        root.AddToClassList("shown");
    }

    private void Hide() => root.RemoveFromClassList("shown");
}
