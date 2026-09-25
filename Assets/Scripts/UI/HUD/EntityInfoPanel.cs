using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The panel of the HUD showing the name and stats of the hovered enemy, shared by all of them
/// </summary>
public class EntityInfoPanel
{
    // In screen height fractions, from the entity
    private const float OffsetAbove = 0.02f;
    private const float OffsetBelow = 0.2f;

    private readonly VisualElement root;
    private readonly Label nameLabel;
    private readonly Label health;
    private readonly Label movement;
    private readonly Label armor;
    private readonly Label effects;

    public EntityInfoPanel(VisualElement root)
    {
        this.root = root;
        nameLabel = root.Q<Label>("EntityName");
        health = root.Q<Label>("EntityHealth");
        movement = root.Q<Label>("EntityMovement");
        armor = root.Q<Label>("EntityArmor");
        effects = root.Q<Label>("EntityEffects");
    }

    /// <summary>
    /// Shows the panel above the entity if it is in the lower half of the screen, below it otherwise
    /// </summary>
    public void Show(Vector3 worldPosition, EntityStats entity, string entityName, int movementPoints)
    {
        Camera cam = Camera.main;
        Vector3 screenPosition = cam.WorldToScreenPoint(worldPosition);
        screenPosition.y += Screen.height * (screenPosition.y > Screen.height / 2f ? OffsetAbove : OffsetBelow);
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
