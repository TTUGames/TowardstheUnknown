using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : TacticsAttack
{
    private bool isAttacking = false;

    private Inventory inventory;
    private bool isAnimationRunning;
    private PlayerStats playerStats;
    private PlayerTurn playerTurn;

    private IArtifact currentArtifact;
    

    // Start is called before the first frame update
    void Start()
    {
        inventory = GetComponent<Inventory>();
        playerStats = GetComponent<PlayerStats>();
        playerTurn = GetComponent<PlayerTurn>();
        isAnimationRunning = false;
        Init();
    }

    private void DisplayTargets(Tile hoveredTile) {
        Tile.ResetTargetTiles();
        foreach (Tile tile in currentArtifact.GetTargets(Tile.GetHoveredTile())) tile.isTarget = true;
    }

    /// <summary>
    /// Launch the attack with the selected <c>Artifact</c>
    /// </summary>
    /// <param name="hitTerrain">The position where the player clicked</param>
    public void Attack(Tile tile)
    {
        if (currentArtifact.CanTarget(tile))
        {
            currentArtifact.Launch(playerStats, tile, animator);
            isAnimationRunning = true;
        }
        TryDisplayArtifactRange();
    }

    /// <summary>
    /// Set the attacking bool to it's opposite
    /// </summary>
    /// <param name="numArtifact">the number of the <c>Artifact</c> call to attack</param>
    public void SetAttackingArtifact(int numArtifact)
    {
        if (numArtifact >= inventory.LArtifacts.Count) {
            playerTurn.SetState(PlayerTurn.PlayerState.MOVE);
        }
        else {
            currentArtifact = inventory.LArtifacts[numArtifact];

            TryDisplayArtifactRange();
        }
    }

    /// <summary>
    /// Checks if the currentArtifact can still be cast, and sets its range if it can. Else, does to move state.
    /// </summary>
    private void TryDisplayArtifactRange() {
        if (!currentArtifact.CanUse(playerStats)) {
            playerTurn.SetState(PlayerTurn.PlayerState.MOVE);
            return;
        }
        Tile.ResetTiles();

        maxAttackDistance = currentArtifact.GetRange().maxRange;
        minAttackDistance = currentArtifact.GetRange().minRange;
        rangeType = currentArtifact.GetRange().type;

        FindSelectibleTiles(maxAttackDistance, minAttackDistance);
    }

    /// <summary>
    /// Repaint the map with 0 attack distance <br/>
    /// used to reset the <c>Tile</c> color before switching to attack mode
    /// </summary>
    public void SetAttackingState(bool state)
    {
        isAttacking = state;
        if (state) {
            Room.currentRoom.newTileHovered.AddListener(DisplayTargets);
            Room.currentRoom.tileClicked.AddListener(Attack);
            ActionManager.queueFree.AddListener(TryDisplayArtifactRange);
        }
        else {
            Room.currentRoom.newTileHovered.RemoveListener(DisplayTargets);
            Room.currentRoom.tileClicked.RemoveListener(Attack);
            ActionManager.queueFree.RemoveListener(TryDisplayArtifactRange);
            Tile.ResetTiles();
        }
    }

    public bool GetAttackingState()
    {
        return isAttacking;
    }

    public bool IsAnimationRunning { get => isAnimationRunning; set => isAnimationRunning = value; }
}
