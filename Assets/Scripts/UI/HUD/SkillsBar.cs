using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

/// <summary>
/// The skills of the artifacts in the player's inventory. Clicking one attacks with it, hovering it shows its effects
/// </summary>
public class SkillsBar : IDisposable
{
    private const long TooltipDelay = 500;

    private readonly VisualElement root;
    private readonly Label tooltip;
    private readonly PlayerTurn player;
    private readonly FilterFunctionDefinition slantedBlur;
    private readonly List<VisualElement> skills = new();
    private IVisualElementScheduledItem showTooltip;

    public SkillsBar(VisualElement root, Label tooltip, PlayerTurn player, FilterFunctionDefinition slantedBlur)
    {
        this.root = root;
        this.tooltip = tooltip;
        this.player = player;
        this.slantedBlur = slantedBlur;
        // The tooltip hides when a menu opens over it
        tooltip.schedule.Execute(() => {
            if (GameScene.UI.IsMenuOpen) HideTooltip();
        }).Every(100);

        //The inventory fills the bar on its first update
        player.Stats.EnergyChanged += Refresh;
        player.Inventory.ArtifactsChanged += Refresh;
        player.SelectedArtifactChanged += Highlight;
    }

    public void Dispose()
    {
        // The player can be destroyed first when the scene unloads, taking its events with it
        if (player == null) return;
        player.Stats.EnergyChanged -= Refresh;
        player.Inventory.ArtifactsChanged -= Refresh;
        player.SelectedArtifactChanged -= Highlight;
    }

    /// <summary>
    /// Highlights the skill of the artifact the player attacks with, none if its index is -1
    /// </summary>
    private void Highlight(int artifactIndex)
    {
        for (int i = 0; i < skills.Count; i++)
            skills[i].EnableInClassList("selected", i == artifactIndex);
    }

    private void Refresh()
    {
        root.Clear();
        skills.Clear();
        HideTooltip();
        List<Artifact> artifacts = player.Inventory.GetPlayerArtifacts();
        for (int i = 0; i < artifacts.Count; i++)
            skills.Add(CreateSkill(artifacts[i], i));
        CutShape.AttachAll(root, slantedBlur);
    }

    private VisualElement CreateSkill(Artifact artifact, int index)
    {
        var skill = new VisualElement();
        skill.AddToClassList("skill");
        skill.AddToClassList(CutShape.ClassName);
        skill.EnableInClassList("unusable", !artifact.CanUse(player.Stats));

        if (artifact.SkillBarIcon != null)
        {
            var icon = new VisualElement { pickingMode = PickingMode.Ignore };
            icon.AddToClassList("skill__icon");
            icon.style.backgroundImage = new StyleBackground(artifact.SkillBarIcon);
            skill.Add(icon);
        }

        var cooldown = new Label(artifact.RemainingCooldown == 0 ? "" : artifact.RemainingCooldown.ToString()) { pickingMode = PickingMode.Ignore };
        cooldown.AddToClassList("skill__cooldown");
        skill.Add(cooldown);

        var cost = new VisualElement { pickingMode = PickingMode.Ignore };
        cost.AddToClassList("skill__cost");
        cost.AddToClassList(CutShape.ClassName);
        cost.Add(new Label(artifact.Cost.ToString()));
        skill.Add(cost);

        skill.RegisterCallback<PointerDownEvent>(_ => Select(index));
        skill.RegisterCallback<PointerEnterEvent>(_ => {
            showTooltip?.Pause();
            showTooltip = tooltip.schedule.Execute(() => ShowTooltip(artifact)).StartingIn(TooltipDelay);
        });
        skill.RegisterCallback<PointerLeaveEvent>(_ => HideTooltip());
        root.Add(skill);
        return skill;
    }

    /// <summary>
    /// Attacks with the artifact, or goes back to moving if the player already attacks with it
    /// </summary>
    private void Select(int index)
    {
        PlayerAttack attack = player.playerAttack;
        if (!attack.GetAttackingState() || attack.currentArtifact != player.Inventory.GetPlayerArtifacts()[index])
            player.SetState(PlayerTurn.PlayerState.ATTACK, index);
        else
            player.SetState(PlayerTurn.PlayerState.MOVE);
    }

    private void ShowTooltip(Artifact artifact)
    {
        if (GameScene.UI.IsMenuOpen) return;
        tooltip.text = artifact.EffectDescription;
        tooltip.AddToClassList("shown");
    }

    private void HideTooltip()
    {
        showTooltip?.Pause();
        tooltip.RemoveFromClassList("shown");
    }
}
