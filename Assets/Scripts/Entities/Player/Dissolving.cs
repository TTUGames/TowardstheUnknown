using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// What an artifact is cast with. Serialized by value in the artifacts: keep the order
/// </summary>
public enum Weapon { Gun, Sword, None, Both }

/// <summary>
/// Draws the player's weapons and takes them away. The player holds the sword, in combat or not; an attack draws its
/// artifact's weapon instead (the gun, both, or none for a bare-handed one), and <see cref="PlayerAttack"/> gives the sword
/// back once the casts are over. The height their material is drawn up to (<c>_DissolvePosition</c>) moves
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

    private void Start() => HoldSword();

    /// <summary>
    /// Displays only the sword, the default weapon
    /// </summary>
    public void HoldSword()
    {
        SetWeaponVisible(gun, false, true);
        SetWeaponVisible(sword, true, true);
    }

    /// <summary>
    /// Draws an artifact's weapon. One weapon takes the other's place, in an edition that swaps them; both are drawn at
    /// once; none takes away the weapons still drawn, at the swap speed: the artifact is cast bare-handed
    /// </summary>
    public void Draw(Weapon weapon) {
        switch (weapon) {
            case Weapon.Sword: DrawOnly(sword, gun); break;
            case Weapon.Gun: DrawOnly(gun, sword); break;
            case Weapon.Both:
                SetWeaponVisible(sword, true);
                SetWeaponVisible(gun, true);
                break;
            default:
                SetWeaponVisible(sword, false, true);
                SetWeaponVisible(gun, false, true);
                break;
        }
    }

    private void DrawOnly(GameObject drawn, GameObject other) {
        if (Swaps) SetWeaponVisible(other, false, true);
        SetWeaponVisible(drawn, true, true);
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
