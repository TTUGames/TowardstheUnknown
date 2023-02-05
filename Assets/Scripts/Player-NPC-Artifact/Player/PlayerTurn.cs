using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerTurn : EntityTurn
{
    public PlayerMove playerMove;
    public PlayerAttack playerAttack;
    private UIEnergy UIEnergy;
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
        UIEnergy = FindObjectOfType<UIEnergy>();
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
        if (Input.GetKeyDown(KeyCode.Mouse1))
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
            AkSoundEngine.PostEvent("PlayerTurn", gameObject);

            foreach (IArtifact artifact in inventoryManager.GetPlayerArtifacts())
            {
                artifact.TurnStart();
            }
            UIEnergy.UpdateEnergyUI();
            UISkillsBar.UpdateSkillBar();
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
        if (!TurnSystem.Instance.IsCombat && state != PlayerState.MOVE)
            return;
        if (TurnSystem.Instance.IsCombat && (!TurnSystem.Instance.IsPlayerTurn || ActionManager.IsBusy))
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
                if (!turnSystem.IsCombat)
                    return;
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
        SkillClickHandler[] handlers = Object.FindObjectsOfType<SkillClickHandler>();

        foreach (SkillClickHandler handler in handlers) {
            if (handler.artifactIndex == artifactIndex && playerAttack.GetAttackingState()) {
                handler.gameObject.GetComponent<Image>().color = new Color32(116, 89, 216, 255);
            } else {
                handler.gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            }
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
    }

    public void OnCombatStart()
    {
        foreach (IArtifact artifact in inventoryManager.GetPlayerArtifacts())
        {
            artifact.CombatStart();
        }
        UIEnergy.UpdateEnergyUI();
        UISkillsBar.UpdateSkillBar();

        TimelineManager timelineManager = Object.FindObjectOfType<TimelineManager>();
        if (timelineManager != null)
            timelineManager.UpdateTimeline();
    }

}
