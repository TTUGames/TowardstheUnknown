using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The player's inventory: the grid of its artifacts, shown by the inventory screen
/// </summary>
public class InventoryManager : MonoBehaviour
{
    [SerializeField] private List<ArtifactData> startingArtifacts;

    private TetrisInventoryData data;

    /// <summary>
    /// Fired when the artifacts in the player inventory change
    /// </summary>
    public event System.Action ArtifactsChanged;

    /// <summary>
    /// The grid, filled with the starting artifacts on first use
    /// </summary>
    public TetrisInventoryData Data
    {
        get
        {
            if (data == null)
            {
                data = TetrisInventoryData.FromArtifacts(startingArtifacts.Select(artifact => artifact.CreateArtifact()));
                data.Changed += () => ArtifactsChanged?.Invoke();
            }
            return data;
        }
    }

    public IReadOnlyList<Artifact> GetPlayerArtifacts() => Data.Artifacts;
}
