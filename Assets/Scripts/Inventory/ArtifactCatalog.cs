using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Every artifact of the game, found by its ID (its asset name): how a suspended run gets its saved artifacts back
/// (<see cref="RunSave"/>). Kept whole by the editor (ArtifactCatalogSync) as artifacts are added or removed
/// </summary>
[CreateAssetMenu(fileName = "ArtifactCatalog", menuName = "TTU/Artifact Catalog")]
public class ArtifactCatalog : ScriptableObject
{
    public List<ArtifactData> artifacts = new();

    /// <summary>
    /// The artifact of an ID, null if none
    /// </summary>
    public ArtifactData Find(string id) => artifacts.Find(artifact => artifact != null && artifact.name == id);
}
