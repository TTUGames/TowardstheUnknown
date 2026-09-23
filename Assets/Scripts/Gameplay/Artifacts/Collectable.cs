using Assets.Scripts.Player_NPC_Artifact.Player.TetrisInventory;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    private List<Artifact> artifacts;

    public static Collectable InstantiateCollectable(List<Artifact> artifacts) {
        Collectable collectable = Instantiate(GameAssets.Instance.collectable);
        collectable.artifacts = artifacts;
        collectable.SetAura();
        return collectable;
    }

    private void SetAura() {
        ArtifactRarity maxRarity = artifacts.Max(artifact => artifact.Rarity);
        Instantiate(GameAssets.Instance.dropAuras[(int)maxRarity], transform).transform.localPosition = Vector3.zero;
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
        if (artifacts == null) throw new System.Exception("Collectable should not be instantiated directly, please use InstantiateCollectable instead");
        ChangeUI changeUI = FindAnyObjectByType<ChangeUI>();
        if (!changeUI.IsInventoryOpened)
            changeUI.ChangeStateInventory();
        changeUI.OpenChestInterface(true);

        TetrisInventory chest = FindAnyObjectByType<InventoryManager>().chest;
        chest.LoadInventoryData(TetrisInventoryData.FromArtifacts(artifacts));
        chest.Open();

        Destroy(gameObject);
    }

    public List<Artifact> GetArtifacts() {
        return artifacts;
	}
}
