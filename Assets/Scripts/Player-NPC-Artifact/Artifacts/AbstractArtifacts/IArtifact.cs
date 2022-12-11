using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IArtifact
{
    /// <summary>
    /// Manages the artifact's targets then applies its effects
    /// </summary>
    /// <param name="source">The entity using the artifact</param>
    /// <param name="tile">The targeted tile</param>
    public void Launch(PlayerAttack source, Tile tile);

    /// <summary>
    /// Tells if a tile is valid to be targeted
    /// </summary>
    public bool CanTarget(Tile tile);

    /// <summary>
    /// Get the artifact's attack range
    /// </summary>
    /// <returns></returns>
    public TileSearch GetRange();

    /// <summary>
    /// Applies start of combat effects to the  <c>Artifact</c>
    /// </summary>
    public void CombatStart();

    /// <summary>
    /// Applies start of turn effects to the  <c>Artifact</c>
    /// </summary>
    public void TurnStart();


    /// <summary>
    /// Tells if the artifact can be cast by the source entity
    /// </summary>
    public bool CanUse(PlayerStats source);

    /// <summary>
    /// Gets the tiles targetted by the  <c>Artifact</c>
    /// </summary>
    public List<Tile> GetTargets(Tile targetedTile);

    /// <summary>
    /// Get the <c>Color</c> of the  <c>Artifact</c>
    /// </summary>
    /// <returns>The <c>Color</c></returns>
    public Color GetColor();

    /// <summary>
    /// Get the weapon of the  <c>Artifact</c>
    /// </summary>
    /// <returns>The <c>WeaponEnum</c></returns>
    public WeaponEnum GetWeapon();

    /// <summary>
    /// Get the <c>Sprite</c> of the <c>Artifact</c>
    /// </summary>
    /// <returns>The <c>Sprite</c></returns>
    public Sprite GetIcon();

    /// <summary>
    /// Get the <c>AudioSound</c> of the <c>Artifact</c>
    /// </summary>
    /// <returns>The <c>AudioSound</c></returns>
    public AudioClip GetSound();

    /// <summary>
    /// Get the energy cost of the <c>Artifact</c>
    /// </summary>
    /// <returns>The cost</returns>
    public int GetCost();
}
