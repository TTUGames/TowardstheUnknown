using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IArtifact
{
    /// <summary>
    /// Launch the vfx of the <c>Artifact</c>
    /// </summary>
    /// <param name="position">the position where the vfx must appear</param>
    public void Launch(Vector3 position);

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
}
