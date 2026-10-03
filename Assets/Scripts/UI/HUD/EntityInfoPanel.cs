using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The panel of the HUD showing the name and stats of the hovered enemy, shared by all of them. It follows the enemy
/// hovered on the board (<see cref="BoardPointer.EntityHovered"/>) and its stats (<see cref="ThreatTiles"/> marks the tiles it can
/// hit). With <see cref="EditionProfile.infoOnHit"/>, an enemy taking
/// damage shows its info until the hover changes away from it or it dies, as the original's
/// </summary>
public class EntityInfoPanel : System.IDisposable
{
    // Panel center, in screen height fractions from the entity's feet: above its model, or below its feet near the top of the screen
    private const float OffsetAbove = 0.2f;
    // The original's panel sat a little right of the enemy: 0.02 of the width left, then its background 41.1 points right
    private const float OriginalShiftX = 2.7f;
    // Between the original's offset above the feet and the panel's center, in screen heights
    private const float OriginalCenterOffset = 88.7f / 1080f;

    private readonly VisualElement root;
    private readonly Label nameLabel;
    private readonly Label health;
    private readonly Label movement;
    private readonly Label armor;
    private readonly Label effects;
    private EnemyStats hovered;
    // The enemy last hit, shown over the hovered one until the hover changes away from it (EditionProfile.infoOnHit)
    private EnemyStats pinned;
    // The shown enemy, whose stats the panel follows
    private EnemyStats watched;

    public EntityInfoPanel(VisualElement root)
    {
        this.root = root;
        nameLabel = root.Q<Label>("EntityName");
        health = root.Q<Label>("EntityHealth");
        movement = root.Q<Label>("EntityMovement");
        armor = root.Q<Label>("EntityArmor");
        effects = root.Q<Label>("EntityEffects");
        BoardPointer.EntityHovered += OnEntityHovered;
        GameEvents.DamageTaken += OnDamageTaken;
        GameEvents.EntityDied += OnEntityDied;
    }

    public void Dispose()
    {
        BoardPointer.EntityHovered -= OnEntityHovered;
        GameEvents.DamageTaken -= OnDamageTaken;
        GameEvents.EntityDied -= OnEntityDied;
        if (watched is not null) watched.StatsChanged -= Refresh;
    }

    // Also raised again when the same enemy goes from the timeline to the board or back: its info shows or hides
    private void OnEntityHovered(TacticsMove entity)
    {
        hovered = entity != null && entity.TryGetComponent(out EnemyStats stats) ? stats : null;
        if (hovered != pinned) pinned = null;
        Refresh();
    }

    // The original showed a hit enemy's info with its new health
    private void OnDamageTaken(EntityStats entity, int damage, int healthLost)
    {
        if (!Edition.Profile.infoOnHit || entity is not EnemyStats enemy || enemy.IsDead) return;
        pinned = enemy;
        Refresh();
    }

    private void OnEntityDied(EntityStats entity)
    {
        if (entity != pinned) return;
        pinned = null;
        Refresh();
    }

    private EnemyStats Shown => pinned != null ? pinned : hovered;

    // Follows the stats of the shown enemy only
    private void Watch(EnemyStats shown)
    {
        if (shown == watched) return;
        if (watched is not null) watched.StatsChanged -= Refresh;
        watched = shown;
        if (watched != null) watched.StatsChanged += Refresh;
    }

    /// <summary>
    /// Shows the info of the shown enemy (the one hit, or else the hovered one), hides it if none, if it died or if a menu
    /// opened. The info stays hidden while the timeline points at the enemy, whose tooltip
    /// shows the same
    /// </summary>
    private void Refresh()
    {
        EnemyStats enemy = Shown;
        Watch(enemy);
        bool shown = enemy != null && !enemy.IsDead && !GameScene.IsGameplayBlocked;
        if (shown && !BoardPointer.IsPointedFromUI) Show(enemy);
        else Hide();
    }

    /// <summary>
    /// Shows the panel above the enemy if it is in the lower half of the screen, below it otherwise, never over its tile.
    /// Above, the original's placement takes each enemy's height
    /// </summary>
    private void Show(EnemyStats enemy)
    {
        EditionProfile profile = Edition.Profile;
        float above = profile.entityInfoOriginalAbove ? enemy.Data.classicInfoOffset - OriginalCenterOffset : OffsetAbove;
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(enemy.transform.position);
        screenPosition.y += Screen.height * (screenPosition.y > Screen.height / 2f ? -profile.entityInfoBelow : above);
        Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(root.panel, new Vector2(screenPosition.x, Screen.height - screenPosition.y));
        Vector2 position = root.parent.WorldToLocal(panelPosition);
        if (profile.entityInfoOriginalAbove) position.x += OriginalShiftX;
        root.style.left = position.x;
        root.style.top = position.y;

        string name = Localization.Entity(enemy.ID);
        // The original's name was in capitals (its TMP style); USS has no text transform
        nameLabel.text = profile.entityInfoCaps ? name.ToUpper() : name;
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
