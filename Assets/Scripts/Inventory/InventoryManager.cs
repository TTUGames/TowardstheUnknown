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
    /// The saved grid: each artifact at its slot and rotation, in the order of the skills; one unknown or that doesn't fit is
    /// left out
    /// </summary>
    private TetrisInventoryData Restore(RunSave.Data save)
    {
        var restored = new TetrisInventoryData(TetrisInventoryData.DefaultGridSize);
        foreach (RunSave.Item saved in save.items)
        {
            ArtifactData artifact = catalog != null ? catalog.Find(saved.artifact) : null;
            if (artifact == null)
            {
                Debug.LogWarning("Saved artifact not found: " + saved.artifact);
                continue;
            }
            var item = new TetrisInventoryItem { itemData = artifact.CreateArtifact(), rotation = saved.rotation };
            if (restored.CanPlace(saved.slot, item)) restored.AddItem(saved.slot, item);
            else Debug.LogWarning("Saved artifact doesn't fit: " + saved.artifact);
        }
        return restored;
    }
}
