using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Drives an entity's animator, an override of <c>Entity.controller</c> giving its locomotion, hit and death clips.
/// Its four layers play over each other: the locomotion, the attacks, the attacks again on the upper body only (<c>UpperBody.mask</c>),
/// then the reactions to hits and death. An attack plays the same slot in both attack layers: the whole body one at the weight of its
/// legs, the upper body one at full weight, so that a humanoid's legs blend between the stance and the attack while its upper body plays it.
/// The attack and reaction layers weigh nothing while they play nothing (an empty state would override a humanoid's pose): their weight blends in and out.
/// The attacks bring their own clips, played in two alternating slots so that an attack blends into the next one, even the same.
/// The layers, states and parameters named here are the contract of <c>Entity.controller</c>.
/// Every clip goes through the <see cref="EditionSkin"/>: an Anniversary clip paired with the original's plays the original in the Classic.
/// </summary>
[RequireComponent(typeof(Animator))]
public class EntityAnimator : MonoBehaviour
{
    private const int ActionLayer = 1;
    private const int UpperActionLayer = 2;
    private const int ReactionLayer = 3;
    private const int LayerCount = 4;
    private static readonly int Walking = Animator.StringToHash("Walking");
    private static readonly int Running = Animator.StringToHash("Running");
    private static readonly int WalkSpeed = Animator.StringToHash("WalkSpeed");
    private static readonly int RunSpeed = Animator.StringToHash("RunSpeed");
    private static readonly int[] AttackStates = { Animator.StringToHash("AttackA"), Animator.StringToHash("AttackB") };
    // The normalized time of the attack states (Motion Time), set at each frame by the attack's clock
    private static readonly int[] AttackTimes = { Animator.StringToHash("AttackTimeA"), Animator.StringToHash("AttackTimeB") };
    private static readonly int HitNone = Animator.StringToHash("HitNone");
    private static readonly int HitSmall = Animator.StringToHash("HitSmall");
    private static readonly int HitRegular = Animator.StringToHash("HitRegular");
    private static readonly int HitCritical = Animator.StringToHash("HitCritical");
    private static readonly int Death = Animator.StringToHash("Death");

    [SerializeField, Required, Tooltip("The placeholder clips of the AttackA and AttackB states of Entity.controller, replaced by each attack's clips")]
    private AnimationClip[] attackSlots = new AnimationClip[2];

    [BoxGroup("Locomotion"), SerializeField, MinValue(0.05f), Tooltip("Speed of the walk clip")] private float walkSpeed = 1;
    [BoxGroup("Locomotion"), SerializeField, MinValue(0.05f), Tooltip("Speed of the run clip")] private float runSpeed = 1;

    [BoxGroup("Attacks"), SerializeField, MinValue(0), SuffixLabel("s"), Tooltip("Blend from the locomotion into an attack")] private float attackFade = 0.12f;
    [BoxGroup("Attacks"), SerializeField, MinValue(0), SuffixLabel("s"), Tooltip("Blend from an attack into the next one, when they are chained")] private float chainedAttackFade = 0.05f;
    [BoxGroup("Attacks"), SerializeField, MinValue(0), SuffixLabel("s"), Tooltip("Blend from an attack into its follow-up clip, ending with the first clip")] private float followUpFade = 0.25f;
    [BoxGroup("Attacks"), SerializeField, MinValue(0), SuffixLabel("s"), Tooltip("Blend back to the locomotion, ending with the attack's clip")] private float attackFadeOut = 0.25f;

    [BoxGroup("Reactions"), SerializeField, MinValue(0), SuffixLabel("s"), Tooltip("Blend into a hit")] private float hitFade = 0.08f;
    [BoxGroup("Reactions"), SerializeField, MinValue(0), SuffixLabel("s"), Tooltip("Blend back from a hit, ending with its clip")] private float hitFadeOut = 0.25f;

    // The edition's blends, when its profile sets them (the original cut into the attacks and blended into the hits in 0.25 s)
    private float AttackFade => Edition.Profile.attackBlendIn >= 0 ? Edition.Profile.attackBlendIn : attackFade;
    private float HitFade => Edition.Profile.hitBlendIn >= 0 ? Edition.Profile.hitBlendIn : hitFade;
    [BoxGroup("Reactions"), SerializeField, MinValue(0), SuffixLabel("s"), Tooltip("Blend into the death")] private float deathFade = 0.25f;
    [BoxGroup("Reactions"), SerializeField, MinValue(1), Tooltip("From this health lost, the hit plays the regular clip rather than the small one")] private int regularHitDamage = 25;
    [BoxGroup("Reactions"), SerializeField, MinValue(1), Tooltip("From this health lost, the hit plays the critical clip")] private int criticalHitDamage = 40;

    private Animator animator;
    private AnimatorOverrideController overrides;
    // The entity's clips over those of Entity.controller, as its override gives them: the Anniversary's, the edition resolves them
    private readonly List<KeyValuePair<AnimationClip, AnimationClip>> entityClips = new();
    // The slot of the last attack, the next one takes the other
    private int slot;
    private Coroutine attack;
    // The attack's action ended: the rest of its clip is its recovery
    private bool recovering;
    // Weight of the whole body attack layer while an attack plays: how much its legs follow the clip
    private float legsWeight = 1;
    private Coroutine reaction;
    private bool dead;
    private readonly Coroutine[] weightFades = new Coroutine[LayerCount];

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (attackSlots.Length != 2 || attackSlots[0] == null || attackSlots[1] == null)
            Debug.LogError($"{name}: the attack slots of its EntityAnimator are not set, its attacks will not play", this);
        //An instance per entity, whose attacks replace the clips of the slots. It wraps the base controller without the entity's overrides: copy them
        var entityOverrides = animator.runtimeAnimatorController as AnimatorOverrideController;
        overrides = new AnimatorOverrideController(animator.runtimeAnimatorController);
        (entityOverrides != null ? entityOverrides : overrides).GetOverrides(entityClips);
        animator.runtimeAnimatorController = overrides;
        ApplyClips();
        ApplySpeeds();
    }

    // The clips and the walk clip's speed follow the edition
    private void OnEnable() => Edition.Changed += OnEditionChanged;

    private void OnDisable() => Edition.Changed -= OnEditionChanged;

    private void OnEditionChanged(GameEdition edition)
    {
        if (animator == null) return;
        ApplyClips();
        ApplySpeeds();
    }

    /// <summary>
    /// Gives the states the edition's clips: the entity's, or the base controller's where it overrides none
    /// </summary>
    private void ApplyClips()
    {
        EditionSkin skin = GameAssets.Instance.classicSkin;
        var clips = new List<KeyValuePair<AnimationClip, AnimationClip>>(entityClips.Count);
        foreach (KeyValuePair<AnimationClip, AnimationClip> pair in entityClips)
        {
            // The attack slots hold the attack playing
            if (System.Array.IndexOf(attackSlots, pair.Key) >= 0) continue;
            //A state can't play nothing: a clip without a Classic one gives back the base controller's
            AnimationClip clip = skin.Current(pair.Value != null ? pair.Value : pair.Key);
            clips.Add(new KeyValuePair<AnimationClip, AnimationClip>(pair.Key, clip == pair.Key ? null : clip));
        }
        overrides.ApplyOverrides(clips);
    }

    private void ApplySpeeds()
    {
        animator.SetFloat(WalkSpeed, walkSpeed * Edition.Profile.walkClipSpeed);
        animator.SetFloat(RunSpeed, runSpeed);
    }

    /// <summary>
    /// Changes the model's avatar, keeping the parameters the rebind resets
    /// </summary>
    public void SetAvatar(Avatar avatar)
    {
        animator.avatar = avatar;
        ApplySpeeds();
    }

    /// <summary>
    /// Walks, runs, or stands if neither
    /// </summary>
    public void SetLocomotion(bool walking, bool running)
    {
        animator.SetBool(Walking, walking);
        animator.SetBool(Running, running);
        //What is left of an attack's clip once its action ended is its recovery, played out standing still: a move cuts it
        if ((walking || running) && attack != null && recovering)
        {
            StopCoroutine(attack);
            attack = null;
            FadeAttackLayers(false, attackFadeOut);
        }
    }

    /// <summary>
    /// The attack's action is over: a move now cuts the rest of its clip
    /// </summary>
    public void EndAttack()
    {
        recovering = true;
    }

    /// <summary>
    /// Plays an attack's clip over the locomotion, then its follow-up if any, and blends back to the locomotion as the last clip ends.
    /// The clock sets the clips' time at each frame (the attack's timing), linear if none
    /// </summary>
    /// <param name="legs">How much the legs follow the clip, from the stance's (0) to the clip's (1); a generic rig, or an edition without the blend, plays the whole body</param>
    public void PlayAttack(AnimationClip clip, float speed = 1, AnimationClip followUp = null, AttackClock clock = null, float legs = 1)
    {
        EditionSkin skin = GameAssets.Instance.classicSkin;
        //An attack the Classic plays without a clip has none there
        clip = skin.Current(clip);
        if (clip == null || dead) return;
        if (attack != null) StopCoroutine(attack);
        recovering = false;
        legsWeight = animator.isHuman && Edition.Profile.attackLegs ? Mathf.Clamp01(legs) : 1;
        attack = StartCoroutine(Attack(clip, Mathf.Max(0.05f, speed), skin.Current(followUp), clock ?? AttackClock.Linear));
    }

    private IEnumerator Attack(AnimationClip clip, float speed, AnimationClip followUp, AttackClock clock)
    {
        //An attack still playing or blending out is cut by the next one: a short blend keeps the chain snappy
        bool chained = animator.GetLayerWeight(ActionLayer) > 0 || animator.GetLayerWeight(UpperActionLayer) > 0;
        int first = PlayInSlot(clip, chained ? chainedAttackFade : 0);
        if (!chained) FadeAttackLayers(true, AttackFade);
        //In seconds of the clips at the attack's speed, the clock's positions
        float clipEnd = clip.length / speed;
        float followUpStart = followUp != null ? Mathf.Max(0, clipEnd - followUpFade) : float.MaxValue;
        float end = followUp != null ? followUpStart + followUp.length / speed : clipEnd;
        float endTime = clock.TimeAt(end);
        int second = -1;
        bool fadingOut = false;
        for (float time = 0; time < endTime; time += Time.deltaTime)
        {
            float position = clock.Position(time);
            SetClipTime(first, clip, position * speed);
            if (position >= followUpStart)
            {
                if (second < 0) second = PlayInSlot(followUp, followUpFade);
                SetClipTime(second, followUp, (position - followUpStart) * speed);
            }
            if (!fadingOut && time >= endTime - attackFadeOut)
            {
                fadingOut = true;
                FadeAttackLayers(false, attackFadeOut);
            }
            yield return null;
        }
        if (!fadingOut) FadeAttackLayers(false, 0);
        attack = null;
    }

    /// <returns>The slot playing the clip</returns>
    private int PlayInSlot(AnimationClip clip, float fade)
    {
        //The other slot: the one playing blends out while this one blends in
        slot = 1 - slot;
        overrides[attackSlots[slot]] = clip;
        animator.SetFloat(AttackTimes[slot], 0);
        //The layers holding the chain at their weights, their fade goes on while this attack plays
        if (weightFades[ActionLayer] != null && fade > 0) FadeAttackLayers(true, fade);
        foreach (int layer in AttackLayers)
        {
            if (fade > 0) animator.CrossFadeInFixedTime(AttackStates[slot], fade, layer, 0);
            else animator.Play(AttackStates[slot], layer, 0);
        }
        return slot;
    }

    private static readonly int[] AttackLayers = { ActionLayer, UpperActionLayer };

    /// <summary>
    /// Blends the attack layers in (the whole body at the legs' weight, the upper body at full weight while the legs don't follow
    /// the clip fully) or out
    /// </summary>
    private void FadeAttackLayers(bool playing, float duration)
    {
        FadeLayer(ActionLayer, playing ? legsWeight : 0, duration);
        FadeLayer(UpperActionLayer, playing && legsWeight < 1 ? 1 : 0, duration);
    }

    /// <summary>
    /// Shows the slot's clip at the time, in seconds of the clip
    /// </summary>
    private void SetClipTime(int clipSlot, AnimationClip clip, float time)
    {
        animator.SetFloat(AttackTimes[clipSlot], Mathf.Clamp01(time / clip.length));
    }

    /// <summary>
    /// Plays the hit matching the health lost, none when the armor took it all, over the locomotion and the attacks
    /// </summary>
    public void PlayHit(int healthLost)
    {
        if (dead) return;
        int state = healthLost <= 0 ? HitNone : healthLost < regularHitDamage ? HitSmall : healthLost < criticalHitDamage ? HitRegular : HitCritical;
        //A crossfade into the state playing would not restart it
        //From no reaction, the weight blends in; a crossfade into the state playing would not restart it
        if (animator.GetLayerWeight(ReactionLayer) == 0 || animator.GetCurrentAnimatorStateInfo(ReactionLayer).shortNameHash == state)
            animator.Play(state, ReactionLayer, 0);
        else animator.CrossFadeInFixedTime(state, HitFade, ReactionLayer, 0);
        FadeLayer(ReactionLayer, 1, HitFade);
        if (reaction != null) StopCoroutine(reaction);
        reaction = StartCoroutine(EndHit());
    }

    private IEnumerator EndHit()
    {
        //The length of the entity's hit clip is known once the animator entered its state
        yield return null;
        AnimatorStateInfo hit = animator.IsInTransition(ReactionLayer) ? animator.GetNextAnimatorStateInfo(ReactionLayer) : animator.GetCurrentAnimatorStateInfo(ReactionLayer);
        yield return new WaitForSeconds(Mathf.Max(0, hit.length - Time.deltaTime - hitFadeOut));
        FadeLayer(ReactionLayer, 0, hitFadeOut);
        reaction = null;
    }

    /// <summary>
    /// Plays the death over everything, for good
    /// </summary>
    public void PlayDeath()
    {
        dead = true;
        StopAllCoroutines();
        System.Array.Clear(weightFades, 0, LayerCount);
        if (animator.GetLayerWeight(ReactionLayer) == 0) animator.Play(Death, ReactionLayer, 0);
        else animator.CrossFadeInFixedTime(Death, deathFade, ReactionLayer, 0);
        FadeLayer(ReactionLayer, 1, deathFade);
    }

    /// <summary>
    /// Blends a layer's weight to the target over the duration, in scaled time like the animator
    /// </summary>
    private void FadeLayer(int layer, float target, float duration)
    {
        if (weightFades[layer] != null) StopCoroutine(weightFades[layer]);
        weightFades[layer] = StartCoroutine(Fade(layer, target, duration));
    }

    private IEnumerator Fade(int layer, float target, float duration)
    {
        float start = animator.GetLayerWeight(layer);
        for (float time = 0; time < duration; time += Time.deltaTime)
        {
            animator.SetLayerWeight(layer, Mathf.Lerp(start, target, time / duration));
            yield return null;
        }
        animator.SetLayerWeight(layer, target);
        weightFades[layer] = null;
    }
}
