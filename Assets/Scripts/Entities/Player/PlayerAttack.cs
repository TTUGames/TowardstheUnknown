using UnityEngine;

/// <summary>
/// The attack mode of the player: shows the range of the selected artifact and its targets under the pointer, and casts it on the clicked tile
/// </summary>
public class PlayerAttack : TacticsAttack, IPlayerMode
{
    public InventoryManager inventory;
    private PlayerStats playerStats;
    private PlayerTurn playerTurn;

    public Artifact currentArtifact { get; private set; }

    [SerializeField] private Transform leftHandMarker;
    [SerializeField] private Transform rightHandMarker;
    [SerializeField] private Transform gunMarker;
    [SerializeField] private Transform swordMarker;
    [SerializeField] private Transform backMarker;

    private Dissolving dissolving;
    private ChangeColor changeColor;

    protected override void Init()
    {
        base.Init();
        inventory = GetComponent<InventoryManager>();
        playerStats = GetComponent<PlayerStats>();
        playerTurn = GetComponent<PlayerTurn>();
        dissolving = GetComponent<Dissolving>();
        changeColor = GetComponent<ChangeColor>();
    }

    /// <summary>
    /// Shows the tiles the artifact would hit
    /// </summary>
    public void OnTileHovered(Tile hoveredTile)
    {
        Tile.ResetTargetTiles();
        if (hoveredTile == null || hoveredTile.Selection != Tile.SelectionType.ATTACK) return;
        foreach (Tile tile in currentArtifact.GetTargets(hoveredTile)) tile.IsTarget = true;
    }

    public void OnTileClicked(Tile tile) => Attack(tile);

    /// <summary>
    /// Launch the attack with the selected <c>Artifact</c>, then goes back to moving once its actions are done
    /// </summary>
    /// <param name="tile">The tile the player clicked</param>
    public void Attack(Tile tile)
    {
        if (!currentArtifact.CanTarget(tile)) return;
        changeColor.Colorize(currentArtifact.Color);
        dissolving.Undissolve(currentArtifact.Weapon);
        currentArtifact.Launch(playerStats, tile); //Spending energy refreshes the energy and skills UI
        Tile.ResetTiles();
        ActionManager.WhenFree(OnAttackEnd);
    }

    /// <summary>
    /// Selects the artifact to attack with
    /// </summary>
    /// <param name="numArtifact">the index of the <c>Artifact</c> to attack with</param>
    public void SetAttackingArtifact(int numArtifact)
    {
        var artifacts = inventory.GetPlayerArtifacts();
        if (numArtifact >= artifacts.Count)
        {
            playerTurn.SetState(PlayerTurn.PlayerState.MOVE);
            return;
        }
        currentArtifact = artifacts[numArtifact];
        CheckAndPreviewArtifact();
    }

    /// <summary>
    /// Checks if the currentArtifact can still be cast, and previews its range and energy cost if it can. Else, does to move state.
    /// </summary>
    private void CheckAndPreviewArtifact()
    {
        if (!currentArtifact.CanUse(playerStats))
        {
            playerStats.PreviewEnergyCost(0);
            playerTurn.SetState(PlayerTurn.PlayerState.MOVE);
            return;
        }
        Tile.ResetTiles();

        FindSelectibleTiles(currentArtifact.Range);
        if (selectableTiles.Contains(Room.HoveredTile))
            OnTileHovered(Room.HoveredTile);
        playerStats.PreviewEnergyCost(currentArtifact.Cost);
    }

    private void OnAttackEnd() {
        //The player can die from its own attack
        if (playerStats.CurrentHealth <= 0) return;
        playerStats.PreviewEnergyCost(0);
        playerTurn.SetState(PlayerTurn.PlayerState.MOVE);
        dissolving.Start();
    }

    // The range is shown once an artifact is selected
    public void Enter() { }

    public void Exit() => Tile.ResetTiles();

    /// <summary>
    /// Ends the visuals of an attack, called when any attack animation ends
    /// </summary>
    public void EndAttackVisuals()
    {
        changeColor.Uncolorize();
        dissolving.DissolveAll();
    }

    public Transform LeftHandMarker => leftHandMarker;
    public Transform RightHandMarker => rightHandMarker;
    public Transform SwordMarker => swordMarker;
    public Transform GunMarker => gunMarker;
    public Transform BackMarker => backMarker;

    public PlayerStats Stats => playerStats;
}
