using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Drives an entity's animator, an override of <c>Entity.controller</c> giving its locomotion, hit and death clips.
/// Its three layers play over each other: the locomotion, the attacks, then the reactions to hits and death.
/// The attack and reaction layers weigh nothing while they play nothing (an empty state would override a humanoid's pose): their weight blends in and out.
/// The attacks bring their own clips, played in two alternating slots so that an attack blends into the next one, even the same.
/// The layers, states and parameters named here are the contract of <c>Entity.controller</c>.
/// </summary>
[RequireComponent(typeof(Animator))]
public class EntityAnimator : MonoBehaviour
{
    private const int ActionLayer = 1;
    private const int ReactionLayer = 2;
    private const int LayerCount = 3;
    private static readonly int Walking = Animator.StringToHash("Walking");
    private static readonly int Running = Animator.StringToHash("Running");
    private static readonly int WalkSpeed = Animator.StringToHash("WalkSpeed");
    private static readonly int RunSpeed = Animator.StringToHash("RunSpeed");
    private static readonly int[] AttackStates = { Animator.StringToHash("AttackA"), Animator.StringToHash("AttackB") };
    private static readonly int[] AttackSpeeds = { Animator.StringToHash("AttackSpeedA"), Animator.StringToHash("AttackSpeedB") };
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
    [BoxGroup("Reactions"), SerializeField, MinValue(0), SuffixLabel("s"), Tooltip("Blend into the death")] private float deathFade = 0.25f;
    [BoxGroup("Reactions"), SerializeField, MinValue(1), Tooltip("From this health lost, the hit plays the regular clip rather than the small one")] private int regularHitDamage = 25;
    [BoxGroup("Reactions"), SerializeField, MinValue(1), Tooltip("From this health lost, the hit plays the critical clip")] private int criticalHitDamage = 40;

    private Animator animator;
    private AnimatorOverrideController overrides;
    // The slot of the last attack, the next one takes the other
    private int slot;
    private Coroutine attack;
    private Coroutine reaction;
    private bool dead;
    private readonly Coroutine[] weightFades = new Coroutine[LayerCount];

    private void Awake()
    {
        animator = GetComponent<Animator>();
        //An instance per entity, whose attacks replace the clips of the slots. It wraps the base controller without the entity's overrides: copy them
        var entityOverrides = animator.runtimeAnimatorController as AnimatorOverrideController;
        overrides = new AnimatorOverrideController(animator.runtimeAnimatorController);
        if (entityOverrides != null)
        {
            var clips = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>>(entityOverrides.overridesCount);
            entityOverrides.GetOverrides(clips);
            overrides.ApplyOverrides(clips);
        }
        animator.runtimeAnimatorController = overrides;
        ApplySpeeds();
    }

    private void ApplySpeeds()
    {
        animator.SetFloat(WalkSpeed, walkSpeed);
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
    }

    /// <summary>
    /// Plays an attack's clip over the locomotion, then its follow-up if any, and blends back to the locomotion as the last clip ends
    /// </summary>
    public void PlayAttack(AnimationClip clip, float speed = 1, AnimationClip followUp = null)
    {
        if (clip == null || dead) return;
        if (attack != null) StopCoroutine(attack);
        attack = StartCoroutine(Attack(clip, Mathf.Max(0.05f, speed), followUp));
    }

    private IEnumerator Attack(AnimationClip clip, float speed, AnimationClip followUp)
    {
        //An attack still playing or blending out is cut by the next one: a short blend keeps the chain snappy
        bool chained = animator.GetLayerWeight(ActionLayer) > 0;
        if (chained) PlayInSlot(clip, speed, chainedAttackFade);
        else
        {
            PlayInSlot(clip, speed, 0);
            FadeLayer(ActionLayer, 1, attackFade);
        }
        if (followUp != null)
        {
            yield return new WaitForSeconds(Mathf.Max(0, clip.length / speed - followUpFade));
            PlayInSlot(followUp, speed, followUpFade);
            clip = followUp;
        }
        yield return new WaitForSeconds(Mathf.Max(0, clip.length / speed - attackFadeOut));
        FadeLayer(ActionLayer, 0, attackFadeOut);
        attack = null;
    }

    private void PlayInSlot(AnimationClip clip, float speed, float fade)
    {
        //The other slot: the one playing blends out while this one blends in
        slot = 1 - slot;
        overrides[attackSlots[slot]] = clip;
        animator.SetFloat(AttackSpeeds[slot], speed);
        //The layer holding the chain at full weight, its fade goes on while this attack plays
        if (weightFades[ActionLayer] != null && fade > 0) FadeLayer(ActionLayer, 1, fade);
        if (fade > 0) animator.CrossFadeInFixedTime(AttackStates[slot], fade, ActionLayer, 0);
        else animator.Play(AttackStates[slot], ActionLayer, 0);
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
        else animator.CrossFadeInFixedTime(state, hitFade, ReactionLayer, 0);
        FadeLayer(ReactionLayer, 1, hitFade);
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
