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
                    Attack(hit);
                }
            }
        }
    }

    /// <summary>
    /// Launch the attack with the selected <c>Artifact</c>
    /// </summary>
    /// <param name="hitTerrain">The position where the player clicked</param>
    public void Attack(RaycastHit hitTerrain)
    {
        inventory.LArtifacts[0].Launch(hitTerrain);
        isAnimationRunning = true;
    }

    /// <summary>
    /// Repaint the map with 0 attack distance <br/>
    /// used to reset the <c>Tile</c> color before switching to move mode
    /// </summary>
    public void RepaintMapWithZero()
    {
        int tempMoveRemaining = maxAttackDistance;
        maxAttackDistance = 0;
        FindSelectibleTiles();
        maxAttackDistance = tempMoveRemaining;
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

    public bool IsAnimationRunning { get => isAnimationRunning; set => isAnimationRunning = value; }
}
