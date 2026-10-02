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

    [BoxGroup("Effects"), Tooltip("Applied once, with the caster as target, whatever the number of targets, after the targets' effects: every effect on the caster goes here")]
    [SerializeReference, ListDrawerSettings(ShowFoldout = false)] public List<CombatEffect> castEffects = new List<CombatEffect>();
    [BoxGroup("Effects"), Tooltip("Applied to each target, in order")]
    [SerializeReference, ListDrawerSettings(ShowFoldout = false)] public List<CombatEffect> effects = new List<CombatEffect>();

    [BoxGroup("Animation"), Tooltip("Played by the caster, none if empty")] public AnimationClip animationClip;
    [BoxGroup("Animation"), ShowIf("animationClip"), Tooltip("Played after the clip, as its second part, none if empty")] public AnimationClip followUpClip;
    [BoxGroup("Animation"), ShowIf("animationClip"), MinValue(0.05f), Tooltip("Speed of the clips")] public float animationSpeed = 1;
    [BoxGroup("Animation"), FormerlySerializedAs("attackDuration"), MinValue(0), SuffixLabel("s"), Tooltip("Time the other actions wait for")] public float duration = 1.2f;
    [BoxGroup("Animation"), MinValue(0), SuffixLabel("s"), Tooltip("How long the VFX play from the start of the attack, even once the other actions stopped waiting for it; they are removed then, or at the end of the duration if later")] public float vfxDuration = 2f;
    [BoxGroup("Animation"), MinValue(0), SuffixLabel("s"), Tooltip("From the start of the animation to the strike: the moment the effects apply (damage, hits, pushes), or the projectile leaves. Clamped to the duration")] public float impactDelay = 0.5f;
    [BoxGroup("Animation"), ShowIf("animationClip"), InlineProperty, Tooltip("How the clip plays in time: the impact and the VFX delays move with it")] public AttackTiming timing = new AttackTiming();
    [BoxGroup("Animation"), ShowIf("animationClip"), Range(0, 1), Tooltip("How much the legs follow the clip, a humanoid's upper body always does: 0 keeps the stance's (no step, no sliding feet), 1 plays the whole body")] public float legs = 1;
    [BoxGroup("Animation"), Tooltip("Shot at the strike: the effects apply at its arrival")] public ProjectileInfo projectile = new ProjectileInfo();
    [BoxGroup("Animation")] public List<VFXInfo> vfx = new List<VFXInfo>();
    [BoxGroup("Animation"), Tooltip("Posted on the caster when the attack starts")] public AK.Wwise.Event sound = new AK.Wwise.Event();
    [BoxGroup("Animation"), MinValue(0), SuffixLabel("s"), Tooltip("Real seconds from the start of the attack to the sound, in an edition with attackSoundDelay (not the Classic, whose sounds carry their own delay)")] public float soundDelay;

    /// <summary>
    /// The timing of an attack starting now, for the clip the edition plays: linear when the edition plays none. Without a clip
    /// (none set, or none in the Classic), it runs over the duration: nothing plays, but the impact, the VFX delays and the recovery follow it
    /// </summary>
    public AttackClock Clock()
    {
        AnimationClip clip = GameAssets.Instance.classicSkin.Current(animationClip);
        if (clip == null) return timing.Clock(impactDelay, duration, Edition.Profile.attackTiming);
        return timing.Clock(impactDelay, clip.length / Mathf.Max(0.05f, animationSpeed), Edition.Profile.attackTiming);
    }

    /// <summary>
    /// Real seconds from the start of the attack to its sound, in the edition shown
    /// </summary>
    public float SoundDelay => Edition.Profile.attackSoundDelay ? soundDelay : 0;

    /// <summary>
    /// Whether the caster's blade leaves a trail during the swing (<see cref="WeaponTrail"/>)
    /// </summary>
    public virtual bool SwingsBlade => false;

    /// <summary>
    /// The attack's color (a trail's), clear for none
    /// </summary>
    public virtual Color AttackColor => Color.clear;

    /// <summary>
    /// The swing in real seconds from the start of the attack, as the clock plays it: from the end of the held pose to
    /// the strike, or the last moments before the impact without a timing
    /// </summary>
    public (float start, float end) SwingWindow(AttackClock clock)
    {
        if (timing.enabled && clock != AttackClock.Linear)
            return (clock.TimeAt(timing.swingStart) + timing.windupHold, clock.TimeAt(timing.strike > 0 ? timing.strike : impactDelay) + SwingTrailTail);
        float impact = clock.EventTime(impactDelay);
        return (Mathf.Max(0, impact - 0.2f), impact + SwingTrailTail);
    }

    // The trail goes on a little after the strike, the blade still moving into its held pose
    private const float SwingTrailTail = 0.04f;

    /// <summary>
    /// The prefabs of the VFX list and of the projectile
    /// </summary>
    public IEnumerable<GameObject> VFXPrefabs => vfx.Where(info => info != null).Select(info => info.Prefab)
        .Append(projectile?.Prefab).Where(prefab => prefab != null);

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
