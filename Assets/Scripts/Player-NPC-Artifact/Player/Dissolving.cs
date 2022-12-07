using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissolving : MonoBehaviour
{
    [SerializeField] private float      dissolveSpeed = 1f;
    [SerializeField] private GameObject sword;
    [SerializeField] private GameObject gun;

    public void DissolveAll()
    {
        DissolveSword();
        DissolveGun();
    }

    public void Undissolve(int weapon)
    {
        if (weapon == -1)
            UndissolveGun();
        else if (weapon == 1)
            UndissolveSword();
        else if (weapon == 2)
        {
            UndissolveSword();
            UndissolveGun();
        }
    }

    public void DissolveSword()
    {
        List<Material> lm = new List<Material>();
        sword.GetComponent<MeshRenderer>().GetMaterials(lm);
        StartCoroutine(Fade(lm[0], -2, dissolveSpeed));
    }

    public void UndissolveSword()
    {
        List<Material> lm = new List<Material>();
        sword.GetComponent<MeshRenderer>().GetMaterials(lm);
        StartCoroutine(Fade(lm[0], 5, dissolveSpeed));
    }
    
    public void DissolveGun()
    {
        List<Material> lm = new List<Material>();
        gun.GetComponent<MeshRenderer>().GetMaterials(lm);
        StartCoroutine(Fade(lm[0], -2, dissolveSpeed));
    }

    public void UndissolveGun()
    {
        List<Material> lm = new List<Material>();
        gun.GetComponent<MeshRenderer>().GetMaterials(lm);
        StartCoroutine(Fade(lm[0], 5, dissolveSpeed));
    }

    IEnumerator Fade(Material material, float target, float time)
    {
        while (material.GetFloat("_DissolvePosition") != target)
        {
            material.SetFloat("_DissolvePosition", Mathf.MoveTowards(material.GetFloat("_DissolvePosition"), target, time)); // possibility to put over time if needed : Time.deltaTime / time
            yield return null;
        }
    }
}
