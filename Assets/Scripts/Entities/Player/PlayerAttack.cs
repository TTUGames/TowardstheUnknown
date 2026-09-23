using UnityEngine;

public class PlayerAttack : TacticsAttack
{
    private bool isAttacking = false;

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
    private ChangeUI changeUI;

    protected override void Init()
    {
        base.Init();
        inventory = FindAnyObjectByType<InventoryManager>();
        playerStats = GetComponent<PlayerStats>();
        playerTurn = GetComponent<PlayerTurn>();
        dissolving = GetComponent<Dissolving>();
        changeColor = GetComponent<ChangeColor>();
        changeUI = FindAnyObjectByType<ChangeUI>();
    }

    private void DisplayTargets(Tile hoveredTile)
    {
        Tile.ResetTargetTiles();
        foreach (Tile tile in currentArtifact.GetTargets(hoveredTile)) tile.IsTarget = true;
    }

    /// <summary>
    /// Launch the attack with the selected <c>Artifact</c>
    /// </summary>
    /// <param name="tile">The tile the player clicked</param>
    public void Attack(Tile tile)
    {
        if (changeUI.IsMenuOpen || !currentArtifact.CanTarget(tile)) return;
        changeColor.Colorize(currentArtifact.Color);
        dissolving.Undissolve(currentArtifact.Weapon);
        currentArtifact.Launch(this, tile); //Spending energy refreshes the energy and skills UI
        AkUnitySoundEngine.PostEvent("Player_" + currentArtifact.ID, gameObject);
        Tile.ResetTiles();
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
        if (selectableTiles.Contains(Room.currentRoom.hoveredTile))
            DisplayTargets(Room.currentRoom.hoveredTile);
        playerStats.PreviewEnergyCost(currentArtifact.Cost);
    }

    private void OnAttackEnd() {
        //The player can die from its own attack
        if (playerStats.CurrentHealth <= 0) {
            ActionManager.QueueFree -= OnAttackEnd;
            return;
        }
        playerStats.PreviewEnergyCost(0);
        playerTurn.SetState(PlayerTurn.PlayerState.MOVE);
        dissolving.Start();
    }

    /// <summary>
    /// Enters or leaves the attack mode, (un)subscribing to the room's tile events
    /// </summary>
    public void SetAttackingState(bool state)
    {
        isAttacking = state;
        if (state)
        {
            Room.currentRoom.newTileHovered.AddListener(DisplayTargets);
            Room.currentRoom.tileClicked.AddListener(Attack);
            ActionManager.QueueFree += OnAttackEnd;
        }
        else
        {
            Room.currentRoom.newTileHovered.RemoveListener(DisplayTargets);
            Room.currentRoom.tileClicked.RemoveListener(Attack);
            ActionManager.QueueFree -= OnAttackEnd;
            Tile.ResetTiles();
        }
    }

    public bool GetAttackingState()
    {
        return isAttacking;
    }

    public Transform LeftHandMarker => leftHandMarker;
    public Transform RightHandMarker => rightHandMarker;
    public Transform SwordMarker => swordMarker;
    public Transform GunMarker => gunMarker;
    public Transform BackMarker => backMarker;

    public PlayerStats Stats => playerStats;
}
