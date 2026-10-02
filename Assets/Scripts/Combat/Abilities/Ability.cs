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

    /// <summary>
    /// Whether the ability hits an area around the targeted tile, rather than the entity on it
    /// </summary>
    public bool IsAreaOfEffect => data.isAreaOfEffect;

    public IEnumerable<GameObject> VFXPrefabs => data.VFXPrefabs;

    /// <summary>
    /// The kind of entity the ability hits
    /// </summary>
    public EntityType Target => data.target;

    protected bool IsTargetable(TacticsMove entity) => entity != null && entity.Stats.type == data.target;

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

    private readonly List<Tile> targetsBuffer = new List<Tile>();

    /// <summary>
    /// Gets the tiles hit when targeting a tile: the area around it, or the tile itself if its entity is a valid target.
    /// The list is reused by the next call (each hovered tile while aiming): read it at once
    /// </summary>
    public List<Tile> GetTargets(Tile targetedTile)
    {
        List<Tile> targetedTiles = targetsBuffer;
        targetedTiles.Clear();
        if (targetedTile == null) return targetedTiles;
        if (data.isAreaOfEffect)
        {
            area.SetStartingTile(targetedTile);
            area.Search();
            area.GetTiles(targetedTiles);
            return targetedTiles;
        }
        if (CanTarget(targetedTile)) targetedTiles.Add(targetedTile);
        return targetedTiles;
    }

    /// <summary>
    /// The damage the ability would deal to the target, before its armor: the damage effects on the target, with the multipliers of both,
    /// counting the status effects applied before them in the list (Puddle lowers the defense, then hits)
    /// </summary>
    public (int min, int max) PreviewDamage(EntityStats caster, EntityStats target)
    {
        int min = 0, max = 0;
        float dealt = caster.DamageDealtMultiplier, received = target.DamageReceivedMultiplier;
        List<StatusEffectData> applied = null;
        foreach (CombatEffect effect in data.effects)
        {
            if (effect is StatModifierEffect modifier && modifier.status != null)
            {
                EntityStats on = modifier.on == EffectTarget.Caster ? caster : target;
                applied ??= new List<StatusEffectData>();
                // Applied twice, a status only extends: it counts once
                if (applied.Contains(modifier.status)) continue;
                applied.Add(modifier.status);
                float change = on.ModifierChangeIfApplied(modifier.status);
                if (modifier.status.stat == StatusEffectData.Stat.DamageDealt && on == caster) dealt += change;
                else if (modifier.status.stat == StatusEffectData.Stat.DamageReceived && on == target) received += change;
            }
            else if (effect is DamageEffect damage && damage.on == EffectTarget.Target)
            {
                // Same rounding as EntityStats.DamageTo
                min += Mathf.CeilToInt(damage.minDamage * dealt * received);
                max += Mathf.CeilToInt(damage.maxDamage * dealt * received);
            }
        }
        return (min, max);
    }

    /// <summary>
    /// Turns the caster towards the tile and queues the animation, VFX and sound, then applies the effects on the caster and on each target at the impact:
    /// the strike, or the arrival of the projectile
    /// </summary>
    /// <param name="chained">Whether another cast follows, which cuts the recovery after <paramref name="chainedRecovery"/> seconds</param>
    public void Cast(EntityStats caster, Tile targetedTile, System.Func<bool> chained = null, float chainedRecovery = 0)
    {
        //The targets are the ones on the tiles when cast, even if they move before the impact
        List<EntityStats> targets = new List<EntityStats>();
        foreach (Tile tile in GetTargets(targetedTile))
        {
            TacticsMove target = tile.GetEntity();
            if (IsTargetable(target)) targets.Add(target.Stats);
        }

        Tile casterTile = caster.Tile;
        if (casterTile != targetedTile)
        {
            float rotation = -Vector3.SignedAngle(targetedTile.transform.position - casterTile.transform.position, Vector3.forward, Vector3.up);
            caster.transform.rotation = Quaternion.Euler(0, rotation, 0);
        }

        AttackAnimationAction attack = new AttackAnimationAction(caster.gameObject, targetedTile, Mathf.Min(data.impactDelay, data.duration), data);
        ActionManager.AddToBottom(attack);

        foreach (EntityStats target in targets)
            foreach (CombatEffect effect in data.effects) effect.Apply(caster, target);
        //Once, on the caster, after the targets: a buff it gains doesn't weigh on this cast's hits
        foreach (CombatEffect effect in data.castEffects) effect.Apply(caster, caster);

        ActionManager.AddToBottom(new AttackRecoveryAction(attack, data.duration, chained, chainedRecovery));
    }
}
