using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int sizeX;
    [SerializeField] private int sizeY;
    
    private List<Artifact> lArtifacts;



    // Start is called before the first frame update
    void Start()
    {
        lArtifacts = new List<Artifact>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public int SizeX                 { get => sizeX;      set => sizeX = value; }
    public int SizeY                 { get => sizeY;      set => sizeY = value; }
    public List<Artifact> LArtifacts { get => lArtifacts; set => lArtifacts = value; }
}
