using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime instance of an <c>AbilityData</c>, holding its range and area searches
/// </summary>
public abstract class Ability
{
    protected readonly AbilityData data;
    private readonly TileSearch range;
    private readonly TileSearch area;
    // The range without its line of sight, for an attack needing one: what an obstacle hides
    private readonly TileSearch rangeInSightOrNot;

    protected Ability(AbilityData data)
    {
        this.data = data;
        range = data.range.Create();
        TileSearchConfig.Shape blind = data.range.shape switch {
            TileSearchConfig.Shape.CircleAttack => TileSearchConfig.Shape.Circle,
            TileSearchConfig.Shape.LineAttack => TileSearchConfig.Shape.Line,
            _ => data.range.shape,
        };
        if (blind != data.range.shape) rangeInSightOrNot = new TileSearchConfig(blind, data.range.min, data.range.max).Create();
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
    /// Adds to <paramref name="into"/> the tiles of the range from <paramref name="casterTile"/> that its line of sight can't
    /// reach: none for a range without one. Searches the range again: call it after reading <see cref="Range"/>
    /// </summary>
    public void GetOutOfSightTiles(Tile casterTile, List<Tile> into)
    {
        if (rangeInSightOrNot == null) return;
        range.SetStartingTile(casterTile);
        range.Search();
        rangeInSightOrNot.SetStartingTile(casterTile);
        rangeInSightOrNot.Search();
        foreach (Tile tile in rangeInSightOrNot.GetTiles())
            if (!range.Contains(tile)) into.Add(tile);
    }

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
    /// The armor and health the caster would gain from the cast hitting <paramref name="targets"/> entities: its armor and
    /// heal effects on itself (the cast effects once, the others once per target), the heal before its missing health caps it
    /// </summary>
    public (int armor, int heal) PreviewGains(EntityStats caster, int targets)
    {
        int armor = 0, heal = 0;
        void Count(CombatEffect effect, int times)
        {
            if (effect is ArmorEffect armorEffect && armorEffect.on == EffectTarget.Caster) armor += armorEffect.armor * times;
            else if (effect is HealEffect healEffect && healEffect.on == EffectTarget.Caster) heal += healEffect.heal * times;
        }
        foreach (CombatEffect effect in data.castEffects) Count(effect, 1);
        foreach (CombatEffect effect in data.effects) Count(effect, targets);
        return (armor, heal);
    }

    /// <summary>
    /// The health the caster would lose to its own damage effects (ExplosiveSacrifice, HitBuff), with its multipliers and,
    /// unless they go through it, its armor taking them first
    /// </summary>
    public (int min, int max) PreviewSelfDamage(EntityStats caster)
    {
        int min = 0, max = 0, armor = caster.Armor;
        float multiplier = caster.DamageDealtMultiplier * caster.DamageReceivedMultiplier;
        foreach (CombatEffect effect in data.castEffects)
        {
            if (effect is not DamageEffect damage) continue;
            // Same rounding as EntityStats.DamageTo
            int low = Mathf.CeilToInt(damage.minDamage * multiplier), high = Mathf.CeilToInt(damage.maxDamage * multiplier);
            if (!damage.ignoreArmor)
            {
                low = Mathf.Max(0, low - armor);
                high = Mathf.Max(0, high - armor);
            }
            min += low;
            max += high;
        }
        return (min, max);
    }

    // The latest cast's turn of each caster, so that an older pivot gives way to it
    private static readonly Dictionary<Transform, int> pivotTurns = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => pivotTurns.Clear();

    // Eases the caster's turn in and out over the seconds given, alongside the attack's start, unless a newer cast turns it
    private static System.Collections.IEnumerator Pivot(Transform caster, Quaternion facing, float seconds, int turn)
    {
        Quaternion from = caster.rotation;
        for (float time = 0; time < seconds; time += Time.deltaTime)
        {
            if (caster == null || pivotTurns[caster] != turn) yield break;
            caster.rotation = Quaternion.Slerp(from, facing, Mathf.SmoothStep(0, 1, time / seconds));
            yield return null;
        }
        if (caster != null && pivotTurns[caster] == turn) caster.rotation = facing;
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
            Quaternion facing = Quaternion.Euler(0, rotation, 0);
            //A short pivot over the start of the windup, which it doesn't delay (EditionProfile.attackPivot); the original snapped
            float pivot = Edition.Profile.attackPivot;
            //A newer cast takes the caster's turn over: the pivot under way stops
            int turn = pivotTurns.TryGetValue(caster.transform, out int last) ? last + 1 : 1;
            pivotTurns[caster.transform] = turn;
            if (pivot > 0 && Quaternion.Angle(caster.transform.rotation, facing) > 1) ActionManager.Run(Pivot(caster.transform, facing, pivot, turn));
            else caster.transform.rotation = facing;
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

/// <summary>
/// An <c>Ability</c> exposing its data asset with its own type, read once from the base
/// </summary>
public abstract class Ability<TData> : Ability where TData : AbilityData
{
    protected Ability(TData data) : base(data) { }

    /// <summary>
    /// The data asset the ability was made from
    /// </summary>
    public TData Data => (TData)data;
}
