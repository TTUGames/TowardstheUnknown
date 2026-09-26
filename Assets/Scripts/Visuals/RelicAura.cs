using UnityEngine;

/// <summary>
/// The look of a relic floating above the floor (a collectable), for one rarity: gives its color, glow and glitch clock to
/// the orb and the distortion around it (a property block, the same clock so that they glitch together), its color to
/// the fragments breaking off and the hue to its light
/// </summary>
[ExecuteAlways]
public class RelicAura : MonoBehaviour
{
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int GlowId = Shader.PropertyToID("_Glow");
    private static readonly int GlitchRateId = Shader.PropertyToID("_GlitchRate");
    private static readonly int GlitchChanceId = Shader.PropertyToID("_GlitchChance");

    [SerializeField, ColorUsage(false, true), Tooltip("The rarity's color")] private Color color = Color.cyan;
    [SerializeField, Tooltip("How bright the orb glows")] private float glow = 1;
    [SerializeField, Tooltip("Glitch slots per second")] private float glitchRate = 9;
    [SerializeField, Range(0, 1), Tooltip("The chance that a slot glitches")] private float glitchChance = 0.18f;
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private ParticleSystem motes;
    [SerializeField, Tooltip("Tinted by the rarity's color, with its own intensity")] private Light glowLight;

    private void OnEnable() => Apply();

    private void OnValidate() => Apply();

    private void Apply()
    {
        var block = new MaterialPropertyBlock();
        foreach (Renderer target in renderers)
        {
            if (target == null) continue;
            target.GetPropertyBlock(block);
            block.SetColor(ColorId, color);
            block.SetFloat(GlowId, glow);
            block.SetFloat(GlitchRateId, glitchRate);
            block.SetFloat(GlitchChanceId, glitchChance);
            target.SetPropertyBlock(block);
        }
        if (motes != null)
        {
            ParticleSystem.MainModule main = motes.main;
            main.startColor = color;
        }
        // A light takes the hue only: its intensity is its own
        if (glowLight != null)
        {
            float max = Mathf.Max(color.r, color.g, color.b, 0.0001f);
            glowLight.color = color / max;
        }
    }
}
