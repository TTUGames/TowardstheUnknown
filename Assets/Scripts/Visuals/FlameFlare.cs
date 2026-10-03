using System.Collections;
using UnityEngine;

/// <summary>
/// A flame of the room flaring up once a relic opened in it is closed (<see cref="GameEvents.ChestOpened"/>, then the menus
/// closing, <see cref="ChangeUI.MenuChanged"/>: the chest covers the room until then): its flame and glow swell, then settle back,
/// and it throws a burst of sparks and shards. Only the flames of the room shown hear it: a room left is disabled. On the
/// Anniversary's flame (<c>Prefabs/VFX/StylizedFlame</c>), which the Classic doesn't show
/// </summary>
public class FlameFlare : MonoBehaviour
{
    [SerializeField, Tooltip("Swells with the flare")] private Transform flame;
    [SerializeField, Tooltip("Swells more than the flame")] private Transform glow;
    [SerializeField] private ParticleSystem sparks;
    [SerializeField] private ParticleSystem shards;
    [SerializeField, Min(1), Tooltip("The flame's scale at the flare's peak, then eased back to 1")] private float flameScale = 1.5f;
    [SerializeField, Min(1), Tooltip("The glow's scale at the flare's peak")] private float glowScale = 1.9f;
    [SerializeField, Min(0.05f), Tooltip("Seconds from the peak back to rest")] private float duration = 1.2f;
    [SerializeField, Min(0)] private int sparkCount = 24;
    [SerializeField, Min(0)] private int shardCount = 6;

    private Vector3 flameRest, glowRest;
    private Coroutine flare;
    // A chest was opened: the flare waits for the menus to close
    private bool armed;

    private void Awake()
    {
        if (flame != null) flameRest = flame.localScale;
        if (glow != null) glowRest = glow.localScale;
    }

    private void OnEnable()
    {
        GameEvents.ChestOpened += OnChestOpened;
        if (GameScene.UI != null) GameScene.UI.MenuChanged += OnMenuChanged;
    }

    private void OnDisable()
    {
        GameEvents.ChestOpened -= OnChestOpened;
        if (GameScene.UI != null) GameScene.UI.MenuChanged -= OnMenuChanged;
        armed = false;
        flare = null;
        SetScale(1, 1);
    }

    private void OnChestOpened(System.Collections.Generic.IReadOnlyList<Artifact> artifacts) => armed = true;

    private void OnMenuChanged()
    {
        if (!armed || GameScene.UI.IsMenuOpen) return;
        armed = false;
        if (sparks != null) sparks.Emit(sparkCount);
        if (shards != null) shards.Emit(shardCount);
        if (flare != null) StopCoroutine(flare);
        flare = StartCoroutine(Flare());
    }

    private IEnumerator Flare()
    {
        for (float time = 0; time < duration; time += Time.deltaTime)
        {
            // Out at once, back with an ease out
            float k = 1 - Mathf.SmoothStep(0, 1, time / duration);
            SetScale(Mathf.Lerp(1, flameScale, k), Mathf.Lerp(1, glowScale, k));
            yield return null;
        }
        SetScale(1, 1);
        flare = null;
    }

    private void SetScale(float flameFactor, float glowFactor)
    {
        if (flame != null) flame.localScale = flameRest * flameFactor;
        if (glow != null) glow.localScale = glowRest * glowFactor;
    }
}
