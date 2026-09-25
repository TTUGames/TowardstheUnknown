using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    private static readonly int LaserColor = Shader.PropertyToID("_LaserColor");
    private static readonly int GlowColor = Shader.PropertyToID("_GlowColor");

    [SerializeField] List<GameObject> lNeonObjectWithSkinnedMeshRenderer;
    [SerializeField] List<GameObject> lNeonObjectWithMeshRenderer;
    [SerializeField] Color baseColor;
    [SerializeField] float intensity;

    private readonly List<Material> laserMaterials = new List<Material>();
    private readonly List<Material> glowMaterials = new List<Material>();

    public void Start()
    {
        foreach (GameObject neonObject in lNeonObjectWithSkinnedMeshRenderer)
            laserMaterials.Add(neonObject.GetComponent<SkinnedMeshRenderer>().material);
        foreach (GameObject neonObject in lNeonObjectWithMeshRenderer)
            glowMaterials.Add(neonObject.GetComponent<MeshRenderer>().material);
        Uncolorize();
    }

    public void Colorize(Color color)
    {
        StopAllCoroutines();
        StartCoroutine(ColorTransition(color));
    }

    public void Uncolorize()
    {
        Colorize(baseColor);
    }

    private IEnumerator ColorTransition(Color targetColor)
    {
        Color startingColor = laserMaterials[0].GetColor(LaserColor) / intensity;

        for (float elapsedTime = 0; elapsedTime < 1f; )
        {
            elapsedTime += Time.deltaTime;
            Color newColor = Color.Lerp(startingColor, targetColor, elapsedTime) * intensity;

            foreach (Material material in laserMaterials)
                material.SetColor(LaserColor, newColor);
            foreach (Material material in glowMaterials)
                material.SetColor(GlowColor, newColor);

            yield return null;
        }
    }
}
