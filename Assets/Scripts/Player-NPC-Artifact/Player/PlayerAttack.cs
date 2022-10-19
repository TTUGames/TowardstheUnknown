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
        foreach (Tile tile in currentArtifact.GetTargets()) tile.isTarget = true;
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
        CheckIfArtifactIsValid();
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

            CheckIfArtifactIsValid();
            if (!isAttacking) return;

            maxAttackDistance = currentArtifact.GetRange().maxRange;
            minAttackDistance = currentArtifact.GetRange().minRange;
            rangeType = currentArtifact.GetRange().type;

            if (isAttacking) FindSelectibleTiles(maxAttackDistance, minAttackDistance);
        }
    }

    /// <summary>
    /// Checks if the currentArtifact can still be cast, and goes to move state if not.
    /// </summary>
    private void CheckIfArtifactIsValid() {
        if (!currentArtifact.CanUse(playerStats)) playerTurn.SetState(PlayerTurn.PlayerState.MOVE);
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
        }
        else {
            Room.currentRoom.newTileHovered.RemoveListener(DisplayTargets);
            Room.currentRoom.tileClicked.RemoveListener(Attack);
            Tile.ResetTiles();
        }
    }

    public bool GetAttackingState()
    {
        return isAttacking;
    }

    public bool IsAnimationRunning { get => isAnimationRunning; set => isAnimationRunning = value; }
}
