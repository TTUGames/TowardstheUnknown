using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The panel of the HUD showing the name and stats of the hovered enemy, shared by all of them. It follows the entity
/// hovered on the board (<see cref="Room.EntityHovered"/>) and tells its <see cref="InfoEntity"/>, the previous one
/// first, so that only one enemy shows its info and threatened tiles at a time
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
    private InfoEntity hovered;

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
        GameEvents.CombatStarted += RefreshHovered;
        GameEvents.CombatEnded += RefreshHovered;
    }

    public void Dispose()
    {
        Room.EntityHovered -= OnEntityHovered;
        if (player != null) player.SelectedArtifactChanged -= OnSelectedArtifactChanged;
        GameEvents.CombatStarted -= RefreshHovered;
        GameEvents.CombatEnded -= RefreshHovered;
    }

    private void OnEntityHovered(TacticsMove entity)
    {
        InfoEntity next = entity != null && entity.TryGetComponent(out InfoEntity info) && info.enabled ? info : null;
        // The same enemy, now hovered on the board rather than from the timeline or back: its info shows or hides
        if (next == hovered)
        {
            RefreshHovered();
            return;
        }
        if (hovered != null) hovered.SetHovered(false);
        hovered = next;
        if (hovered != null) hovered.SetHovered(true);
    }

    // The threatened tiles are hidden while the player aims an artifact, and shown in combat only
    private void OnSelectedArtifactChanged(int artifact) => RefreshHovered();

    private void RefreshHovered()
    {
        if (hovered != null) hovered.Refresh();
    }

    /// <summary>
    /// Shows the panel above the entity if it is in the lower half of the screen, below it otherwise, never over its tile
    /// </summary>
    public void Show(Vector3 worldPosition, EntityStats entity, string entityName, int movementPoints)
    {
        Camera cam = Camera.main;
        Vector3 screenPosition = cam.WorldToScreenPoint(worldPosition);
        screenPosition.y += Screen.height * (screenPosition.y > Screen.height / 2f ? -OffsetBelow : OffsetAbove);
        Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(root.panel, new Vector2(screenPosition.x, Screen.height - screenPosition.y));
        Vector2 position = root.parent.WorldToLocal(panelPosition);
        root.style.left = position.x;
        root.style.top = position.y;

        nameLabel.text = entityName;
        health.text = string.Format(Localization.UI("EntityInfoHealth"), entity.CurrentHealth);
        movement.text = string.Format(Localization.UI("EntityInfoMovement"), movementPoints);
        armor.text = "+" + entity.Armor;
        armor.EnableInClassList("hidden", entity.Armor <= 0);
        var statuses = new System.Text.StringBuilder();
        foreach (StatusEffect status in entity.StatusEffects)
        {
            if (statuses.Length > 0) statuses.Append("   ");
            statuses.Append(Localization.UI("Status" + status.Data.name)).Append(" (").Append(status.Duration).Append(')');
        }
        effects.text = statuses.ToString();
        effects.EnableInClassList("hidden", statuses.Length == 0);
        root.AddToClassList("shown");
    }

    public void Hide() => root.RemoveFromClassList("shown");
}
