using System.Collections.Generic;

/// <summary>
/// Runtime instance of an <c>ArtifactData</c>, holding its cooldown and uses
/// </summary>
public class DataArtifact : Artifact
{
    private readonly ArtifactData data;
    private TileSearch area;

    public DataArtifact(ArtifactData data) : base(data.name)
    {
        this.data = data;
        CompleteInit();
    }

    protected override void InitValues()
    {
        if (data.skillBarIcon != null) skillBarIcon = data.skillBarIcon;
        if (data.inventoryIcon != null) inventoryIcon = data.inventoryIcon;
        playerColor = data.playerColor;
        weapon = data.weapon;
        rarity = data.rarity;
        attackDuration = data.attackDuration;
        cost = data.cost;
        maximumUsePerTurn = data.maximumUsePerTurn;
        cooldown = data.cooldown;
        SetRange(data.range.Create(), data.range.min, data.range.max);
        if (data.isAreaOfEffect)
        {
            area = data.area.Create();
            minArea = data.area.min;
            maxArea = data.area.max;
        }
        vfxInfos.AddRange(data.vfx);
        slots = new List<UnityEngine.Vector2Int>(data.shape);
        targets.Add(data.target.ToString());
        effectDescription = string.Format(effectDescription ?? "", data.DescriptionValues);
    }

    public override bool CanTarget(Tile tile)
    {
        if (data.isAreaOfEffect) return true;
        TacticsMove target = tile.GetEntity();
        return target != null && targets.Contains(target.tag);
    }

    public override List<Tile> GetTargets(Tile targetedTile)
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

    public override void Launch(PlayerAttack source, Tile tile)
    {
        if (!CanTarget(tile)) return;
        ApplyCosts(source.Stats);

        foreach (CombatEffect effect in data.castEffects) effect.Apply(source.Stats, source.Stats);
        if (data.isAreaOfEffect)
        {
            foreach (Tile target in GetTargets(tile))
            {
                TacticsMove entity = target.GetEntity();
                if (entity != null && targets.Contains(entity.tag))
                    ApplyEffects(source.Stats, entity.GetComponent<EntityStats>());
            }
        }
        else ApplyEffects(source.Stats, tile.GetEntity().GetComponent<EntityStats>());

        PlayAnimation(source.CurrentTile, tile, source);
    }

    protected override void ApplyEffects(PlayerStats source, EntityStats target)
    {
        foreach (CombatEffect effect in data.effects) effect.Apply(source, target);
    }
}
