using Assets.Scripts.Player_NPC_Artifact.Player.TetrisInventory;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private List<ArtifactData> startingArtifacts;

    /// <summary>
    /// Fired when the artifacts in the player inventory change
    /// </summary>
    public event System.Action ArtifactsChanged;

    private TetrisInventory PlayerInventory => GameScene.UI.Inventory.PlayerInventory;

    void Start()
    {
        PlayerInventory.LoadInventoryData(TetrisInventoryData.FromArtifacts(startingArtifacts.Select(data => data.CreateArtifact())));
        PlayerInventory.Changed += OnInventoryUpdate;
        OnInventoryUpdate();
    }

    private void OnInventoryUpdate()
    {
        ArtifactsChanged?.Invoke();
    }

    public List<Artifact> GetPlayerArtifacts()
    {
        return PlayerInventory.GetInventoryData().GetArtifacts();
    }
}
