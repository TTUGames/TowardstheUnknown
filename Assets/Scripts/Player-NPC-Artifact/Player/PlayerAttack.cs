using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : TacticsAttack
{
    private bool isAttacking = false;

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
            InputListener();
        }
    }

    public void UseArtifact(int numArtifact, Vector3 position)
    {
        inventory.LArtifacts[numArtifact].Launch(position);
        isAnimationRunning = true;
    }

    /// <summary>
    /// Handler for the <c>Inputs</c>
    /// </summary>
    private void InputListener()
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

    /// <summary>
    /// Set the attacking bool to it's opposite
    /// </summary>
    /// <param name="numArtifact">the number of the <c>Artifact</c> call to attack</param>
    public void SetAttackingArtifact(int numArtifact)
    {
        maxAttackDistance = inventory.LArtifacts[numArtifact].GetMaxDistance();
        minAttackDistance = inventory.LArtifacts[numArtifact].GetMinDistance();
    }

    public void RepaintMapWithZero()
    {
        int tempMoveRemaining = maxAttackDistance;
        maxAttackDistance = 0;
        FindSelectibleTiles();
        maxAttackDistance = tempMoveRemaining;
    }

    public bool IsAnimationRunning { get => isAnimationRunning; set => isAnimationRunning = value; }

    public void SetAttackingState(bool state)
    {
        isAttacking = state;
        if (!state)
            RepaintMapWithZero();
    }

    public bool GetAttackingingState()
    {
        return isAttacking;
    }
}
