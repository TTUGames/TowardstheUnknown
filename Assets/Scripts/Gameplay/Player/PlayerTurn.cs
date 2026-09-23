using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerTurn : EntityTurn
{
    private static readonly Color selectedSkillColor = new Color32(116, 89, 216, 255);

    public PlayerMove playerMove;
    public PlayerAttack playerAttack;
    private UIEnergy uiEnergy;
    private UISkillsBar uiSkillsBar;
    private InventoryManager inventoryManager;
    private BuffDebuff buffDebuff;
    private ChangeUI changeUI;
    private InputAction[] skillActions;

    public enum PlayerState
    {
        ATTACK, MOVE
    }

    protected override void Init()
    {
        buffDebuff = FindAnyObjectByType<BuffDebuff>();
        playerMove = GetComponent<PlayerMove>();
        playerAttack = GetComponent<PlayerAttack>();
        uiEnergy = FindAnyObjectByType<UIEnergy>();
        uiSkillsBar = FindAnyObjectByType<UISkillsBar>();
        inventoryManager = FindAnyObjectByType<InventoryManager>();
        changeUI = FindAnyObjectByType<ChangeUI>();

        Controls.GameplayActions gameplay = GameInput.Controls.Gameplay;
        skillActions = new[] { gameplay.Skill1, gameplay.Skill2, gameplay.Skill3, gameplay.Skill4, gameplay.Skill5,
            gameplay.Skill6, gameplay.Skill7, gameplay.Skill8, gameplay.Skill9 };
    }

    public override void TurnUpdate()
    {
        if (changeUI.IsMenuOpen) return;
        for (int i = 0; i < skillActions.Length; i++)
        {
            if (skillActions[i].WasPressedThisFrame())
            {
                SetState(PlayerState.ATTACK, i);
                break;
            }
        }
        if (GameInput.Controls.Gameplay.Cancel.WasPressedThisFrame())
            SetState(PlayerState.MOVE);
    }

    /// <summary>
    /// Launch the turn
    /// </summary>
    public override void OnTurnLaunch()
    {
        base.OnTurnLaunch();
        playerMove.SetPlayingState(true);
        if (turnSystem.IsCombat)
        {
            AkUnitySoundEngine.PostEvent("PlayerTurn", gameObject);
            foreach (Artifact artifact in inventoryManager.GetPlayerArtifacts())
                artifact.TurnStart();
            uiEnergy.UpdateEnergyUI();
            uiSkillsBar.UpdateSkillBar();
        }
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
        UpdateSkillClickHandlersColor(artifact);
    }

    private void UpdateSkillClickHandlersColor(int artifactIndex) {
        bool isAttacking = playerAttack.GetAttackingState();
        foreach (SkillClickHandler handler in FindObjectsByType<SkillClickHandler>())
            handler.GetComponent<Image>().color = isAttacking && handler.artifactIndex == artifactIndex ? selectedSkillColor : Color.white;
    }

    /// <summary>
    /// On combat end, sets the player's state to move and updates the UI
    /// </summary>
    public override void OnCombatEnd()
    {
        base.OnCombatEnd();
        uiEnergy.UpdateEnergyUI();
        foreach (Artifact artifact in inventoryManager.GetPlayerArtifacts())
            artifact.ResetConstraints();
        uiSkillsBar.UpdateSkillBar();
        buffDebuff.DisplayBuffDebuff();
        NextTurnButton.instance.EnterState(NextTurnButton.State.EXPLORATION);
        SetState(PlayerState.MOVE);
    }

    public void OnCombatStart()
    {
        TimelineManager timelineManager = FindAnyObjectByType<TimelineManager>();
        if (timelineManager != null)
            timelineManager.UpdateTimeline();
    }
}
