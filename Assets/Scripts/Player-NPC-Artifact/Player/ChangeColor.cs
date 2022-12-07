using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    [SerializeField] List<GameObject> lNeonObjectWithSkinnedMeshRenderer;
    [SerializeField] List<GameObject> lNeonObjectWithMeshRenderer;
    [SerializeField] Color            baseColor;

    public void Colorize(Color color)
    {
        print(lNeonObjectWithSkinnedMeshRenderer[0].GetComponent<Renderer>().material.GetColor("_EmissionColor"));
        foreach (GameObject neonObject in lNeonObjectWithSkinnedMeshRenderer)
                neonObject.GetComponent<SkinnedMeshRenderer>().material.SetColor("_LaserColor", color);

        foreach (GameObject neonObject in lNeonObjectWithMeshRenderer)
            neonObject.GetComponent<MeshRenderer>().material.SetColor("_GlowColor", color);
    }

    public void Uncolorize()
    {
        foreach (GameObject neonObject in lNeonObjectWithSkinnedMeshRenderer)
            neonObject.GetComponent<SkinnedMeshRenderer>().material.SetColor("_LaserColor", baseColor);

        foreach (GameObject neonObject in lNeonObjectWithMeshRenderer)
            neonObject.GetComponent<MeshRenderer>().material.SetColor("_GlowColor", baseColor);
    }

    public Color GetColor()
    {
        return lNeonObjectWithSkinnedMeshRenderer[0].GetComponent<SkinnedMeshRenderer>().material.GetColor("_LaserColor");
    }
}
