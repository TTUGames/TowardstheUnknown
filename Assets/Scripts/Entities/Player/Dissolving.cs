using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponEnum
{
    gun, sword, none, both
};

/// <summary>
/// Draws the player's weapons and takes them away: the height their material is drawn up to (<c>_DissolvePosition</c>) moves
/// through a property block of their renderer, so that the edition can swap their shared materials. In an edition with
/// <see cref="EditionProfile.weaponSwapSpeed"/>, the player never holds both at once but for an artifact wielding both: a
/// weapon drawn waits for the other to be taken away, at that speed
/// </summary>
public class Dissolving : MonoBehaviour
{
    private static readonly int DissolvePosition = Shader.PropertyToID("_DissolvePosition");
    private const float dissolvedPosition = -2;
    private const float visiblePosition = 5;

    [SerializeField, Tooltip("Dissolve position units per second")] private float dissolveSpeed = 3f;
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
        SetWeaponVisible(gun, false, true);
        SetWeaponVisible(sword, true, true);
    }

    public void DissolveAll()
    {
        SetWeaponVisible(sword, false);
        SetWeaponVisible(gun, false);
    }

    public void Undissolve(WeaponEnum weapon)
    {
        //The weapon of an artifact wielding one takes the other's place; an artifact wielding both draws both at once
        if (weapon == WeaponEnum.sword)
        {
            if (Swaps) SetWeaponVisible(gun, false, true);
            SetWeaponVisible(sword, true, true);
        }
        else if (weapon == WeaponEnum.gun)
        {
            if (Swaps) SetWeaponVisible(sword, false, true);
            SetWeaponVisible(gun, true, true);
        }
        else if (weapon == WeaponEnum.both)
        {
            SetWeaponVisible(sword, true);
            SetWeaponVisible(gun, true);
        }
    }

    private static bool Swaps => Edition.Profile.weaponSwapSpeed > 0;

    private readonly Dictionary<GameObject, Coroutine> fades = new Dictionary<GameObject, Coroutine>();
    // Where each weapon's fade is: its material's value until the first one
    private readonly Dictionary<GameObject, float> positions = new Dictionary<GameObject, float>();
    private MaterialPropertyBlock block;

    /// <param name="swap">Taken away for the other weapon, at the edition's swap speed; drawn, once the other is gone</param>
    private void SetWeaponVisible(GameObject weapon, bool visible, bool swap = false)
    {
        //A new fade replaces the running one, otherwise they would fight over the material
        if (fades.TryGetValue(weapon, out Coroutine fade) && fade != null) StopCoroutine(fade);
        swap &= Swaps;
        //The edition may draw the weapon faster, so that it is whole before the strike
        float speed = visible && Edition.Profile.weaponAppearSpeed > 0 ? Edition.Profile.weaponAppearSpeed
            : swap ? Edition.Profile.weaponSwapSpeed : dissolveSpeed;
        GameObject after = visible && swap ? (weapon == sword ? gun : sword) : null;
        fades[weapon] = StartCoroutine(Fade(weapon.GetComponent<MeshRenderer>(), visible ? visiblePosition : dissolvedPosition, weapon, !visible, speed, after));
    }

    private IEnumerator Fade(Renderer renderer, float target, GameObject weapon, bool dissolve, float speed, GameObject after)
    {
        //The other weapon deactivates once dissolved
        if (after != null)
            while (after.activeSelf) yield return null;
        if (!dissolve) weapon.SetActive(true);
        block ??= new MaterialPropertyBlock();
        if (!positions.TryGetValue(weapon, out float position)) position = renderer.sharedMaterial.GetFloat(DissolvePosition);
        while (position != target)
        {
            position = Mathf.MoveTowards(position, target, speed * Time.deltaTime);
            positions[weapon] = position;
            renderer.GetPropertyBlock(block);
            block.SetFloat(DissolvePosition, position);
            renderer.SetPropertyBlock(block);
            yield return null;
        }

        if (dissolve) weapon.SetActive(false);
    }
}
