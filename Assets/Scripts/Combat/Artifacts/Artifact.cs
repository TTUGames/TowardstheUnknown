using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime instance of an <c>ArtifactData</c>, holding its cooldown and remaining uses
/// </summary>
public class Artifact : Ability
{
    private readonly ArtifactData data;

    private int remainingUsesThisTurn;
    private int remainingCooldown;

    public Artifact(ArtifactData data) : base(data)
    {
        this.data = data;

        Title = Localization.Artifact(ID, "Title");
        Description = Localization.Artifact(ID, "Description");
        EffectDescription = Localization.Artifact(ID, "Effects", data.DescriptionArguments);
        RangeDescription = Localization.Artifact(ID, "Range", RangeArguments(data));
        CooldownDescription = Localization.Artifact(ID, "Cooldown", new Dictionary<string, object> {
            ["value"] = data.cooldown == 0 ? data.maximumUsePerTurn : data.cooldown - 1 });

        TurnStart(); //Inits values to avoid greying the artifact in the skillbar
    }

    /// <summary>
    /// The named values available in the localized range description
    /// </summary>
    public static Dictionary<string, object> RangeArguments(ArtifactData data) => new Dictionary<string, object>
    {
        ["minRange"] = data.range.min,
        ["maxRange"] = data.range.max,
        ["minArea"] = data.isAreaOfEffect ? data.area.min : 0,
        ["maxArea"] = data.isAreaOfEffect ? data.area.max : 0,
    };

    /// <summary>
    /// Applies energy cost and cast restrictions such as cooldown and max uses per turn
    /// </summary>
    /// <param name="source">The player entity that cast the artifact</param>
    private void ApplyCosts(PlayerStats source)
    {
        --remainingUsesThisTurn;
        if (remainingUsesThisTurn == 0 && remainingCooldown == 0)
            remainingCooldown = data.cooldown;
        source.UseEnergy(data.cost); //Last, as it refreshes the skills bar
    }

    /// <summary>
    /// Tells if the artifact can be cast by the source entity
    /// </summary>
    public bool CanUse(PlayerStats source)
    {
        return source.CurrentEnergy >= data.cost && remainingCooldown == 0 && (data.maximumUsePerTurn == 0 || remainingUsesThisTurn > 0);
    }

    /// <summary>
    /// Applies start of combat effects to the artifact
    /// </summary>
    public void ResetConstraints()
    {
        remainingCooldown = 0;
        remainingUsesThisTurn = data.maximumUsePerTurn;
    }

    /// <summary>
    /// Applies start of turn effects to the artifact
    /// </summary>
    public void TurnStart()
    {
        if (remainingCooldown > 0)
            --remainingCooldown;
        remainingUsesThisTurn = data.maximumUsePerTurn;
    }

    /// <summary>
    /// Pays the artifact's costs and casts it on the targeted tile
    /// </summary>
    /// <param name="source">The player using the artifact</param>
    /// <param name="tile">The targeted tile</param>
    public void Launch(PlayerStats source, Tile tile)
    {
        if (!CanTarget(tile)) return;
        ApplyCosts(source);
        Cast(source, tile);
    }

    /// <summary>
    /// Identifies the artifact in localization
    /// </summary>
    public string ID => data.name;
    public string Title { get; }
    public string Description { get; }
    public string EffectDescription { get; }
    public string RangeDescription { get; }
    public string CooldownDescription { get; }
    public int Cost => data.cost;
    public int Cooldown => data.cooldown;
    public int RemainingCooldown => remainingCooldown;
    public Sprite SkillBarIcon => data.skillBarIcon;
    public ArtifactIconFit InventoryIconFit => data.inventoryIconFit;
    public float InventoryIconScale => data.inventoryIconScale;
    public int InventoryIconRotation => data.inventoryIconRotation;
    public Vector2 InventoryIconOffset => data.inventoryIconOffset;
    public Rect InventoryIconBounds => data.inventoryIconBounds;
    public Color Color => data.playerColor;
    public WeaponEnum Weapon => data.weapon;
    public ArtifactRarity Rarity => data.rarity;
    public List<Vector2Int> Slots => data.shape;
}
