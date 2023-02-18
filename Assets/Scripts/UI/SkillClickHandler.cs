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
    private ChangeUI changeUI;

    private void Awake()
    {
        inventory = FindObjectOfType<InventoryManager>();
        playerTurn = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerTurn>();
        tooltipContainer = GameObject.Find("UI/Canvas | MainUI/TooltipContainer");
        tooltip = tooltipContainer.GetComponentInChildren<TextMeshProUGUI>();
        changeUI = FindObjectOfType<ChangeUI>();
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

    private IEnumerator CheckTooltipActive()
    {
        while (tooltipContainer.activeSelf)
        {
            if (changeUI.uIIsOpen)
            {
                tooltipContainer.SetActive(false);
                break;
            }
            yield return null;
        }
    }

    private void ShowTooltip()
    {
        if (isPointerInside)
        {
            tooltipContainer.SetActive(true);
            tooltip.text = inventory.GetPlayerArtifacts()[artifactIndex].GetEffectDescription().ToString();
            StartCoroutine(CheckTooltipActive());
        }
    }

    public int ArtifactIndex { get => artifactIndex; set => artifactIndex = value; }
}
