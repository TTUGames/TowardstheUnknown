using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime instance of an <c>ArtifactData</c>, holding its cooldown and remaining uses
/// </summary>
public class Artifact
{
    private readonly ArtifactData data;
    private readonly TileSearch range;
    private readonly TileSearch area;

    private int remainingUsesThisTurn;
    private int remainingCooldown;

    public Artifact(ArtifactData data)
    {
        this.data = data;
        range = data.range.Create();
        if (data.isAreaOfEffect) area = data.area.Create();

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

    private bool IsTargetable(TacticsMove entity) => entity != null && entity.CompareTag(data.target.ToString());

    /// <summary>
    /// Tells if a tile is valid to be targeted
    /// </summary>
    public bool CanTarget(Tile tile)
    {
        return data.isAreaOfEffect || IsTargetable(tile.GetEntity());
    }

    /// <summary>
    /// Gets the tiles targetted by the artifact
    /// </summary>
    public List<Tile> GetTargets(Tile targetedTile)
    {
        List<Tile> targetedTiles = new List<Tile>();
        if (targetedTile == null || targetedTile.Selection != Tile.SelectionType.ATTACK) return targetedTiles;
        if (data.isAreaOfEffect)
        {
            area.SetStartingTile(targetedTile);
            area.Search();
            return area.GetTiles();
        }
        if (CanTarget(targetedTile)) targetedTiles.Add(targetedTile);
        return targetedTiles;
    }

    /// <summary>
    /// Pays the artifact's costs, applies its effects on the targets and plays its animation
    /// </summary>
    /// <param name="source">The entity using the artifact</param>
    /// <param name="tile">The targeted tile</param>
    public void Launch(PlayerAttack source, Tile tile)
    {
        if (!CanTarget(tile)) return;
        ApplyCosts(source.Stats);

        foreach (CombatEffect effect in data.castEffects) effect.Apply(source.Stats, source.Stats);
        if (data.isAreaOfEffect)
        {
            foreach (Tile target in GetTargets(tile))
                if (IsTargetable(target.GetEntity())) ApplyEffects(source.Stats, target.GetEntity());
        }
        else ApplyEffects(source.Stats, tile.GetEntity());

        PlayAnimation(source.CurrentTile, tile, source);
    }

    private void ApplyEffects(PlayerStats caster, TacticsMove target)
    {
        EntityStats targetStats = target.GetComponent<EntityStats>();
        foreach (CombatEffect effect in data.effects) effect.Apply(caster, targetStats);
    }

    /// <summary>
    /// Plays the artifacts animation and vfx
    /// </summary>
    private void PlayAnimation(Tile sourceTile, Tile targetTile, PlayerAttack source)
    {
        if (sourceTile != targetTile) {
            float modelRotation = -Vector3.SignedAngle(targetTile.transform.position - sourceTile.transform.position, Vector3.forward, Vector3.up);
            source.transform.rotation = Quaternion.Euler(0, modelRotation, 0);
        }

        if (source.TryGetComponent(out Animator animator)) animator.Play(ID);

        WaitForAttackEndAction action = new WaitForAttackEndAction(data.attackDuration, source.gameObject);
        ActionManager.AddToBottom(action);

        foreach (VFXInfo vfxInfo in data.vfx)
            vfxInfo.Play(action, source.gameObject, targetTile);
    }

    /// <summary>
    /// Identifies the artifact in localization, animations and sounds
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
    public Sprite InventoryIcon => data.inventoryIcon;
    public TileSearch Range => range;
    public Color Color => data.playerColor;
    public WeaponEnum Weapon => data.weapon;
    public ArtifactRarity Rarity => data.rarity;
    public List<Vector2Int> Slots => data.shape;
}
