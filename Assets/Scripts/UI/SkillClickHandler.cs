using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class SkillClickHandler : EventTrigger
{
    private PlayerTurn playerTurn;
    private InventoryManager inventory;
    public int artifactIndex;
    private TextMeshProUGUI tooltip;
    private GameObject tooltipContainer;
    private bool isPointerInside;

    private void Awake()
    {
        inventory = FindObjectOfType<InventoryManager>();
        playerTurn = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerTurn>();

        tooltipContainer = GameObject.Find("UI/Canvas | MainUI/TooltipContainer");
        tooltip = tooltipContainer.GetComponentInChildren<TextMeshProUGUI>();
        tooltipContainer.SetActive(false);
    }


    public override void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;
        Invoke("ShowTooltip", 0.5f); 
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        tooltipContainer.SetActive(false);
        isPointerInside = false;
        tooltip.text = "";
    }

    public override void OnPointerDown(PointerEventData data)
    {
        if (!playerTurn.playerAttack.GetAttackingState() || playerTurn.playerAttack.currentArtifact != playerTurn.playerAttack.inventory.GetPlayerArtifacts()[artifactIndex])
            playerTurn.SetState(PlayerTurn.PlayerState.ATTACK, artifactIndex);
        else
            playerTurn.SetState(PlayerTurn.PlayerState.MOVE);
    }
    
    private void ShowTooltip()
    {
        if (isPointerInside) // Vérifie si la souris est toujours sur l'objet
        {
            tooltipContainer.SetActive(true);
            tooltip.text = inventory.GetPlayerArtifacts()[artifactIndex].GetEffectDescription().ToString();
        }
    }

    public int ArtifactIndex { get => artifactIndex; set => artifactIndex = value; }
}
