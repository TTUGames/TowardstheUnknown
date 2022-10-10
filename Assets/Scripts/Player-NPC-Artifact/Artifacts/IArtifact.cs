using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IArtifact
{
    /// <summary>
    /// Do all the effect of the <c>Artifact</c>
    /// </summary>
    /// <param name="source">The entity using the artifact</param>
    /// <param name="tile">The targeted tile</param>
    /// <param name="animator"></param>
    public void Launch(PlayerStats source, Tile tile, Animator animator);

    /// <summary>
    /// Tells if a tile is valid to be targeted
    /// </summary>
    public bool CanTarget(Tile tile);

    /// <summary>
    /// Get the maximum distance to attack with this <c>Artifact</c>
    /// </summary>
    /// <returns>the maximum distance</returns>
    public int GetMaxDistance();

    /// <summary>
    /// Get the minimum distance to attack with this <c>Artifact</c>
    /// </summary>
    /// <returns>the minimum distance</returns>
    public int GetMinDistance();

    /// <summary>
    /// Get the energy cost of the artifact
    /// </summary>
    /// <returns>The energy cost</returns>
    public int GetCost();
}
