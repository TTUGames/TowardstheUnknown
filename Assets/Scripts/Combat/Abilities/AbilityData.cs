using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// What an artifact and an enemy pattern share: what they target, their range and area, their effects and how they play
/// </summary>
public abstract class AbilityData : ScriptableObject
{
    [BoxGroup("Targeting"), FormerlySerializedAs("targetType")] public EntityType target = EntityType.ENEMY;
    [BoxGroup("Targeting")] public TileSearchConfig range = new TileSearchConfig(TileSearchConfig.Shape.CircleAttack, 1, 1);
    [BoxGroup("Targeting"), Tooltip("Hits every target in an area around the targeted tile instead of the targeted entity")] public bool isAreaOfEffect;
    [BoxGroup("Targeting"), ShowIf("isAreaOfEffect")] public TileSearchConfig area = new TileSearchConfig(TileSearchConfig.Shape.Circle, 0, 1);

    [BoxGroup("Effects"), Tooltip("Applied once, with the caster as target, whatever the number of targets")]
    [SerializeReference, ListDrawerSettings(ShowFoldout = false)] public List<CombatEffect> castEffects = new List<CombatEffect>();
    [BoxGroup("Effects"), Tooltip("Applied to each target, in order")]
    [SerializeReference, ListDrawerSettings(ShowFoldout = false)] public List<CombatEffect> effects = new List<CombatEffect>();

    [BoxGroup("Animation"), FormerlySerializedAs("animStateName"), Tooltip("Animator state played by the caster, none if empty")] public string animationState;
    [BoxGroup("Animation"), FormerlySerializedAs("attackDuration"), MinValue(0), SuffixLabel("s"), Tooltip("Time the other actions wait for")] public float duration = 2f;
    [BoxGroup("Animation"), MinValue(0), SuffixLabel("s"), Tooltip("From the start of the animation to the moment the effects apply: damage, hits, pushes. Clamped to the duration")] public float impactDelay = 0.5f;
    [BoxGroup("Animation")] public List<VFXInfo> vfx = new List<VFXInfo>();
    [BoxGroup("Animation"), Tooltip("Posted on the caster")] public AK.Wwise.Event sound = new AK.Wwise.Event();

    /// <summary>
    /// The prefabs of the VFX list
    /// </summary>
    public IEnumerable<GameObject> VFXPrefabs => vfx.Where(info => info != null && info.Prefab != null).Select(info => info.Prefab);

    /// <summary>
    /// The named values of the effects, available in the localized effect description
    /// </summary>
    public Dictionary<string, object> DescriptionArguments
    {
        get
        {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            foreach (CombatEffect effect in castEffects.Concat(effects).Where(effect => effect != null))
                foreach ((string name, object value) in effect.DescriptionArguments)
                    arguments[name] = value;
            return arguments;
        }
    }
}
