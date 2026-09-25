using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime instance of an <c>AbilityData</c>, holding its range and area searches
/// </summary>
public abstract class Ability
{
    private readonly AbilityData data;
    private readonly TileSearch range;
    private readonly TileSearch area;

    protected Ability(AbilityData data)
    {
        this.data = data;
        range = data.range.Create();
        if (data.isAreaOfEffect) area = data.area.Create();
    }

    public TileSearch Range => range;

    public IEnumerable<GameObject> VFXPrefabs => data.VFXPrefabs;

    /// <summary>
    /// The kind of entity the ability hits
    /// </summary>
    public EntityType Target => data.target;

    protected bool IsTargetable(TacticsMove entity) => entity != null && entity.GetComponent<EntityStats>().type == data.target;

    /// <summary>
    /// Tells if a tile of the range is valid to be targeted
    /// </summary>
    public bool CanTarget(Tile tile) => data.isAreaOfEffect || IsTargetable(tile.GetEntity());

    /// <summary>
    /// Tells if the targeted tile is in range from the caster's tile
    /// </summary>
    public bool CanReach(Tile casterTile, Tile targetedTile)
    {
        range.SetStartingTile(casterTile);
        range.Search();
        return range.Contains(targetedTile);
    }

    /// <summary>
    /// Gets the tiles hit when targeting a tile: the area around it, or the tile itself if its entity is a valid target
    /// </summary>
    public List<Tile> GetTargets(Tile targetedTile)
    {
        List<Tile> targetedTiles = new List<Tile>();
        if (targetedTile == null) return targetedTiles;
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
    /// Turns the caster towards the tile and plays the animation, VFX and sound, then applies the effects on the caster and on each target at the impact
    /// </summary>
    public void Cast(EntityStats caster, Tile targetedTile)
    {
        //The targets are the ones on the tiles when cast, even if they move before the impact
        List<EntityStats> targets = new List<EntityStats>();
        foreach (Tile tile in GetTargets(targetedTile))
        {
            TacticsMove target = tile.GetEntity();
            if (IsTargetable(target)) targets.Add(target.GetComponent<EntityStats>());
        }

        Tile casterTile = caster.GetComponent<TacticsMove>().CurrentTile;
        if (casterTile != targetedTile)
        {
            float rotation = -Vector3.SignedAngle(targetedTile.transform.position - casterTile.transform.position, Vector3.forward, Vector3.up);
            caster.transform.rotation = Quaternion.Euler(0, rotation, 0);
        }

        float impactDelay = Mathf.Min(data.impactDelay, data.duration);
        AttackAnimationAction attack = new AttackAnimationAction(caster.gameObject, targetedTile, impactDelay, data.animationState, data.vfx);
        ActionManager.AddToBottom(attack);
        data.sound.Post(caster.gameObject);

        foreach (CombatEffect effect in data.castEffects) effect.Apply(caster, caster);
        foreach (EntityStats target in targets)
            foreach (CombatEffect effect in data.effects) effect.Apply(caster, target);

        ActionManager.AddToBottom(new AttackRecoveryAction(attack, data.duration - impactDelay));
    }
}
