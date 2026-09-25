using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class SkillClickHandler : EventTrigger
{
    private PlayerTurn playerTurn;
    public int artifactIndex;
    private TextMeshProUGUI tooltip;
    private GameObject tooltipContainer;
    private bool isPointerInside;

    /// <summary>
    /// Sets the artifact this skill selects, and the tooltip showing its effects
    /// </summary>
    public void Init(PlayerTurn playerTurn, int artifactIndex, GameObject tooltipContainer)
    {
        this.playerTurn = playerTurn;
        this.artifactIndex = artifactIndex;
        this.tooltipContainer = tooltipContainer;
        tooltip = tooltipContainer.GetComponentInChildren<TextMeshProUGUI>(true);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;
        Invoke(nameof(ShowTooltip), 0.5f);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        tooltipContainer.SetActive(false);
        isPointerInside = false;
        tooltip.text = "";
    }

    public override void OnPointerDown(PointerEventData data)
    {
        PlayerAttack playerAttack = playerTurn.playerAttack;
        if (!playerAttack.GetAttackingState() || playerAttack.currentArtifact != playerTurn.Inventory.GetPlayerArtifacts()[artifactIndex])
            playerTurn.SetState(PlayerTurn.PlayerState.ATTACK, artifactIndex);
        else
            playerTurn.SetState(PlayerTurn.PlayerState.MOVE);
    }

    private IEnumerator CheckTooltipActive()
    {
        while (tooltipContainer.activeSelf)
        {
            if (GameScene.UI.uIIsOpen)
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
            tooltip.text = playerTurn.Inventory.GetPlayerArtifacts()[artifactIndex].EffectDescription;
            StartCoroutine(CheckTooltipActive());
        }
    }

}
