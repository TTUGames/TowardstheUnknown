using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// The entity an effect is applied to
/// </summary>
public enum EffectTarget { Target, Caster }

/// <summary>
/// A modular effect of an artifact or an enemy pattern. Applying it queues the matching actions.
/// Renaming a subclass breaks the assets using it, unless it gets a [MovedFrom] attribute.
/// </summary>
[System.Serializable]
public abstract class CombatEffect
{
    public abstract void Apply(EntityStats caster, EntityStats target);

    /// <summary>
    /// The named values available in the localized effect description, such as {minDamage}
    /// </summary>
    public abstract IEnumerable<(string name, object value)> DescriptionArguments { get; }

    protected static EntityStats Resolve(EffectTarget who, EntityStats caster, EntityStats target) => who == EffectTarget.Caster ? caster : target;
}

[System.Serializable]
public class DamageEffect : CombatEffect
{
    public EffectTarget on = EffectTarget.Target;
    [HorizontalGroup, MinValue(0)] public int minDamage;
    [HorizontalGroup, MinValue("minDamage")] public int maxDamage;
    [Tooltip("Goes through the armor, straight to the health")] public bool ignoreArmor;

    public override void Apply(EntityStats caster, EntityStats target) =>
        ActionManager.AddToBottom(new DamageAction(caster, Resolve(on, caster, target), minDamage, maxDamage, ignoreArmor));

    public override IEnumerable<(string, object)> DescriptionArguments => on == EffectTarget.Caster
        ? new (string, object)[] { ("minSelfDamage", minDamage), ("maxSelfDamage", maxDamage) }
        : new (string, object)[] { ("minDamage", minDamage), ("maxDamage", maxDamage) };
}

[System.Serializable]
public class ArmorEffect : CombatEffect
{
    public EffectTarget on = EffectTarget.Caster;
    [MinValue(0)] public int armor;

    public override void Apply(EntityStats caster, EntityStats target) =>
        ActionManager.AddToBottom(new ArmorAction(Resolve(on, caster, target), armor));

    public override IEnumerable<(string, object)> DescriptionArguments => new (string, object)[] { ("armor", armor) };
}

[System.Serializable]
public class HealEffect : CombatEffect
{
    public EffectTarget on = EffectTarget.Caster;
    [MinValue(0)] public int heal;

    public override void Apply(EntityStats caster, EntityStats target) =>
        ActionManager.AddToBottom(new HealAction(Resolve(on, caster, target), heal));

    public override IEnumerable<(string, object)> DescriptionArguments => new (string, object)[] { ("heal", heal) };
}

[System.Serializable]
public class StatModifierEffect : CombatEffect
{
    public EffectTarget on = EffectTarget.Target;
    [Required, AssetsOnly] public StatusEffectData status;
    [MinValue(1), SuffixLabel("turns")] public int duration = 1;

    public override void Apply(EntityStats caster, EntityStats target) =>
        ActionManager.AddToBottom(new ApplyStatusAction(Resolve(on, caster, target), status, duration));

    public override IEnumerable<(string, object)> DescriptionArguments => status == null
        ? new (string, object)[0]
        : new (string, object)[] { (status.DurationArgument, duration) };
}

/// <summary>
/// Moves an entity in a straight line relatively to the other one: towards it for a positive distance, away from it for a negative one
/// </summary>
[System.Serializable]
public class MoveEffect : CombatEffect
{
    [LabelText("Moved entity")] public EffectTarget moved = EffectTarget.Target;
    [InfoBox("Positive: towards the other entity. Negative: away from it.")] public int distance;

    public override void Apply(EntityStats caster, EntityStats target)
    {
        EntityStats movedEntity = Resolve(moved, caster, target);
        ActionManager.AddToBottom(new MoveTowardsAction(movedEntity, movedEntity == caster ? target : caster, distance));
    }

    public override IEnumerable<(string, object)> DescriptionArguments => new (string, object)[] { ("distance", Mathf.Abs(distance)) };
}
