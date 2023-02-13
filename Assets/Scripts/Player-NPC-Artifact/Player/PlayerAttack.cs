using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : TacticsAttack
{
    private bool isAttacking = false;

    public InventoryManager inventory;
    private bool isAnimationRunning;
    private PlayerStats playerStats;
    private PlayerTurn playerTurn;

    public IArtifact currentArtifact;

    [SerializeField] private Transform leftHandMarker;
    [SerializeField] private Transform rightHandMarker;
    [SerializeField] private Transform gunMarker;
    [SerializeField] private Transform swordMarker;

    private UIEnergy uiEnergy;
    private 

    // Start is called before the first frame update
    void Start()
    {
        inventory = FindObjectOfType<InventoryManager>();
        playerStats = GetComponent<PlayerStats>();
        playerTurn = GetComponent<PlayerTurn>();
        isAnimationRunning = false;

        uiEnergy = FindObjectOfType<UIEnergy>();

        Init();
    }

    private void DisplayTargets(Tile hoveredTile)
    {
        Tile.ResetTargetTiles();
        foreach (Tile tile in currentArtifact.GetTargets(Tile.GetHoveredTile())) tile.IsTarget = true;
    }

    /// <summary>
    /// Launch the attack with the selected <c>Artifact</c>
    /// </summary>
    /// <param name="hitTerrain">The position where the player clicked</param>
    public void Attack(Tile tile)
    {
        if (currentArtifact.CanTarget(tile))
        {
            GetComponent<ChangeColor>().Colorize(currentArtifact.GetColor());
            GetComponent<Dissolving>().Undissolve(currentArtifact.GetWeapon());
            currentArtifact.Launch(this, tile);
            AkSoundEngine.PostEvent("Player_" + currentArtifact.GetType().Name, gameObject);
            isAnimationRunning = true;
            Tile.ResetTiles();
            FindObjectOfType<UIEnergy>().UpdateEnergyUI(); //Refresh the UIEnergy after the attack is done
            FindObjectOfType<UISkillsBar>().UpdateSkillBar(); //Refresh the UISkills after the attack is done
        }
    }

    /// <summary>
    /// Set the attacking bool to it's opposite
    /// </summary>
    /// <param name="numArtifact">the number of the <c>Artifact</c> call to attack</param>
    public void SetAttackingArtifact(int numArtifact)
    {
        if (numArtifact >= inventory.GetPlayerArtifacts().Count)
        {
            playerTurn.SetState(PlayerTurn.PlayerState.MOVE);
        }
        else
        {
            currentArtifact = inventory.GetPlayerArtifacts()[numArtifact];

            CheckAndPreviewArtifact();
        }
    }

    /// <summary>
    /// Checks if the currentArtifact can still be cast, and previews its range and energy cost if it can. Else, does to move state.
    /// </summary>
    private void CheckAndPreviewArtifact()
    {
        if (!currentArtifact.CanUse(playerStats))
        {
            uiEnergy.SetPreviewedEnergy(0);
            playerTurn.SetState(PlayerTurn.PlayerState.MOVE);
            return;
        }
        Tile.ResetTiles();

        FindSelectibleTiles(currentArtifact.GetRange());
        if (selectableTiles.GetTiles().Contains(Room.currentRoom.hoveredTile))
        {
            DisplayTargets(Room.currentRoom.hoveredTile);
        }
        uiEnergy.SetPreviewedEnergy(currentArtifact.GetCost());
    }

    /// <summary>
    /// Repaint the map with 0 attack distance <br/>
    /// used to reset the <c>Tile</c> color before switching to attack mode
    /// </summary>
    public void SetAttackingState(bool state)
    {
        isAttacking = state;
        if (state)
        {
            Room.currentRoom.newTileHovered.AddListener(DisplayTargets);
            Room.currentRoom.tileClicked.AddListener(Attack);
            ActionManager.queueFree.AddListener(CheckAndPreviewArtifact);
        }
        else
        {
            Room.currentRoom.newTileHovered.RemoveListener(DisplayTargets);
            Room.currentRoom.tileClicked.RemoveListener(Attack);
            ActionManager.queueFree.RemoveListener(CheckAndPreviewArtifact);
            Tile.ResetTiles();
        }
    }

    public bool GetAttackingState()
    {
        return isAttacking;
    }

    public bool IsAnimationRunning { get => isAnimationRunning; set => isAnimationRunning = value; }

    public Transform LeftHandMarker { get => leftHandMarker; }
    public Transform RightHandMarker { get => rightHandMarker; }
    public Transform SwordMarker { get => swordMarker; }
    public Transform GunMarker { get => gunMarker; }

    public PlayerStats Stats { get => playerStats; }
}
