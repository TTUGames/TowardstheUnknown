using UnityEngine.InputSystem;

public class PlayerTurn : EntityTurn
{
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
    }

    private void OnDisable()
    {
        for (int i = 0; i < skillActions.Length; i++)
            skillActions[i].performed -= skillHandlers[i];
        GameInput.Controls.Gameplay.Cancel.performed -= OnCancel;
    }

    private void OnCancel(InputAction.CallbackContext context) => OnShortcut(PlayerState.MOVE);

    /// <summary>
    /// Keyboard and mouse shortcuts only work during the player's turn in combat, with no menu open
    /// </summary>
    private void OnShortcut(PlayerState state, int artifact = 0)
    {
        if (!turnSystem.IsCombat || !turnSystem.IsPlayerTurn || GameScene.UI.IsMenuOpen) return;
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
        playerMove.SetPlayingState(true);
        if (turnSystem.IsCombat)
            AkUnitySoundEngine.PostEvent("PlayerTurn", gameObject);
    }

    /// <summary>
    /// Stop the turn
    /// </summary>
    public override void OnTurnStop()
    {
        playerMove.SetPlayingState(false);
        playerAttack.SetAttackingState(false);
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
                if (!playerMove.IsPlaying)
                {
                    playerAttack.SetAttackingState(false);
                    playerMove.SetPlayingState(true);
                }
                else
                    playerMove.FindSelectibleTiles();
                break;
            case PlayerState.ATTACK:
                if (!playerAttack.GetAttackingState())
                {
                    playerMove.SetPlayingState(false);
                    playerAttack.SetAttackingState(true);
                }
                playerAttack.SetAttackingArtifact(artifact);
                break;
        }
        SelectedArtifactChanged?.Invoke(playerAttack.GetAttackingState() ? artifact : -1);
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
        NextTurnButton.instance.EnterState(NextTurnButton.State.EXPLORATION);
        SetState(PlayerState.MOVE);
    }
}
