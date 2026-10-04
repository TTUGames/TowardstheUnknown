using System;
using UnityEngine.UIElements;

/// <summary>
/// The hints of the first combat, each shown once ever (<see cref="HintCard"/>), above the skills bar: choosing the start
/// tile at the first deploy, moving and casting at the player's first turn, ending the turn once the energy is first spent.
/// The chest's is the inventory's (<see cref="InventoryScreen"/>)
/// </summary>
public class HudHints : IDisposable
{
    private const string DeployHint = "Deploy";
    private const string TurnHint = "Turn";
    private const string EndTurnHint = "EndTurn";

    private readonly HintCard card;
    private readonly PlayerTurn player;

    /// <param name="wipe">The room wipe, drawn over the card</param>
    public HudHints(SlantedWipe wipe, PlayerTurn player)
    {
        this.player = player;
        card = new HintCard();
        card.AddToClassList("hint--hud");
        wipe.parent.Insert(wipe.parent.IndexOf(wipe), card);
        GameEvents.DeployChoiceShown += OnDeployChoiceShown;
        GameEvents.CombatStarted += OnCombatStarted;
        GameEvents.CombatEnded += OnCombatEnded;
        GameEvents.RoomLeft += OnCombatEnded;
        TurnSystem.Instance.TurnChanged += OnTurnChanged;
        player.Stats.EnergyChanged += OnEnergyChanged;
        Edition.Changed += OnEditionChanged;
    }

    public void Dispose()
    {
        GameEvents.DeployChoiceShown -= OnDeployChoiceShown;
        GameEvents.CombatStarted -= OnCombatStarted;
        GameEvents.CombatEnded -= OnCombatEnded;
        GameEvents.RoomLeft -= OnCombatEnded;
        // The turn system may be destroyed first when the scene unloads
        if (TurnSystem.Instance != null) TurnSystem.Instance.TurnChanged -= OnTurnChanged;
        if (player != null) player.Stats.EnergyChanged -= OnEnergyChanged;
        Edition.Changed -= OnEditionChanged;
    }

    private void OnDeployChoiceShown(CombatPlayerDeploy deploy) => card.TryShow(DeployHint, "HintDeploy", HintCard.Key(GameInput.Controls.Gameplay.EndTurn));

    private void OnCombatStarted() => card.Hide(DeployHint);

    private void OnCombatEnded() => card.Hide();

    private void OnTurnChanged()
    {
        TurnSystem turnSystem = TurnSystem.Instance;
        if (!turnSystem.IsCombat) return;
        if (turnSystem.IsPlayerTurn)
        {
            var gameplay = GameInput.Controls.Gameplay;
            card.TryShow(TurnHint, "HintTurn", HintCard.Key(gameplay.Skill1), HintCard.Key(gameplay.Skill2));
            return;
        }
        // The player ended a turn: the end turn hint has nothing to teach
        card.Hide();
        HintCard.MarkSeen(EndTurnHint);
    }

    private void OnEnergyChanged()
    {
        TurnSystem turnSystem = TurnSystem.Instance;
        if (turnSystem == null || !turnSystem.IsCombat || !turnSystem.IsPlayerTurn || player.Stats.CurrentEnergy > 0) return;
        card.Hide(TurnHint);
        card.TryShow(EndTurnHint, "HintEndTurn", HintCard.Key(GameInput.Controls.Gameplay.EndTurn));
    }

    private void OnEditionChanged(GameEdition edition)
    {
        if (!Edition.Profile.hints) card.Hide();
    }
}
