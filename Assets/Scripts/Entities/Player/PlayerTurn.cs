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

    /// <summary>
    /// Fired when a click on the board does nothing during the player's combat turn: a tile out of reach or out of range
    /// </summary>
    public event System.Action ClickRefused;

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
        Room.UnselectableTileClicked += OnUnselectableTileClicked;
        GameEvents.RoomLeft += StopPlaying;
    }

    private void OnDisable()
    {
        for (int i = 0; i < skillActions.Length; i++)
            skillActions[i].performed -= skillHandlers[i];
        GameInput.Controls.Gameplay.Cancel.performed -= OnCancel;
        Room.TileHovered -= OnTileHovered;
        Room.TileClicked -= OnTileClicked;
        Room.UnselectableTileClicked -= OnUnselectableTileClicked;
        GameEvents.RoomLeft -= StopPlaying;
    }

    private void OnTileHovered(Tile tile) => mode?.OnTileHovered(tile);

    private void OnTileClicked(Tile tile) => mode?.OnTileClicked(tile);

    // Out of the aimed artifact's range, any tile; while moving, an empty floor tile only: clicking an entity or a wall
    // out of reach isn't meant as a move
    private void OnUnselectableTileClicked(Tile tile)
    {
        if (IsAttacking || (tile.isWalkable && tile.GetEntity() == null)) RefuseClick(tile);
    }

    /// <summary>
    /// Blinks the clicked tile and raises <see cref="ClickRefused"/>, during the player's combat turn only
    /// </summary>
    public void RefuseClick(Tile tile)
    {
        if (mode == null || !turnSystem.IsCombat || !turnSystem.IsPlayerTurn || GameScene.IsGameplayBlocked) return;
        if (ActionManager.IsBusy && !playerAttack.IsCasting) return;
        tile.BlinkRefused();
        ClickRefused?.Invoke();
    }

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
        playerAttack.CancelQueuedCasts();
        bool wasAttacking = IsAttacking;
        SetMode(null);
        //The skills bar and the hovered enemy's threat follow the end of the aim
        if (wasAttacking) SelectedArtifactChanged?.Invoke(-1);
    }

    /// <summary>
    /// Drops the queued casts and goes back to moving
    /// </summary>
    private void OnCancel(InputAction.CallbackContext context)
    {
        if (!turnSystem.IsCombat || !turnSystem.IsPlayerTurn || GameScene.IsGameplayBlocked) return;
        playerAttack.CancelQueuedCasts();
        SetState(PlayerState.MOVE);
    }

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
        //While casting, the player can aim and queue the next casts
        if (turnSystem.IsCombat ? !turnSystem.IsPlayerTurn || ActionManager.IsBusy && !playerAttack.IsCasting : state != PlayerState.MOVE)
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
        playerAttack.CancelQueuedCasts();
        foreach (Artifact artifact in Inventory.GetPlayerArtifacts())
            artifact.ResetConstraints();
        base.OnCombatEnd();
        SetState(PlayerState.MOVE);
    }
}
