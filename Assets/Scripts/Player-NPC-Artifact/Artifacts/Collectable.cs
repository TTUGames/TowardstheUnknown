using Assets.Scripts.Player_NPC_Artifact.Player.TetrisInventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    public Vector3 posToGo;
    private List<Artifact> artifacts;

    private static Dictionary<ArtifactRarity, GameObject> auras;
    private static Collectable collectablePrefab;
    private static bool initialized = false;

    private InventoryManager inventoryManager;
    private ChangeUI changeUI;

    static void Init() {
        collectablePrefab = Resources.Load<Collectable>("Prefabs/Collectables/Collectable");
        auras = new Dictionary<ArtifactRarity, GameObject>();
        auras.Add(ArtifactRarity.COMMON, Resources.Load<GameObject>("VFX/Drop/CommonDrop"));
        auras.Add(ArtifactRarity.RARE, Resources.Load<GameObject>("VFX/Drop/RareDrop"));
        auras.Add(ArtifactRarity.EPIC, Resources.Load<GameObject>("VFX/Drop/EpicDrop"));
        auras.Add(ArtifactRarity.LEGENDARY, Resources.Load<GameObject>("VFX/Drop/LegendaryDrop"));

        initialized = true;
    }

    public static Collectable InstantiateCollectable(List<Artifact> artifacts) {
        if (!initialized) Init();
        Collectable collectable = Instantiate<Collectable>(collectablePrefab);
        collectable.artifacts = artifacts;
        collectable.SetAura();
        return collectable;
    }

	private void Start() {
        if (artifacts == null) throw new System.Exception("Collectable should not be instantiated directly, please use InstantiateCollectable instead");
        inventoryManager = FindObjectOfType<InventoryManager>();
        changeUI = FindObjectOfType<ChangeUI>();
    }

    private void SetAura() {
        ArtifactRarity maxRarity = ArtifactRarity.COMMON;
        foreach(Artifact artifact in artifacts) 
            if (artifact.GetRarity() > maxRarity) 
                maxRarity = artifact.GetRarity();
		
        GameObject aura = Instantiate<GameObject>(auras[maxRarity], transform);
        aura.transform.localPosition = Vector3.zero;
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
        if (!changeUI.IsInventoryOpened)
            changeUI.ChangeStateInventory();
        changeUI.OpenChestInterface(true);

        TetrisInventoryData tetrisInventoryData = new TetrisInventoryData(new Vector2Int(5, 5));


        foreach (Artifact artifact in artifacts) {
            TetrisInventoryItem randomItem = new TetrisInventoryItem() {
                itemData = artifact
            };

            if (tetrisInventoryData.FindSlotForItem(randomItem, out Vector2Int slot)) {
                tetrisInventoryData.AddItem(slot, randomItem);
            }
        }
        
        inventoryManager.chest.LoadInventoryData(tetrisInventoryData);
        inventoryManager.chest.Open();


        Destroy(this.gameObject);
    }
}