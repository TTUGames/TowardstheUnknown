using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    [SerializeField] List<GameObject> lNeonObjectWithSkinnedMeshRenderer;
    [SerializeField] List<GameObject> lNeonObjectWithMeshRenderer;
    [SerializeField] Color baseColor;
    [SerializeField] float intensity;

    public void Start()
    {
        Uncolorize();
    }
    
    public void Colorize(Color color)
    {
        StopAllCoroutines();
        StartCoroutine(ColorTransition(color));
    }

    public void Uncolorize()
    {
        StopAllCoroutines();
        StartCoroutine(ColorTransition(baseColor));
    }

    public Color GetColor()
    {
        return lNeonObjectWithSkinnedMeshRenderer[0].GetComponent<SkinnedMeshRenderer>().material.GetColor("_LaserColor");
    }

    private IEnumerator ColorTransition(Color targetColor)
    {
        float elapsedTime = 0;
        Color startingColor = GetColor()/intensity;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / 1f);

            // Interpolate color values
            Color newColor = Color.Lerp(startingColor, targetColor, t);

            // Update neon objects
            foreach (GameObject neonObject in lNeonObjectWithSkinnedMeshRenderer)
                neonObject.GetComponent<SkinnedMeshRenderer>().material.SetColor("_LaserColor", newColor * intensity);

            foreach (GameObject neonObject in lNeonObjectWithMeshRenderer)
                neonObject.GetComponent<MeshRenderer>().material.SetColor("_GlowColor", newColor * intensity);

            yield return null;
        }
    }
}
