using Assets.Scripts.Player_NPC_Artifact.Player.TetrisInventory;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    [SerializeField, Tooltip("Indexed by the best artifact rarity: common, rare, epic, legendary")]
    private GameObject[] auras = new GameObject[4];

    private List<Artifact> artifacts;

    public void SetArtifacts(List<Artifact> artifacts) {
        this.artifacts = artifacts;
        ArtifactRarity maxRarity = artifacts.Max(artifact => artifact.Rarity);
        Instantiate(auras[(int)maxRarity], transform).transform.localPosition = Vector3.zero;
	}

    /// <summary>
    /// Tries to pickup the item when the player enters the collision
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) {
            other.GetComponent<PlayerMove>().InterruptMovement();
            TryPickUp();
        }
    }

    /// <summary>
    /// Opens the chest interface with this collectable's artifacts, and destroys it
    /// </summary>
    private void TryPickUp()
    {
        if (artifacts == null) throw new System.Exception("Collectable should not be instantiated directly, SetArtifacts must be called after instantiating it");
        InventoryScreen inventory = GameScene.UI.Inventory;
        if (!inventory.IsOpen)
            inventory.Toggle();
        inventory.OpenChest(true);
        inventory.Chest.LoadInventoryData(TetrisInventoryData.FromArtifacts(artifacts));

        Destroy(gameObject);
    }

    public List<Artifact> GetArtifacts() {
        return artifacts;
	}
}
