using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

/// <summary>
/// The skills of the artifacts in the player's inventory. Clicking one attacks with it, hovering it shows its effects
/// </summary>
public class SkillsBar : IDisposable
{
    private const long TooltipDelay = 400;
    // Between the appearance of two skills
    private const long StaggerDelay = 50;

    private readonly VisualElement root;
    private readonly Label tooltip;
    private readonly PlayerTurn player;
    private readonly List<SkillSlot> skills = new();
    private IVisualElementScheduledItem showTooltip;

    public SkillsBar(VisualElement root, Label tooltip, PlayerTurn player)
    {
        this.root = root;
        this.tooltip = tooltip;
        this.player = player;
        // The tooltip hides when a menu opens over it
        tooltip.schedule.Execute(() => {
            if (GameScene.IsGameplayBlocked) HideTooltip();
        }).Every(100);

        player.Stats.EnergyChanged += Refresh;
        player.Inventory.ArtifactsChanged += Refresh;
        player.SelectedArtifactChanged += Highlight;
        player.playerAttack.ArtifactRefused += Refuse;
        // The inventory may have been filled before the HUD was built
        Refresh();
    }

    public void Dispose()
    {
        // The player can be destroyed first when the scene unloads, taking its events with it
        if (player == null) return;
        player.Stats.EnergyChanged -= Refresh;
        player.Inventory.ArtifactsChanged -= Refresh;
        player.SelectedArtifactChanged -= Highlight;
        player.playerAttack.ArtifactRefused -= Refuse;
    }

    // The skill shakes sideways, then settles
    private static readonly float[] refuseShake = { -7, 7, -5, 4, -2, 0 };
    private const long RefuseStep = 45;

    /// <summary>
    /// Shakes the skill of an artifact the player can't cast, its cost or cooldown in the accent color
    /// </summary>
    private void Refuse(Artifact artifact)
    {
        int index = -1;
        IReadOnlyList<Artifact> artifacts = player.Inventory.GetPlayerArtifacts();
        for (int i = 0; i < artifacts.Count; i++)
            if (artifacts[i] == artifact) index = i;
        if (index < 0 || index >= skills.Count) return;
        SkillSlot skill = skills[index];
        skill.AddToClassList("skill--refused");
        for (int step = 0; step < refuseShake.Length; step++)
        {
            float offset = refuseShake[step];
            skill.schedule.Execute(() => skill.style.translate = new Translate(offset, 0)).StartingIn(step * RefuseStep);
        }
        skill.schedule.Execute(() => {
            skill.style.translate = StyleKeyword.Null;
            skill.RemoveFromClassList("skill--refused");
        }).StartingIn(refuseShake.Length * RefuseStep + 200);
    }

    /// <summary>
    /// Highlights the skill of the artifact the player attacks with, none if its index is -1
    /// </summary>
    private void Highlight(int artifactIndex)
    {
        for (int i = 0; i < skills.Count; i++)
            skills[i].EnableInClassList("selected", i == artifactIndex);
    }

    /// <summary>
    /// Updates the skills, keeping their elements while their number stays the same so that they animate
    /// </summary>
    private void Refresh()
    {
        IReadOnlyList<Artifact> artifacts = player.Inventory.GetPlayerArtifacts();
        if (artifacts.Count != skills.Count)
        {
            root.Clear();
            skills.Clear();
            HideTooltip();
            for (int i = 0; i < artifacts.Count; i++)
                skills.Add(CreateSkill(i));
        }
        for (int i = 0; i < artifacts.Count; i++)
            skills[i].Set(artifacts[i], artifacts[i].CanUse(player.Stats));
    }

    private SkillSlot CreateSkill(int index)
    {
        // The keys 1 to 9 select the first skills
        var skill = new SkillSlot { Key = index < 9 ? (index + 1).ToString() : "" };
        skill.RegisterCallback<PointerDownEvent>(_ => Select(index));
        skill.RegisterCallback<PointerEnterEvent>(_ => {
            showTooltip?.Pause();
            showTooltip = tooltip.schedule.Execute(() => ShowTooltip(index)).StartingIn(TooltipDelay);
        });
        skill.RegisterCallback<PointerLeaveEvent>(_ => HideTooltip());
        root.Add(skill);
        // The skills pop one after the other (transition of Hud.uss)
        skill.schedule.Execute(() => skill.AddToClassList("skill--shown")).StartingIn(StaggerDelay * (index + 1));
        return skill;
    }

    /// <summary>
    /// Attacks with the artifact, or goes back to moving if the player already attacks with it
    /// </summary>
    private void Select(int index)
    {
        if (GameScene.IsGameplayBlocked) return;
        PlayerAttack attack = player.playerAttack;
        if (!player.IsAttacking || attack.currentArtifact != player.Inventory.GetPlayerArtifacts()[index])
            player.SetState(PlayerTurn.PlayerState.ATTACK, index);
        else
            player.SetState(PlayerTurn.PlayerState.MOVE);
    }

    private void ShowTooltip(int index)
    {
        if (GameScene.IsGameplayBlocked) return;
        Artifact artifact = player.Inventory.GetPlayerArtifacts()[index];
        tooltip.text = "<b>" + artifact.Title + "</b>\n" + artifact.EffectDescription + "\n<size=85%>" + artifact.RangeDescription + "   " + artifact.CooldownDescription + "</size>";
        tooltip.AddToClassList("shown");
    }

    private void HideTooltip()
    {
        showTooltip?.Pause();
        tooltip.RemoveFromClassList("shown");
    }
}
