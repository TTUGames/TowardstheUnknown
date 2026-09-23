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
    /// The values inserted in the localized effect description, in order
    /// </summary>
    public abstract IEnumerable<object> DescriptionValues { get; }

    protected static EntityStats Resolve(EffectTarget who, EntityStats caster, EntityStats target) => who == EffectTarget.Caster ? caster : target;
}

[System.Serializable]
public class DamageEffect : CombatEffect
{
    public EffectTarget on = EffectTarget.Target;
    [HorizontalGroup, MinValue(0)] public int minDamage;
    [HorizontalGroup, MinValue("minDamage")] public int maxDamage;

    public override void Apply(EntityStats caster, EntityStats target) =>
        ActionManager.AddToBottom(new DamageAction(caster, Resolve(on, caster, target), minDamage, maxDamage));

    public override IEnumerable<object> DescriptionValues => new object[] { minDamage, maxDamage };
}

[System.Serializable]
public class ArmorEffect : CombatEffect
{
    public EffectTarget on = EffectTarget.Caster;
    [MinValue(0)] public int armor;

    public override void Apply(EntityStats caster, EntityStats target) =>
        ActionManager.AddToBottom(new ArmorAction(Resolve(on, caster, target), armor));

    public override IEnumerable<object> DescriptionValues => new object[] { armor };
}

[System.Serializable]
public class HealEffect : CombatEffect
{
    public EffectTarget on = EffectTarget.Caster;
    [MinValue(0)] public int heal;

    public override void Apply(EntityStats caster, EntityStats target) =>
        ActionManager.AddToBottom(new HealAction(Resolve(on, caster, target), heal));

    public override IEnumerable<object> DescriptionValues => new object[] { heal };
}

[System.Serializable]
public class StatModifierEffect : CombatEffect
{
    public EffectTarget on = EffectTarget.Target;
    public StatModifierType modifier;
    [MinValue(1), SuffixLabel("turns")] public int duration = 1;

    public override void Apply(EntityStats caster, EntityStats target) =>
        ActionManager.AddToBottom(new ApplyStatusAction(Resolve(on, caster, target), StatModifierFactory.Create(modifier, duration)));

    public override IEnumerable<object> DescriptionValues => new object[] { duration };
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

    public override IEnumerable<object> DescriptionValues => new object[] { Mathf.Abs(distance) };
}
