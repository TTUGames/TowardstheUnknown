using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Definition of a status effect: a change of a damage multiplier while it lasts.
/// Its name in camel case followed by "Turns" names its duration in the localized effect descriptions, such as {attackUpTurns}
/// </summary>
[CreateAssetMenu(fileName = "NewStatusEffect", menuName = "TTU/Status Effect")]
public class StatusEffectData : ScriptableObject
{
    public enum Stat { DamageDealt, DamageReceived }

    [Tooltip("The damage multiplier changed while the status lasts")] public Stat stat;
    [Tooltip("Added to the multiplier")] public float delta;
    [Tooltip("Good for its owner: shown as a buff in the HUD, else as a debuff")] public bool isBuff;
    [Tooltip("Applying this status on an entity having the opposite one cancels both"), AssetsOnly] public StatusEffectData opposite;

    public string DurationArgument => char.ToLower(name[0]) + name.Substring(1) + "Turns";
}
