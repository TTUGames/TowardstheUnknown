using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int sizeX = 5;
    [SerializeField] private int sizeY = 5;
    
    [SerializeField] private List<IArtifact> lArtifacts;



    // Start is called before the first frame update
    void Start()
    {
        lArtifacts = new List<IArtifact>();
        lArtifacts.Add(new PrecisionShoot());
        lArtifacts.Add(new TestMovementArtifact());
        lArtifacts.Add(new AttackBuffArtifact());
        lArtifacts.Add(new AoeDebugArtifact());
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public int SizeX                  { get => sizeX;      set => sizeX = value; }
    public int SizeY                  { get => sizeY;      set => sizeY = value; }
    public List<IArtifact> LArtifacts { get => lArtifacts; set => lArtifacts = value; }

    public void TurnStart() {
        foreach (IArtifact artifact in lArtifacts) {
            artifact.TurnStart();
		}
	}
}
