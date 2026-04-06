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
        DissolveWeapon(sword);
    }

    public void UndissolveSword()
    {
        UndissolveWeapon(sword);
    }

    public void DissolveGun()
    {
        DissolveWeapon(gun);
    }

    public void UndissolveGun()
    {
        UndissolveWeapon(gun);
    }

    private void DissolveWeapon(GameObject weapon)
    {
        List<Material> materials = new List<Material>();
        weapon.GetComponent<MeshRenderer>().GetMaterials(materials);
        StartCoroutine(Fade(materials[0], -2, dissolveSpeed, weapon, true));
    }

    private void UndissolveWeapon(GameObject weapon)
    {
        weapon.SetActive(true);
        List<Material> materials = new List<Material>();
        weapon.GetComponent<MeshRenderer>().GetMaterials(materials);
        StartCoroutine(Fade(materials[0], 5, dissolveSpeed, weapon, false));
    }

    private IEnumerator Fade(Material material, float target, float time, GameObject weapon, bool dissolve)
    {
        while (material.GetFloat("_DissolvePosition") != target)
        {
            material.SetFloat("_DissolvePosition", Mathf.MoveTowards(material.GetFloat("_DissolvePosition"), target, time));
            yield return null;
        }

        if (dissolve)
        {
            weapon.SetActive(false);
        }
    }
}
