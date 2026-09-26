using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Shows the hits and the death of an entity: hit VFX, white flash, animator triggers, and the corpse vanishing at the end of its death animation
/// </summary>
[RequireComponent(typeof(EntityStats))]
public class EntityFeedback : MonoBehaviour
{
    private static readonly int TakingDamage = Animator.StringToHash("isTakingDamage");
    private static readonly int DamageValue = Animator.StringToHash("DamageValue");
    private static readonly int Dying = Animator.StringToHash("isDying");

    [SerializeField, Tooltip("Played where the entity is hit")] private GameObject hitVFX;
    [SerializeField, Tooltip("Height of the hit VFX")] private float hitVFXHeight;
    [SerializeField] private Animator animator;
    [SerializeField, Tooltip("Time the death animation plays before the entity is removed"), SuffixLabel("s")] private float deathDuration = 1.5f;
    [SerializeField, Tooltip("At the end of the death, the corpse shrinks into the ground"), SuffixLabel("s")] private float vanishDuration = 0.35f;
    [SerializeField, Tooltip("Opacity of the white flash on a hit"), Range(0, 1)] private float flashStrength = 0.75f;
    [SerializeField, Tooltip("In real time, so that it shows through the hit stop"), SuffixLabel("s")] private float flashDuration = 0.18f;

    private EntityStats stats;
    private HitFlash flash;
    private Coroutine flashing;

    private void Awake()
    {
        stats = GetComponent<EntityStats>();
        flash = new HitFlash(gameObject);
    }

    private void OnEnable()
    {
        stats.Hit += OnHit;
        stats.Died += OnDied;
    }

    private void OnDisable()
    {
        stats.Hit -= OnHit;
        stats.Died -= OnDied;
        flash.Set(0);
    }

    public float DeathDuration => deathDuration;

    public GameObject HitVFX => hitVFX;

    private void OnHit(int healthLost)
    {
        Vector3 spawnPosition = transform.position;
        spawnPosition.y = hitVFXHeight;
        VFXPool.Release(VFXPool.Get(hitVFX, spawnPosition, Quaternion.identity), 0.5f);
        //A hit taken by the armor does not flash
        if (healthLost > 0)
        {
            if (flashing != null) StopCoroutine(flashing);
            flashing = StartCoroutine(Flash());
        }

        if (animator == null) return;
        animator.SetTrigger(TakingDamage);
        animator.SetInteger(DamageValue, healthLost);
    }

    private IEnumerator Flash()
    {
        for (float time = 0; time < flashDuration; time += Time.unscaledDeltaTime)
        {
            flash.Set(flashStrength * (1 - time / flashDuration));
            yield return null;
        }
        flash.Set(0);
        flashing = null;
    }

    private void OnDied()
    {
        if (animator != null) animator.SetTrigger(Dying);
        StartCoroutine(Vanish());
    }

    /// <summary>
    /// Shrinks the corpse into the ground just before the <c>DieAction</c> removes it
    /// </summary>
    private IEnumerator Vanish()
    {
        float duration = Mathf.Min(vanishDuration, deathDuration);
        yield return new WaitForSeconds(deathDuration - duration);
        Vector3 scale = transform.localScale;
        Vector3 position = transform.position;
        for (float time = 0; time < duration; time += Time.deltaTime)
        {
            float t = time / duration;
            float eased = t * t;
            transform.localScale = scale * (1 - eased);
            transform.position = position + Vector3.down * (0.3f * eased);
            yield return null;
        }
        transform.localScale = Vector3.zero;
    }
}
