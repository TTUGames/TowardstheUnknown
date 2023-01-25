using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillClickHandler : EventTrigger
{
    private PlayerTurn playerTurn;
    private int artifactIndex;
    
    private void Awake()
    {
        playerTurn = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerTurn>();
    }

    public override void OnPointerDown(PointerEventData data)
    {
        if (!playerTurn.playerAttack.GetAttackingState())
            playerTurn.SetState(PlayerTurn.PlayerState.ATTACK, artifactIndex);
        else
            playerTurn.SetState(PlayerTurn.PlayerState.MOVE);
    }
    
    public int ArtifactIndex { get => artifactIndex; set => artifactIndex = value; }
}
