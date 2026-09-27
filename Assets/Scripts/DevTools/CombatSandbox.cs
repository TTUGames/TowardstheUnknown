using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The combat sandbox's rules, on its map variant: the player's energy is unlimited and its artifacts ignore their cooldowns,
/// and its inventory holds one set of the artifacts at a time, as many as its grid takes: Page Down and Page Up go through the sets
/// </summary>
public class CombatSandbox : MonoBehaviour
{
    [SerializeField, Tooltip("Every artifact to try, split into sets that fit the inventory")] private List<ArtifactData> artifacts;

    private readonly List<List<ArtifactData>> sets = new List<List<ArtifactData>>();
    private int currentSet;

    //The Debug controls are only enabled in the editor and development builds
    private void OnEnable()
    {
        GameInput.Controls.Debug.NextArtifacts.performed += OnNextArtifacts;
        GameInput.Controls.Debug.PreviousArtifacts.performed += OnPreviousArtifacts;
    }

    private void OnDisable()
    {
        GameInput.Controls.Debug.NextArtifacts.performed -= OnNextArtifacts;
        GameInput.Controls.Debug.PreviousArtifacts.performed -= OnPreviousArtifacts;
    }

    private void Start()
    {
        GameScene.Player.Stats.Unlimited = true;
        SplitIntoSets();
        ShowSet(0);
    }

    /// <summary>
    /// Fills an empty grid with the artifacts in order, starting a new set with the first one that doesn't fit
    /// </summary>
    private void SplitIntoSets()
    {
        TetrisInventoryData grid = null;
        foreach (ArtifactData data in artifacts.Where(data => data != null))
        {
            TetrisInventoryItem item = new TetrisInventoryItem() { itemData = data.CreateArtifact() };
            if (grid == null || !grid.FindSlotForItem(item, out Vector2Int slot))
            {
                grid = new TetrisInventoryData(TetrisInventoryData.DefaultGridSize);
                sets.Add(new List<ArtifactData>());
                grid.FindSlotForItem(item, out slot);
            }
            grid.AddItem(slot, item);
            sets[^1].Add(data);
        }
    }

    private void ShowSet(int index)
    {
        if (sets.Count == 0) return;
        currentSet = (index + sets.Count) % sets.Count;
        GameScene.Player.Inventory.Data.Replace(sets[currentSet].Select(data => data.CreateArtifact()));
        Debug.Log($"Combat sandbox: artifact set {currentSet + 1}/{sets.Count} ({string.Join(", ", sets[currentSet].Select(data => data.name))})");
    }

    private void OnNextArtifacts(InputAction.CallbackContext context) => ShowSet(currentSet + 1);

    private void OnPreviousArtifacts(InputAction.CallbackContext context) => ShowSet(currentSet - 1);
}
