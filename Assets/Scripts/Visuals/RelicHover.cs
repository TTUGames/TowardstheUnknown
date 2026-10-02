using UnityEngine;

/// <summary>
/// How a relic floating above the floor (a collectable's aura) answers the pointer: eases <c>_Hover</c> in and out and
/// gives <c>_HoverPulse</c>, the seconds since the pointer came on it, to the orb and the distortion (property blocks, next
/// to <see cref="RelicAura"/>'s), brightens its light, which lights the floor under it, sheds a few fragments and a little
/// more while hovered, and plays its sound. The Anniversary's relic only: the Classic's aura has none, and stays still
/// </summary>
public class RelicHover : MonoBehaviour
{
    private static readonly int HoverId = Shader.PropertyToID("_Hover");
    private static readonly int HoverPulseId = Shader.PropertyToID("_HoverPulse");
    // The pulse is over long before: past it, the shaders show the hover's steady state
    private const float PulseEnd = 10;

    [SerializeField, Tooltip("The orb and the distortion")] private Renderer[] renderers;
    [SerializeField, Min(0.01f), Tooltip("Seconds the hover takes to come")] private float hoverIn = 0.18f;
    [SerializeField, Min(0.01f), Tooltip("Seconds the hover takes to go")] private float hoverOut = 0.35f;
    [SerializeField] private Light glowLight;
    [SerializeField, Min(0), Tooltip("Added to the light's intensity while hovered, in share of its own")] private float lightBoost = 1;
    [SerializeField, Min(0), Tooltip("Added to the light's intensity as the pointer arrives, in share of its own, fading at once")] private float lightFlash = 0.8f;
    [SerializeField] private ParticleSystem motes;
    [SerializeField, Min(0), Tooltip("Added to the fragments' rate while hovered, in share of their own")] private float moteBoost = 0.8f;
    [SerializeField, Min(0), Tooltip("Fragments shed as the pointer arrives")] private int moteBurst = 6;
    [SerializeField, Tooltip("When the pointer comes on the relic")] private AK.Wwise.Event hoverSound = new AK.Wwise.Event();

    private MaterialPropertyBlock block;
    private bool hovered;
    private float hover;
    private float pulse = PulseEnd;
    private float lightIntensity;
    private float moteRate;

    private void Awake() {
        block = new MaterialPropertyBlock();
        if (glowLight != null) lightIntensity = glowLight.intensity;
        if (motes != null) moteRate = motes.emission.rateOverTimeMultiplier;
    }

    private void OnDisable() {
        // Left with its room: still once back
        hovered = false;
        hover = 0;
        pulse = PulseEnd;
        Apply();
    }

    /// <summary>
    /// The pointer comes on the relic or leaves it
    /// </summary>
    public void SetHovered(bool hovered) {
        if (hovered == this.hovered) return;
        this.hovered = hovered;
        if (!hovered || !isActiveAndEnabled) return;
        pulse = 0;
        hoverSound.Post(gameObject);
        if (motes != null && moteBurst > 0) motes.Emit(moteBurst);
    }

    private void Update() {
        float target = hovered ? 1 : 0;
        if (hover == target && pulse >= PulseEnd) return;
        hover = Mathf.MoveTowards(hover, target, Time.deltaTime / (hovered ? hoverIn : hoverOut));
        pulse = Mathf.Min(pulse + Time.deltaTime, PulseEnd);
        Apply();
    }

    private void Apply() {
        float eased = hover * hover * (3 - 2 * hover);
        foreach (Renderer target in renderers) {
            if (target == null) continue;
            target.GetPropertyBlock(block);
            block.SetFloat(HoverId, eased);
            block.SetFloat(HoverPulseId, pulse);
            target.SetPropertyBlock(block);
        }
        if (glowLight != null)
            glowLight.intensity = lightIntensity * (1 + eased * lightBoost + Mathf.Exp(-pulse * 6) * lightFlash);
        if (motes != null) {
            ParticleSystem.EmissionModule emission = motes.emission;
            emission.rateOverTimeMultiplier = moteRate * (1 + eased * moteBoost);
        }
    }
}
