using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IArtifact
{
    /// <summary>
    /// Do all the effect of the <c>Artifact</c>
    /// </summary>
    /// <param name="hitTerrain">the position where the player clicked</param>
    public void Launch(PlayerStats source, RaycastHit hitTerrain, Animator animator);

    public bool IsRaycastHitAccepted(RaycastHit hitTerrain);

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
