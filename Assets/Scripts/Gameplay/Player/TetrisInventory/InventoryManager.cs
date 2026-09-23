using Assets.Scripts.Player_NPC_Artifact.Player.TetrisInventory;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public TetrisInventory PlayerInventory;
    public TetrisInventory chest;

    [SerializeField] private List<ArtifactData> startingArtifacts;

    void Start()
    {
        PlayerInventory.LoadInventoryData(TetrisInventoryData.FromArtifacts(startingArtifacts.Select(data => data.CreateArtifact())));
        PlayerInventory.OnInventoryChange.AddListener(OnInventoryUpdate);
        OnInventoryUpdate();
    }

    public void OnInventoryUpdate()
    {
        FindAnyObjectByType<UIEnergy>().UpdateEnergyUI();
        FindAnyObjectByType<UISkillsBar>().UpdateSkillBar();
    }

    public List<Artifact> GetPlayerArtifacts()
    {
        return PlayerInventory.GetInventoryData().GetArtifacts();
    }
}
