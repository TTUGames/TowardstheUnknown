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
        Debug.Log("Clicked " + artifactIndex);
        playerTurn.SetState(PlayerTurn.PlayerState.ATTACK, artifactIndex);
    }
    
    public int ArtifactIndex { get => artifactIndex; set => artifactIndex = value; }
}
