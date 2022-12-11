using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private Vector2Int inventorySize = new Vector2Int(5, 5);
    [SerializeField] private Vector2Int saveSlotsSize;
    
    private List<IArtifact> lArtifacts;



    // Start is called before the first frame update
    void Awake()
    {
        lArtifacts = new List<IArtifact>();
        //lArtifacts.Add(new Rush());
        /*lArtifacts.Add(new CelestialSword());
        lArtifacts.Add(new ExplosiveSacrifice());
        lArtifacts.Add(new ClearRoomArtifact());*/
    }
    
    private void Update()
    {
    }

    public Vector2Int InventorySize { get => inventorySize; set => inventorySize = value; }
    public Vector2Int SaveSlotsSize { get => saveSlotsSize; set => saveSlotsSize = value; }
    public List<IArtifact> LArtifacts { get => lArtifacts; set => lArtifacts = value; }

    public void CombatStart() {
        foreach (IArtifact artifact in lArtifacts) {
            artifact.CombatStart();
        }
    }

    public void TurnStart() {
        foreach (IArtifact artifact in lArtifacts) {
            artifact.TurnStart();
		}
        GameObject.FindGameObjectWithTag("UI").transform.GetChild(0).Find("Skills").gameObject.GetComponent<UISkillsBar>().UpdateSkillBar();
    }
}
