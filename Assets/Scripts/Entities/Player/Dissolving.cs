using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponEnum
{
    gun, sword, none, both
};

public class Dissolving : MonoBehaviour
{
    private const float dissolvedPosition = -2;
    private const float visiblePosition = 5;

    [SerializeField] private float dissolveSpeed = 1f;
    [SerializeField] private GameObject sword;
    [SerializeField] private GameObject gun;

    private void OnEnable()
    {
        GameEvents.CombatStarted += DissolveAll;
        GameEvents.CombatEnded += Start;
    }

    private void OnDisable()
    {
        GameEvents.CombatStarted -= DissolveAll;
        GameEvents.CombatEnded -= Start;
    }

    /// <summary>
    /// Displays only the sword, the default weapon
    /// </summary>
    public void Start()
    {
        SetWeaponVisible(gun, false);
        SetWeaponVisible(sword, true);
    }

    public void DissolveAll()
    {
        SetWeaponVisible(sword, false);
        SetWeaponVisible(gun, false);
    }

    public void Undissolve(WeaponEnum weapon)
    {
        if (weapon == WeaponEnum.sword || weapon == WeaponEnum.both) SetWeaponVisible(sword, true);
        if (weapon == WeaponEnum.gun || weapon == WeaponEnum.both) SetWeaponVisible(gun, true);
    }

    private readonly Dictionary<GameObject, Coroutine> fades = new Dictionary<GameObject, Coroutine>();

    private void SetWeaponVisible(GameObject weapon, bool visible)
    {
        //A new fade replaces the running one, otherwise they would fight over the material
        if (fades.TryGetValue(weapon, out Coroutine fade) && fade != null) StopCoroutine(fade);
        if (visible) weapon.SetActive(true);
        fades[weapon] = StartCoroutine(Fade(weapon.GetComponent<MeshRenderer>().material, visible ? visiblePosition : dissolvedPosition, weapon, !visible));
    }

    private IEnumerator Fade(Material material, float target, GameObject weapon, bool dissolve)
    {
        float position = material.GetFloat("_DissolvePosition");
        while (position != target)
        {
            position = Mathf.MoveTowards(position, target, dissolveSpeed);
            material.SetFloat("_DissolvePosition", position);
            yield return null;
        }

        if (dissolve) weapon.SetActive(false);
    }
}
