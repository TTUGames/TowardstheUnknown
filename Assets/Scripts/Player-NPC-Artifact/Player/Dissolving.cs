using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponEnum
{
    gun, sword, none, both
};

public class Dissolving : MonoBehaviour
{
    [SerializeField] private float dissolveSpeed = 1f;
    [SerializeField] private GameObject sword;
    [SerializeField] private GameObject gun;

    public void Start()
    {
        DissolveGun();
        UndissolveSword();
    }

    public void DissolveAll()
    {
        DissolveSword();
        DissolveGun();
    }

    public void Undissolve(WeaponEnum weapon)
    {
        if (weapon == WeaponEnum.gun)
            UndissolveGun();
        else if (weapon == WeaponEnum.sword)
            UndissolveSword();
        else if (weapon == WeaponEnum.both)
        {
            UndissolveSword();
            UndissolveGun();
        }
    }

    public void DissolveSword()
    {
        List<Material> lm = new List<Material>();
        sword.GetComponent<MeshRenderer>().GetMaterials(lm);
        StartCoroutine(Fade(lm[0], -2, dissolveSpeed, "sword", true));
    }

    public void UndissolveSword()
    {
        sword.SetActive(true);
        List<Material> lm = new List<Material>();
        sword.GetComponent<MeshRenderer>().GetMaterials(lm);
        StartCoroutine(Fade(lm[0], 5, dissolveSpeed, "sword", false));
    }

    public void DissolveGun()
    {
        List<Material> lm = new List<Material>();
        gun.GetComponent<MeshRenderer>().GetMaterials(lm);
        StartCoroutine(Fade(lm[0], -2, dissolveSpeed, "gun", true));
    }

    public void UndissolveGun()
    {
        gun.SetActive(true);
        List<Material> lm = new List<Material>();
        gun.GetComponent<MeshRenderer>().GetMaterials(lm);
        StartCoroutine(Fade(lm[0], 5, dissolveSpeed, "gun", false));
    }

    IEnumerator Fade(Material material, float target, float time, string weapon, bool dissolve)
    {
        while (material.GetFloat("_DissolvePosition") != target)
        {
            material.SetFloat("_DissolvePosition", Mathf.MoveTowards(material.GetFloat("_DissolvePosition"), target, time));
            yield return null;
        }

        if (dissolve)
        {
            if (weapon == "sword")
                sword.SetActive(false);
            else if (weapon == "gun")
                gun.SetActive(false);
        }
    }
}
