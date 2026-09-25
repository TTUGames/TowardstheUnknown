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

    public EntityInfoPanel(VisualElement root)
    {
        this.root = root;
        nameLabel = root.Q<Label>("EntityName");
        health = root.Q<Label>("EntityHealth");
        movement = root.Q<Label>("EntityMovement");
    }

    /// <summary>
    /// Shows the panel above the entity if it is in the lower half of the screen, below it otherwise
    /// </summary>
    public void Show(Vector3 worldPosition, string entityName, int healthPoints, int movementPoints)
    {
        Camera cam = Camera.main;
        Vector3 screenPosition = cam.WorldToScreenPoint(worldPosition);
        screenPosition.y += Screen.height * (screenPosition.y > Screen.height / 2f ? OffsetAbove : OffsetBelow);
        Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(root.panel, new Vector2(screenPosition.x, Screen.height - screenPosition.y));
        root.style.left = panelPosition.x;
        root.style.top = panelPosition.y;

        nameLabel.text = entityName;
        health.text = string.Format(Localization.UI("EntityInfoHealth"), healthPoints);
        movement.text = string.Format(Localization.UI("EntityInfoMovement"), movementPoints);
        root.AddToClassList("shown");
    }

    public void Hide() => root.RemoveFromClassList("shown");
}
