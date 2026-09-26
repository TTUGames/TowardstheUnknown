using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tints the player's neons in the color of the artifact being cast, then back to their rest color: the glowing outfit
/// (Character Glow shader, whose intensity stays the material's) and the weapons (HDR glow color, times <c>intensity</c>).
/// The rest color is the outfit material's glow color
/// </summary>
public class ChangeColor : MonoBehaviour
{
    private static readonly int GlowColor = Shader.PropertyToID("_GlowColor");

    [SerializeField] List<GameObject> lNeonObjectWithSkinnedMeshRenderer;
    [SerializeField] List<GameObject> lNeonObjectWithMeshRenderer;
    [Tooltip("HDR multiplier of the weapons' glow color")]
    [SerializeField] float intensity;

    private readonly List<Material> outfitMaterials = new List<Material>();
    private readonly List<Material> weaponMaterials = new List<Material>();
    private Color restColor;
    private Color currentColor;

    public void Start()
    {
        foreach (GameObject neonObject in lNeonObjectWithSkinnedMeshRenderer)
            outfitMaterials.Add(neonObject.GetComponent<SkinnedMeshRenderer>().material);
        foreach (GameObject neonObject in lNeonObjectWithMeshRenderer)
            weaponMaterials.Add(neonObject.GetComponent<MeshRenderer>().material);
        restColor = outfitMaterials.Count > 0 ? outfitMaterials[0].GetColor(GlowColor) : Color.white;
        currentColor = restColor;
        Apply(restColor);
    }

    public void Colorize(Color color)
    {
        StopAllCoroutines();
        StartCoroutine(ColorTransition(color));
    }

    public void Uncolorize()
    {
        Colorize(restColor);
    }

    private IEnumerator ColorTransition(Color targetColor)
    {
        Color startingColor = currentColor;
        for (float elapsedTime = 0; elapsedTime < 1f; )
        {
            elapsedTime += Time.deltaTime;
            Apply(Color.Lerp(startingColor, targetColor, elapsedTime));
            yield return null;
        }
    }

    private void Apply(Color color)
    {
        currentColor = color;
        foreach (Material material in outfitMaterials)
            material.SetColor(GlowColor, color);
        foreach (Material material in weaponMaterials)
            material.SetColor(GlowColor, color * intensity);
    }
}
