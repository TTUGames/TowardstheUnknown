using System;
using UnityEngine;

/// <summary>
/// The rarities' design tokens, respected from end to end (inventory pieces, relics on the floor, their light): one hue per
/// rarity, and a few tones, the brightness at which a use shows it. Every use asks for a rarity and a tone, so that
/// changing a hue or a tone changes it everywhere. A use may still scale what it gets (a brighter legendary relic), never
/// pick its own color
/// </summary>
[CreateAssetMenu(fileName = "RarityPalette", menuName = "TTU/Rarity Palette")]
public class RarityPalette : ScriptableObject
{
    public enum Tone
    {
        [Tooltip("Filled shapes of the interface: the inventory pieces")] Surface,
        [Tooltip("Lines and highlights of the interface")] Accent,
        [Tooltip("Emissive light, HDR: the relics' glow and light")] Glow,
    }

    [Serializable]
    private struct ToneLevel
    {
        [Tooltip("Multiplies the hue; above 1 for the HDR tones")] public float brightness;
        [Range(0, 1), Tooltip("Mixes the hue towards white, to keep it readable when light")] public float whiten;
    }

    [SerializeField, Tooltip("The hue of each rarity: common, rare, epic, legendary")]
    private Color[] hues = { new(0.086f, 0.086f, 0.11f), new(0.18f, 0.48f, 1f), new(0.48f, 0.24f, 1f), new(1f, 0.6f, 0.18f) };

    [SerializeField, Tooltip("Surface, Accent, Glow")]
    private ToneLevel[] tones = { new() { brightness = 0.72f }, new() { brightness = 1, whiten = 0.35f }, new() { brightness = 1.6f } };

    /// <summary>
    /// The rarity's hue at the tone's brightness
    /// </summary>
    public Color Get(ArtifactRarity rarity, Tone tone)
    {
        Color hue = hues[(int)rarity];
        ToneLevel level = tones[(int)tone];
        Color color = Color.Lerp(hue, Color.white, level.whiten) * level.brightness;
        color.a = 1;
        return color;
    }
}
