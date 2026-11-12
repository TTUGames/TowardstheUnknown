using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The player's inventory: the grid of its artifacts, shown by the inventory screen
/// </summary>
public class InventoryManager : MonoBehaviour
{
    [SerializeField] private List<ArtifactData> startingArtifacts;
    [SerializeField, Tooltip("Every artifact, to give a suspended run its saved ones back")] private ArtifactCatalog catalog;

    private TetrisInventoryData data;

    /// <summary>
    /// Fired when the artifacts in the player inventory change
    /// </summary>
    public event System.Action ArtifactsChanged;

    /// <summary>
    /// The grid, filled with the starting artifacts on first use, or a suspended run's (<see cref="RunSave"/>)
    /// </summary>
    public TetrisInventoryData Data
    {
        get
        {
            if (data == null)
            {
                data = RunSave.Resumed != null ? Restore(RunSave.Resumed) : TetrisInventoryData.FromArtifacts(startingArtifacts.Select(artifact => artifact.CreateArtifact()));
                data.Changed += () =>
                {
                    //Covers the artifacts that come from elsewhere than a chest (debug tools)
                    VFXWarmup.Warm(data.Artifacts);
                    ArtifactsChanged?.Invoke();
                };
            }
            return data;
        }
    }

    public IReadOnlyList<Artifact> GetPlayerArtifacts() => Data.Artifacts;

    /// <summary>
    /// Every artifact of the game
    /// </summary>
    public ArtifactCatalog Catalog => catalog;

    /// <summary>
    /// A new artifact of the catalog by its ID, null if unknown
    /// </summary>
    public Artifact Create(string id) => catalog != null ? catalog.Find(id)?.CreateArtifact() : null;

    // The saved grid
    private TetrisInventoryData Restore(RunSave.Data save)
    {
        var restored = new TetrisInventoryData(TetrisInventoryData.DefaultGridSize);
        restored.Place(save.items, Create);
        return restored;
    }
}
