using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Shows the hits and the death of an entity: hit VFX, white flash, a recoil of its model away from the attacker,
/// hit and death animations, and the corpse vanishing at the end of its death animation
/// </summary>
[RequireComponent(typeof(EntityStats))]
public class EntityFeedback : MonoBehaviour
{

    [SerializeField, Tooltip("Played where the entity is hit")] private GameObject hitVFX;
    [SerializeField, Tooltip("Height of the hit VFX")] private float hitVFXHeight;
    [SerializeField, Tooltip("Time the death animation plays before the entity is removed"), SuffixLabel("s")] private float deathDuration = 1.5f;
    [SerializeField, Tooltip("At the end of the death, the corpse shrinks into the ground"), SuffixLabel("s")] private float vanishDuration = 0.35f;
    [SerializeField, Tooltip("Opacity of the white flash on a hit"), Range(0, 1)] private float flashStrength = 0.75f;
    [SerializeField, Tooltip("In real time, so that it shows through the hit stop"), SuffixLabel("s")] private float flashDuration = 0.18f;

    [BoxGroup("Recoil"), SerializeField, SuffixLabel("m"), Tooltip("Push of the model away from the attacker on the lightest hit taking health")] private float lightRecoil = 0.05f;
    [BoxGroup("Recoil"), SerializeField, SuffixLabel("m"), Tooltip("On a heavy hit (hit weight 1, see ImpactFeedback)")] private float heavyRecoil = 0.16f;
    [BoxGroup("Recoil"), SerializeField, SuffixLabel("m"), Tooltip("When the armor takes all of the hit")] private float blockedRecoil = 0.025f;
    [BoxGroup("Recoil"), SerializeField, Min(0), Tooltip("Squash of the model at the recoil's peak, per meter of recoil: it flattens and widens")] private float squashPerMeter = 0.6f;
    [BoxGroup("Recoil"), SerializeField, Min(0.01f), SuffixLabel("s"), Tooltip("Time to settle back, in game time: the recoil holds its peak through the hit stop")] private float recoilDuration = 0.25f;

    /// <summary>
    /// A dead entity's corpse starts vanishing, with the time it takes
    /// </summary>
    public static event System.Action<EntityFeedback, float> VanishStarted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => VanishStarted = null;

    private EntityStats stats;
    private EntityAnimator animator;
    private HitFlash flash;
    private Coroutine flashing;

    // The recoil moves the children holding the model, never the entity itself, which the tiles and the moves place
    private readonly List<Transform> bodies = new();
    private readonly List<(Vector3 position, Vector3 scale)> written = new();
    private Vector3 writtenOffset;
    private Vector3 writtenSquash = Vector3.one;
    private Vector3 recoilOffset;
    private float recoilSquash;
    private float recoilTime = 1;

    private void Awake()
    {
        stats = GetComponent<EntityStats>();
        animator = GetComponent<EntityAnimator>();
        flash = new HitFlash(gameObject);
        FindBodies();
    }

    private void OnEnable()
    {
        stats.Hit += OnHit;
        stats.Died += OnDied;
        ImpactFeedback.HitWeighed += OnHitWeighed;
    }

    private void OnDisable()
    {
        stats.Hit -= OnHit;
        stats.Died -= OnDied;
        ImpactFeedback.HitWeighed -= OnHitWeighed;
        flash.Set(0);
        recoilTime = 1;
        ApplyRecoil(0);
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
        else Recoil(blockedRecoil);

        if (animator != null) animator.PlayHit(healthLost);
    }

    // Raised right after the stats' Hit, for the hits taking health
    private void OnHitWeighed(EntityStats entity, float weight)
    {
        if (entity == stats) Recoil(Mathf.Lerp(lightRecoil, heavyRecoil, weight));
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

    /// <summary>
    /// Pushes the model away from the entity playing its turn, the attacker, at once: the hit stop holds the peak.
    /// A hit without an attacker (its own turn) only squashes it
    /// </summary>
    private void Recoil(float distance)
    {
        EntityTurn attacker = TurnSystem.Instance != null ? TurnSystem.Instance.Current : null;
        Vector3 away = attacker != null && attacker.gameObject != gameObject ? transform.position - attacker.transform.position : Vector3.zero;
        away.y = 0;
        recoilOffset = away.sqrMagnitude > 1e-4f ? away.normalized * distance : Vector3.zero;
        recoilSquash = distance * squashPerMeter;
        recoilTime = 0;
    }

    private void LateUpdate()
    {
        if (recoilTime >= 1 && written.Count == 0) return;
        recoilTime = Mathf.Min(1, recoilTime + Time.deltaTime / recoilDuration);
        float left = 1 - recoilTime;
        ApplyRecoil(left * left);
    }

    /// <summary>
    /// Offsets and squashes the model's children by the recoil's amount (1 at the peak, 0 at rest). The animation may
    /// write their position or scale again each frame, or not: a value still holding what the recoil wrote gets its rest
    /// back before the new offset, each one on its own
    /// </summary>
    private void ApplyRecoil(float amount)
    {
        bool rest = amount <= 0;
        Vector3 offset = rest ? Vector3.zero : transform.InverseTransformVector(recoilOffset * amount);
        float squash = rest ? 0 : recoilSquash * amount;
        Vector3 squashScale = new Vector3(1 + squash * 0.5f, 1 - squash, 1 + squash * 0.5f);
        for (int i = 0; i < bodies.Count; i++)
        {
            Transform body = bodies[i];
            if (body == null) continue;
            Vector3 position = body.localPosition, scale = body.localScale;
            if (i < written.Count && position == written[i].position) position -= writtenOffset;
            if (i < written.Count && scale == written[i].scale)
                scale = new Vector3(scale.x / writtenSquash.x, scale.y / writtenSquash.y, scale.z / writtenSquash.z);
            body.localPosition = position + offset;
            body.localScale = Vector3.Scale(scale, squashScale);
            if (i < written.Count) written[i] = (body.localPosition, body.localScale);
            else written.Add((body.localPosition, body.localScale));
        }
        writtenOffset = offset;
        writtenSquash = squashScale;
        if (rest) written.Clear();
    }

    // The direct children holding a mesh: the model, not the hover box, the tile watcher nor the particles
    private void FindBodies()
    {
        foreach (Transform child in transform)
            foreach (Renderer renderer in child.GetComponentsInChildren<Renderer>(true))
                if (renderer is SkinnedMeshRenderer || renderer is MeshRenderer)
                {
                    bodies.Add(child);
                    break;
                }
    }

    private void OnDied()
    {
        if (animator != null) animator.PlayDeath();
        StartCoroutine(Vanish());
    }

    /// <summary>
    /// Shrinks the corpse into the ground just before the <c>DieAction</c> removes it
    /// </summary>
    private IEnumerator Vanish()
    {
        float duration = Mathf.Min(vanishDuration, deathDuration);
        yield return new WaitForSeconds(deathDuration - duration);
        VanishStarted?.Invoke(this, duration);
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
