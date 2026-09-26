using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

/// <summary>
/// The skills of the artifacts in the player's inventory. Clicking one attacks with it, hovering it shows its effects
/// </summary>
public class SkillsBar : IDisposable
{
    // Between the appearance of two skills
    private const long StaggerDelay = 50;

    private readonly VisualElement root;
    private readonly HudTooltip tooltip;
    private readonly PlayerTurn player;
    private readonly List<SkillSlot> skills = new();

    public SkillsBar(VisualElement root, HudTooltip tooltip, PlayerTurn player)
    {
        this.root = root;
        this.tooltip = tooltip;
        this.player = player;

        player.Stats.EnergyChanged += Refresh;
        player.Inventory.ArtifactsChanged += Refresh;
        player.SelectedArtifactChanged += Highlight;
        player.playerAttack.ArtifactRefused += Refuse;
        player.playerAttack.QueueChanged += Refresh;
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
        player.playerAttack.QueueChanged -= Refresh;
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
            // The removed skills hide their tooltip
            skills.Clear();
            for (int i = 0; i < artifacts.Count; i++)
                skills.Add(CreateSkill(i));
        }
        IReadOnlyList<PlayerAttack.QueuedCast> queued = player.playerAttack.QueuedCasts;
        for (int i = 0; i < artifacts.Count; i++)
        {
            skills[i].Set(artifacts[i], artifacts[i].CanUse(player.Stats));
            int count = 0;
            foreach (PlayerAttack.QueuedCast cast in queued)
                if (cast.Artifact == artifacts[i]) count++;
            skills[i].Queued = count;
        }
    }

    private SkillSlot CreateSkill(int index)
    {
        // The keys 1 to 9 select the first skills
        var skill = new SkillSlot { Key = index < 9 ? (index + 1).ToString() : "" };
        skill.RegisterCallback<PointerDownEvent>(_ => Select(index));
        // The tooltip keeps its place, above the middle of the bar
        tooltip.Register(skill, () => TooltipText(index), HudTooltip.Placement.Styled);
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

    /// <summary>
    /// The artifact's title, effects, range and cooldown
    /// </summary>
    private string TooltipText(int index)
    {
        IReadOnlyList<Artifact> artifacts = player.Inventory.GetPlayerArtifacts();
        if (index >= artifacts.Count) return null;
        Artifact artifact = artifacts[index];
        return "<b>" + artifact.Title + "</b>\n" + artifact.EffectDescription + "\n<size=85%>" + artifact.RangeDescription + "   " + artifact.CooldownDescription + "</size>";
    }
}
