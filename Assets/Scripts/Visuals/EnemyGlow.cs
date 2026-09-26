using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The energy of an enemy following the game (<c>_GlowMultiplier</c> of the Enemy Energy and Spectral Glow materials, through
/// a property block): brighter on its turn (<see cref="TurnSystem.TurnChanged"/>), while hovered (<see cref="Room.EntityHovered"/>)
/// or targeted by the selected artifact, flaring when hit, flickering when its health runs low, and dying out with it.
/// The materials are never touched, only the multiplier, eased. The <c>wisps</c> escaping from the body (the Great enemies)
/// follow it: their emission scales with the energy and stops on death
/// </summary>
[DisallowMultipleComponent]
public class EnemyGlow : MonoBehaviour
{
    private static readonly int GlowMultiplier = Shader.PropertyToID("_GlowMultiplier");

    [Tooltip("Energy on its turn in a combat, 1 at rest")]
    [SerializeField] private float turnGlow = 1.6f;
    [Tooltip("Energy added while hovered")]
    [SerializeField] private float hoverGlow = 0.4f;
    [Tooltip("Energy added while the selected artifact would hit it")]
    [SerializeField] private float targetedGlow = 0.3f;
    [Tooltip("Energy added when hit, fading out over hitFlareDuration, times the health lost over heavyHitHealth")]
    [SerializeField] private float hitFlare = 2f;
    [SerializeField] private float hitFlareDuration = 0.4f;
    [SerializeField] private int heavyHitHealth = 30;
    [Tooltip("Health ratio under which the energy flickers, stronger as the health drops")]
    [SerializeField, Range(0, 1)] private float lowHealth = 0.3f;
    [Tooltip("Energy the flicker takes away at no health left")]
    [SerializeField, Range(0, 1)] private float lowHealthFlicker = 0.6f;
    [SerializeField] private float flickerSpeed = 9f;
    [Tooltip("Seconds for the energy to die out with the enemy")]
    [SerializeField] private float deathFade = 0.6f;
    [Tooltip("How fast the energy eases towards its level, per second")]
    [SerializeField] private float easing = 6f;
    [Tooltip("The particles of energy escaping from the body, if any: their emission rate follows the energy")]
    [SerializeField] private ParticleSystem[] wisps = System.Array.Empty<ParticleSystem>();

    private readonly List<Renderer> renderers = new();
    private MaterialPropertyBlock block;
    private EntityStats stats;
    private EntityTurn turn;
    private TacticsMove move;
    private PlayerAttack playerAttack;
    private bool active, hovered, targeted;
    private float level = 1;
    private float flare;
    private float applied = -1;
    private float[] wispRates;

    private void Awake()
    {
        stats = GetComponent<EntityStats>();
        turn = GetComponent<EntityTurn>();
        move = GetComponent<TacticsMove>();
        block = new MaterialPropertyBlock();
        wispRates = new float[wisps.Length];
        for (int i = 0; i < wisps.Length; i++)
            if (wisps[i] != null) wispRates[i] = wisps[i].emission.rateOverTimeMultiplier;
        foreach (Renderer candidate in GetComponentsInChildren<Renderer>(true))
        {
            if (candidate is ParticleSystemRenderer) continue;
            foreach (Material material in candidate.sharedMaterials)
                if (material != null && material.HasFloat(GlowMultiplier))
                {
                    renderers.Add(candidate);
                    break;
                }
        }
    }

    private void OnEnable()
    {
        if (stats != null) stats.Hit += OnHit;
        TurnSystem.Instance.TurnChanged += OnTurnChanged;
        Room.EntityHovered += OnEntityHovered;
        GameEvents.CombatEnded += OnCombatEnded;
        if (GameScene.Player != null && GameScene.Player.TryGetComponent(out playerAttack))
            playerAttack.TargetsPreviewed += OnTargetsPreviewed;
        OnTurnChanged();
        OnEntityHovered(Room.HoveredEntity);
    }

    private void OnDisable()
    {
        if (stats != null) stats.Hit -= OnHit;
        if (TurnSystem.Instance != null) TurnSystem.Instance.TurnChanged -= OnTurnChanged;
        Room.EntityHovered -= OnEntityHovered;
        GameEvents.CombatEnded -= OnCombatEnded;
        if (playerAttack != null) playerAttack.TargetsPreviewed -= OnTargetsPreviewed;
        playerAttack = null;
    }

    private void Update()
    {
        bool dead = stats != null && stats.IsDead;
        float target = dead ? 0 : (active ? turnGlow : 1) + (hovered ? hoverGlow : 0) + (targeted ? targetedGlow : 0);
        level = dead
            ? Mathf.MoveTowards(level, 0, Time.deltaTime / Mathf.Max(deathFade, 0.001f) * Mathf.Max(turnGlow, 1))
            : Mathf.Lerp(level, target, 1 - Mathf.Exp(-easing * Time.deltaTime));
        flare = Mathf.MoveTowards(flare, 0, Time.deltaTime * hitFlare / Mathf.Max(hitFlareDuration, 0.001f));

        float multiplier = level + flare;
        float health = stats != null && stats.MaxHealth > 0 ? (float)stats.CurrentHealth / stats.MaxHealth : 1;
        if (!dead && health < lowHealth)
        {
            // Two detuned waves: an irregular stutter rather than a steady blink
            float danger = 1 - health / Mathf.Max(lowHealth, 0.001f);
            float wave = Mathf.PerlinNoise(Time.time * flickerSpeed, transform.position.x * 3.1f);
            multiplier *= 1 - lowHealthFlicker * danger * wave;
        }

        if (Mathf.Abs(multiplier - applied) < 0.002f) return;
        applied = multiplier;
        for (int i = 0; i < wisps.Length; i++)
        {
            if (wisps[i] == null) continue;
            ParticleSystem.EmissionModule emission = wisps[i].emission;
            emission.rateOverTimeMultiplier = wispRates[i] * multiplier;
        }
        foreach (Renderer glowing in renderers)
        {
            if (glowing == null) continue;
            glowing.GetPropertyBlock(block);
            block.SetFloat(GlowMultiplier, multiplier);
            glowing.SetPropertyBlock(block);
        }
    }

    private void OnHit(int healthLost)
    {
        if (healthLost <= 0) return;
        flare = Mathf.Max(flare, hitFlare * Mathf.Clamp01((float)healthLost / Mathf.Max(heavyHitHealth, 1)));
    }

    private void OnTurnChanged() => active = turn != null && TurnSystem.Instance.IsCurrentTurn(turn);

    private void OnCombatEnded()
    {
        active = false;
        targeted = false;
    }

    private void OnEntityHovered(TacticsMove entity) => hovered = entity != null && entity == move;

    private void OnTargetsPreviewed(Artifact artifact, IReadOnlyList<EntityStats> targets)
    {
        targeted = false;
        if (stats == null) return;
        foreach (EntityStats entity in targets)
            if (entity == stats) targeted = true;
    }
}
