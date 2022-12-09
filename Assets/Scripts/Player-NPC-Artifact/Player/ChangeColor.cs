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

    public void Colorize(Color color)
    {
        foreach (GameObject neonObject in lNeonObjectWithSkinnedMeshRenderer)
            neonObject.GetComponent<SkinnedMeshRenderer>().material.SetColor("_LaserColor", new Vector4(color.r * intensity * 2.5f, color.g * intensity * 2.5f, color.b * intensity * 2.5f, 1));

        foreach (GameObject neonObject in lNeonObjectWithMeshRenderer)
            neonObject.GetComponent<MeshRenderer>().material.SetColor("_GlowColor", new Vector4(color.r * intensity * 2.5f, color.g * intensity * 2.5f, color.b * intensity * 2.5f, 1));
    }

    public void Uncolorize()
    {
        foreach (GameObject neonObject in lNeonObjectWithSkinnedMeshRenderer)
            neonObject.GetComponent<SkinnedMeshRenderer>().material.SetColor("_LaserColor", new Vector4(baseColor.r * intensity * 2.5f, baseColor.g * intensity * 2.5f, baseColor.b * intensity * 2.5f, 1));

        foreach (GameObject neonObject in lNeonObjectWithMeshRenderer)
            neonObject.GetComponent<MeshRenderer>().material.SetColor("_GlowColor", new Vector4(baseColor.r * intensity * 2.5f, baseColor.g * intensity * 2.5f, baseColor.b * intensity * 2.5f, 1));
    }

    public Color GetColor()
    {
        return lNeonObjectWithSkinnedMeshRenderer[0].GetComponent<SkinnedMeshRenderer>().material.GetColor("_LaserColor");
    }
}
