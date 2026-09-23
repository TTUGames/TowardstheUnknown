using Assets.Scripts.Player_NPC_Artifact.Player.TetrisInventory;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public TetrisInventory PlayerInventory;
    public TetrisInventory chest;

    void Start()
    {
        List<Artifact> startingArtifacts = new List<Artifact>()
        {
            new BasicDamage(),
            new PrecisionShoot(),
            new OffensiveFluid(),
            new DefensiveFluid(),
            new Barrier(),
        };

        PlayerInventory.LoadInventoryData(TetrisInventoryData.FromArtifacts(startingArtifacts));
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
