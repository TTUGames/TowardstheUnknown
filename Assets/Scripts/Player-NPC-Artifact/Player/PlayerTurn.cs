using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTurn : EntityTurn
{
    private PlayerMove playerMove;
    private PlayerAttack playerAttack;
    private Timer playerTimer;
    private UISkillsBar UISkillsBar;
    private InventoryManager inventoryManager;

    private Dictionary<KeyCode, int> keys;

    public enum PlayerState
    {
        ATTACK, MOVE
    }

    protected override void Init()
    {
        playerMove = GetComponent<PlayerMove>();
        playerAttack = GetComponent<PlayerAttack>();
        playerTimer = GetComponent<Timer>();
        UISkillsBar = FindObjectOfType<UISkillsBar>();
        inventoryManager = FindObjectOfType<InventoryManager>();
        keys = new Dictionary<KeyCode, int>() {
            { KeyCode.Alpha1, 0 },
            { KeyCode.Alpha2, 1 },
            { KeyCode.Alpha3, 2 },
            { KeyCode.Alpha4, 3 },
            { KeyCode.Alpha5, 4 },
            { KeyCode.Alpha6, 5 },
            { KeyCode.Alpha7, 6 },
            { KeyCode.Alpha8, 7 },
            { KeyCode.Alpha9, 8 },
        };
    }

    public override void TurnUpdate()
    {
        foreach (KeyCode key in keys.Keys)
        {
            if (Input.GetKeyDown(key))
            {
                SetState(PlayerState.ATTACK, keys[key]);
                break;
            }
        }
        if (Input.GetKeyDown(KeyCode.Mouse1)) SetState(PlayerState.MOVE);
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
            AkSoundEngine.PostEvent("PlayerTurn", gameObject);
            playerTimer.LaunchTimer();

            foreach (IArtifact artifact in inventoryManager.GetPlayerArtifacts())
            {
                artifact.TurnStart();
            }

            UISkillsBar.UpdateSkillBar();

        }
    }

    /// <summary>
    /// Stop the turn
    /// </summary>
    public override void OnTurnStop()
    {
        playerTimer.StopTimer();
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
        if (!TurnSystem.Instance.IsCombat && state != PlayerState.MOVE) return;
        if (TurnSystem.Instance.IsCombat && (!TurnSystem.Instance.IsPlayerTurn || ActionManager.IsBusy)) return;
        switch (state)
        {
            case PlayerState.MOVE:
                if (!playerMove.IsPlaying)
                {
                    playerAttack.SetAttackingState(false);
                    playerMove.SetPlayingState(true);
                }
                else playerMove.FindSelectibleTiles();
                break;
            case PlayerState.ATTACK:
                if (!turnSystem.IsCombat) return;
                if (!playerAttack.GetAttackingState())
                {
                    playerMove.SetPlayingState(false);
                    playerAttack.SetAttackingState(true);
                }
                playerAttack.SetAttackingArtifact(artifact);
                break;
        }
    }

    /// <summary>
    /// On combat end, sets the player's state to move
    /// </summary>
    public override void OnCombatEnd()
    {
        base.OnCombatEnd();
        NextTurnButton.instance.EnterState(NextTurnButton.State.EXPLORATION);
        SetState(PlayerState.MOVE);
        playerTimer.StopTimer();
    }

    public void OnCombatStart()
    {
        foreach (IArtifact artifact in inventoryManager.GetPlayerArtifacts())
        {
            artifact.CombatStart();
        }
        UISkillsBar.UpdateSkillBar();

        TimelineManager timelineManager = Object.FindObjectOfType<TimelineManager>();
        if (timelineManager != null)
            timelineManager.UpdateTimeline();
    }

}
