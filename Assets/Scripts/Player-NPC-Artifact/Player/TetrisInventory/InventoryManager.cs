using Assets.Scripts.Player_NPC_Artifact.Player.TetrisInventory;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{

    public TetrisInventory PlayerInventory;

    // Start is called before the first frame update
    void Start()
    {

        List<Artifact> startingArtifacts = new List<Artifact>()
        {
            new EchoBomb(),
            new Impale(),
            new PrecisionShoot(),
            new BasicDamage(),
            new BasicShield(),
        };


        TetrisInventoryData tetrisInventoryData = new TetrisInventoryData(new Vector2Int(5, 5));

        foreach (Artifact artifact in startingArtifacts)
        {
            TetrisInventoryItem item = new TetrisInventoryItem()
            {
                itemData = artifact,
            };

            if (tetrisInventoryData.FindSlotForItem(item, out Vector2Int slot))
            {
                tetrisInventoryData.AddItem(slot, item);
            }
        }

        PlayerInventory.LoadInventoryData(tetrisInventoryData);
        PlayerInventory.Open();
        PlayerInventory.OnInventoryChange.AddListener(OnInventoryUpdate);

        FindObjectOfType<UISkillsBar>().UpdateSkillBar();

    }

    public void OnInventoryUpdate()
    {
        FindObjectOfType<UISkillsBar>().UpdateSkillBar();
    }

    public List<Artifact> GetPlayerArtifacts()
    {
        return PlayerInventory.GetInventoryData().GetArtifacts();
    }
}
