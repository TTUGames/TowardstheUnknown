using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : TacticsAttack
{
    private Inventory inventory;
    private bool isAnimationRunning;

    // Start is called before the first frame update
    void Start()
    {
        inventory = GetComponent<Inventory>();
        isAnimationRunning = false;
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (isAttacking)
        {
            FindSelectibleTiles();
            MouseListener();
        }
    }

    public void UseArtifact(int numArtifact, Vector3 position)
    {
        inventory.LArtifacts[numArtifact].Launch(position);
        isAnimationRunning = true;
    }

    private void MouseListener()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            int layerTerrain = LayerMask.NameToLayer("Terrain");

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << layerTerrain))
            {
                Tile t = hit.collider.GetComponent<Tile>();

                if (t.isSelectable)
                {
                    UseArtifact(0, hit.transform.position);
                }
            }
        }
    }
    

    public void setAttacking(int numArtifact)
    {
        if (!isAttacking)
        {
            maxAttackDistance = inventory.LArtifacts[numArtifact].GetMaxDistance();
            minAttackDistance = inventory.LArtifacts[numArtifact].GetMinDistance();
            isAttacking = true;
        }
        else
            isAttacking = false;
        
    }
}
