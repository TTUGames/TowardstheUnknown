using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    public Vector3 posToGo;
    private Artifact artifact;

    private static Dictionary<ArtifactRarity, GameObject> auras;
    private static Collectable collectablePrefab;
    private static bool initialized = false;

    static void Init() {
        collectablePrefab = Resources.Load<Collectable>("Prefabs/Collectables/Collectable");
        auras = new Dictionary<ArtifactRarity, GameObject>();
        auras.Add(ArtifactRarity.COMMON, Resources.Load<GameObject>("VFX/Drop/CommonDrop"));
        auras.Add(ArtifactRarity.RARE, Resources.Load<GameObject>("VFX/Drop/RareDrop"));
        auras.Add(ArtifactRarity.EPIC, Resources.Load<GameObject>("VFX/Drop/EpicDrop"));
        auras.Add(ArtifactRarity.LEGENDARY, Resources.Load<GameObject>("VFX/Drop/LegendaryDrop"));
        initialized = true;
    }

    public static Collectable InstantiateCollectable(Artifact artifact) {
        if (!initialized) Init();
        Collectable collectable = Instantiate<Collectable>(collectablePrefab);
        collectable.artifact = artifact;
        collectable.SetAura();
        return collectable;
    }

	private void Start() {
        if (artifact == null) throw new System.Exception("Collectable should not be instantiated directly, please use InstantiateCollectable instead");
	}

	private void SetAura() {
        GameObject aura = Instantiate<GameObject>(auras[artifact.GetRarity()], transform);
        aura.transform.localPosition = Vector3.zero;
	}
    /// <summary>
    /// Tries to pickup the item when the player enters the collision
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            TryPickUp();
    }

    /// <summary>
    /// Tries to pickup the item when E is pressed while colliding
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Input.GetKey(KeyCode.E) && !GameObject.FindGameObjectWithTag("UI").GetComponent<ChangeUI>().GetIsInventoryOpen())
            {
                TryPickUp();
            }
        }
    }

    /// <summary>
    /// Tries to pickup this item, and destroys it if successful
    /// </summary>
    private void TryPickUp()
    {
        bool wasPickedUp = false;
        wasPickedUp = TetrisSlot.instanceSlot.addInFirstSpace(artifact); //add to the bag matrix.

        if (wasPickedUp)
            Destroy(this.gameObject);
    }
}