using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTurn : EntityTurn
{
    [SerializeField, Tooltip("Posted when a combat turn of the player starts")] private AK.Wwise.Event turnStartSound = new AK.Wwise.Event();

    public PlayerMove playerMove;
    public PlayerAttack playerAttack;
    private InputAction[] skillActions;
    private System.Action<InputAction.CallbackContext>[] skillHandlers;

    public enum PlayerState
    {
        ATTACK, MOVE
    }

    /// <summary>
    /// Fired with the index of the artifact the player attacks with when the state changes, -1 when moving
    /// </summary>
    public event System.Action<int> SelectedArtifactChanged;

    private InventoryManager inventory;
    private PlayerStats playerStats;
    private IPlayerMode mode;

    /// <summary>
    /// Whether the player attacks with an artifact, rather than moving
    /// </summary>
    public bool IsAttacking => mode == (IPlayerMode)playerAttack;

    //Resolved on first use: the HUD reads them in its Awake, which may run before this one
    public InventoryManager Inventory => inventory != null ? inventory : inventory = GetComponent<InventoryManager>();
    public PlayerStats Stats => playerStats != null ? playerStats : playerStats = GetComponent<PlayerStats>();

    protected override void Init()
    {
        playerMove = GetComponent<PlayerMove>();
        playerAttack = GetComponent<PlayerAttack>();

        Controls.GameplayActions gameplay = GameInput.Controls.Gameplay;
        skillActions = new[] { gameplay.Skill1, gameplay.Skill2, gameplay.Skill3, gameplay.Skill4, gameplay.Skill5,
            gameplay.Skill6, gameplay.Skill7, gameplay.Skill8, gameplay.Skill9 };
        skillHandlers = new System.Action<InputAction.CallbackContext>[skillActions.Length];
        for (int i = 0; i < skillActions.Length; i++)
        {
            int artifactIndex = i;
            skillHandlers[i] = _ => OnShortcut(PlayerState.ATTACK, artifactIndex);
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < skillActions.Length; i++)
            skillActions[i].performed += skillHandlers[i];
        GameInput.Controls.Gameplay.Cancel.performed += OnCancel;
        Room.TileHovered += OnTileHovered;
        Room.TileClicked += OnTileClicked;
        GameEvents.RoomLeft += StopPlaying;
    }

    private void OnDisable()
    {
        for (int i = 0; i < skillActions.Length; i++)
            skillActions[i].performed -= skillHandlers[i];
        GameInput.Controls.Gameplay.Cancel.performed -= OnCancel;
        Room.TileHovered -= OnTileHovered;
        Room.TileClicked -= OnTileClicked;
        GameEvents.RoomLeft -= StopPlaying;
    }

    private void OnTileHovered(Tile tile) => mode?.OnTileHovered(tile);

    private void OnTileClicked(Tile tile) => mode?.OnTileClicked(tile);

    /// <summary>
    /// Leaves the current mode and enters the next one, none if null
    /// </summary>
    private void SetMode(IPlayerMode next)
    {
        mode?.Exit();
        mode = next;
        mode?.Enter();
    }

    /// <summary>
    /// The board ignores the player until its next turn
    /// </summary>
    private void StopPlaying()
    {
        bool wasAttacking = IsAttacking;
        SetMode(null);
        //The skills bar and the hovered enemy's threat follow the end of the aim
        if (wasAttacking) SelectedArtifactChanged?.Invoke(-1);
    }

    private void OnCancel(InputAction.CallbackContext context) => OnShortcut(PlayerState.MOVE);

    /// <summary>
    /// Keyboard and mouse shortcuts only work during the player's turn in combat, with no menu open
    /// </summary>
    private void OnShortcut(PlayerState state, int artifact = 0)
    {
        if (!turnSystem.IsCombat || !turnSystem.IsPlayerTurn || GameScene.IsGameplayBlocked) return;
        SetState(state, artifact);
    }

    /// <summary>
    /// Launch the turn
    /// </summary>
    public override void OnTurnLaunch()
    {
        //Before the energy refill, which refreshes the skills bar
        if (turnSystem.IsCombat)
            foreach (Artifact artifact in Inventory.GetPlayerArtifacts())
                artifact.TurnStart();
        base.OnTurnLaunch();
        SetMode(playerMove);
        if (turnSystem.IsCombat)
            turnStartSound.Post(gameObject);
    }

    /// <summary>
    /// Stop the turn
    /// </summary>
    public override void OnTurnStop()
    {
        StopPlaying();
        base.OnTurnStop();
    }

    /// <summary>
    /// Sets the player's state among the <c>PlayerState</c>
    /// </summary>
    /// <param name="state">The player's new state</param>
    /// <param name="artifact">If attacking, the artifact's index</param>
    public void SetState(PlayerState state, int artifact = 0)
    {
        if (turnSystem.IsCombat ? !turnSystem.IsPlayerTurn || ActionManager.IsBusy : state != PlayerState.MOVE)
            return;
        switch (state)
        {
            case PlayerState.MOVE:
                if (mode == (IPlayerMode)playerMove) playerMove.FindSelectibleTiles();
                else SetMode(playerMove);
                break;
            case PlayerState.ATTACK:
                if (!IsAttacking) SetMode(playerAttack);
                playerAttack.SetAttackingArtifact(artifact);
                break;
        }
        SelectedArtifactChanged?.Invoke(IsAttacking ? artifact : -1);
    }

    /// <summary>
    /// On combat end, sets the player's state to move and updates the UI
    /// </summary>
    public override void OnCombatEnd()
    {
        //Before the energy refill, which refreshes the skills bar
        foreach (Artifact artifact in Inventory.GetPlayerArtifacts())
            artifact.ResetConstraints();
        base.OnCombatEnd();
        SetState(PlayerState.MOVE);
    }
}
