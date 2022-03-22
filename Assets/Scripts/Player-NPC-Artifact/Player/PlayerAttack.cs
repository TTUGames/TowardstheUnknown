using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : TacticsAttack
{
    private Inventory inventory;

    // Start is called before the first frame update
    void Start()
    {
        inventory = GetComponent<Inventory>();
    }

    // Update is called once per frame
    void Update()
    {
        UseArtifact(0);
    }

    public void UseArtifact(int num)
    {
        IArtifact artifact = inventory.LArtifacts[num];
        artifact.Launch();
    }
}
