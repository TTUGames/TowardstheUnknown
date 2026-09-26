using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    [SerializeField, Tooltip("Indexed by the best artifact rarity: common, rare, epic, legendary")]
    private GameObject[] auras = new GameObject[4];

    private List<Artifact> artifacts;
    private Tile tile;

    public void SetArtifacts(List<Artifact> artifacts) {
        this.artifacts = artifacts;
        ArtifactRarity maxRarity = artifacts.Max(artifact => artifact.Rarity);
        Instantiate(auras[(int)maxRarity], transform).transform.localPosition = Vector3.zero;
        //Before the player can pick them up and cast them
        VFXWarmup.Warm(artifacts);
	}

    /// <summary>
    /// Registers on the tile under it, so that the movement paths go around it. Done in Start, once the spawn point has placed it
    /// </summary>
    private void Start() {
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Terrain"))
            && hit.collider.TryGetComponent(out tile))
            tile.Collectable = this;
    }

    private void OnDestroy() {
        if (tile != null && tile.Collectable == this) tile.Collectable = null;
    }

    /// <summary>
    /// Picks the artifacts up when the player walks into it
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out PlayerMove player)) return;
        player.InterruptMovement();
        TryPickUp();
    }

    /// <summary>
    /// Opens the chest interface with this collectable's artifacts, and destroys it
    /// </summary>
    private void TryPickUp()
    {
        if (artifacts == null) throw new System.Exception("Collectable should not be instantiated directly, SetArtifacts must be called after instantiating it");
        GameScene.UI.Inventory.OpenChest(artifacts);
        Destroy(gameObject);
    }
}
