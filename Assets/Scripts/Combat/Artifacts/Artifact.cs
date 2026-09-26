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
    //Whether the last use this turn started the cooldown, which a refund cancels
    private bool cooldownStarted;

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
    /// Pays the energy cost and the cast restrictions such as cooldown and max uses per turn, before casting it now or later
    /// </summary>
    /// <param name="source">The player entity that casts the artifact</param>
    public void Pay(PlayerStats source)
    {
        --remainingUsesThisTurn;
        if (remainingUsesThisTurn == 0 && remainingCooldown == 0)
        {
            remainingCooldown = data.cooldown;
            cooldownStarted = true;
        }
        source.UseEnergy(data.cost); //Last, as it refreshes the skills bar
    }

    /// <summary>
    /// Gives back what <c>Pay</c> took, for a queued cast dropped before being launched
    /// </summary>
    public void Refund(PlayerStats source)
    {
        ++remainingUsesThisTurn;
        if (cooldownStarted)
        {
            remainingCooldown = 0;
            cooldownStarted = false;
        }
        source.RefundEnergy(data.cost); //Last, as it refreshes the skills bar
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
        cooldownStarted = false;
    }

    /// <summary>
    /// Applies start of turn effects to the artifact
    /// </summary>
    public void TurnStart()
    {
        if (remainingCooldown > 0)
            --remainingCooldown;
        remainingUsesThisTurn = data.maximumUsePerTurn;
        cooldownStarted = false;
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
