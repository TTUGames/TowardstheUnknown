using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the turn order
/// </summary>
public class TurnSystem : MonoBehaviour
{
    private List<EntityTurn> turns = new List<EntityTurn>();
    private PlayerTurn playerTurn;
    private int currentTurn;
    private bool isCombat = false;
    private static TurnSystem instance;

    public static TurnSystem Instance { get {
            if (instance == null) instance = FindAnyObjectByType<TurnSystem>();
            return instance;
        } }

    /// <summary>
    /// Fired when the turn order shown by the timeline changes: a room was entered, a combat started or a dead entity disappeared
    /// </summary>
    public event System.Action TurnOrderChanged;

    public void NotifyTurnOrderChanged() => TurnOrderChanged?.Invoke();

    /// <summary>
    /// Fired when another entity starts its turn, and when the combat ends
    /// </summary>
    public event System.Action TurnChanged;

    public bool IsCombat { get => isCombat; }
    public IReadOnlyList<EntityTurn> Turns => turns;
    public bool IsPlayerTurn { get => turns[currentTurn] == playerTurn; }

    /// <summary>
    /// Whether it is this entity's turn in the current combat
    /// </summary>
    public bool IsCurrentTurn(EntityTurn turn) => isCombat && currentTurn < turns.Count && turns[currentTurn] == turn;

    /// <summary>
    /// The entity playing its turn in the current combat, null out of combat
    /// </summary>
    public EntityTurn Current => isCombat && currentTurn < turns.Count ? turns[currentTurn] : null;

	/// <summary>
	/// Subscribes an <c>EntityTurn</c> to the <c>TurnSystem</c>, and sets it as the first to play
	/// </summary>
	/// <param name="turn"></param>
	public void RegisterPlayer(PlayerTurn turn) {
        if (playerTurn != null) throw new System.Exception("The player can't be registered twice in TurnSystem");
        turns.Insert(0, turn);
        playerTurn = turn;
	}

    /// <summary>
    /// Subscribes an <c>EntityTurn</c> to the <c>TurnSystem</c>, and sets it as the last to play
    /// </summary>
    /// <param name="turn"></param>
    public void RegisterEnemy(EntityTurn turn) {
        if (turns.Contains(turn)) throw new System.Exception("Enemies can't be registered twice in TurnSystem");
        turns.Add(turn);
	}

    /// <summary>
    /// Clears the turns registered in the <c>TurnSystem</c>
    /// </summary>
    public void Clear() {
        playerTurn = null;
        turns.Clear();
	}

    /// <summary>
    /// Tries to start a combat. Requires the player turn to be registered.
    /// </summary>
    public void CheckForCombatStart() {
        if (playerTurn == null) throw new System.Exception("Player not found to start combat");
        isCombat = turns.Count > 1;
        currentTurn = 0;
        if (isCombat) {
            NotifyTurnOrderChanged();
            GameEvents.StartCombat();
        }
        else GameEvents.StartExploration();
        LaunchCurrentTurn();
    }

    /// <summary>
    /// Removes an <c>EntityTurn</c> from the <c>TurnSystem</c>
    /// </summary>
    /// <param name="turn"></param>
    public void Remove(EntityTurn turn) {
        int index = turns.IndexOf(turn);
        if (index < 0) return;
        turns.RemoveAt(index);

        if (turn == playerTurn) {
            playerTurn = null;
            isCombat = false;
        }
        else if (turns.Count == 1) {
            ActionManager.QueueFree += EndCombat;
        }

        if (turns.Count == 0) return;
        if (index < currentTurn) {
            --currentTurn;
        }
        else if (index == currentTurn) {
            currentTurn %= turns.Count;
            if (isCombat) LaunchCurrentTurn();
        }
    }

    private void EndCombat() {
        ActionManager.QueueFree -= EndCombat;
        if (playerTurn == null) return; //The player died in the same attack
        isCombat = false;
        GameEvents.EndCombat();
        foreach (EntityTurn turn in turns) turn.OnCombatEnd();
        GameEvents.StartExploration();
        TurnChanged?.Invoke();
    }

    /// <summary>
    /// Ends the current <c>EntityTurn</c> and starts the next one.
    /// </summary>
    public void GoToNextTurn() {
        if (!isCombat) return;
        turns[currentTurn].OnTurnStop();
        currentTurn = (currentTurn + 1) % turns.Count;
        LaunchCurrentTurn();
	}

    private void LaunchCurrentTurn() {
        turns[currentTurn].OnTurnLaunch();
        TurnChanged?.Invoke();
    }

    /// <summary>
    /// Ends the player's turn, once its casts are done if it is casting
    /// </summary>
    public void EndPlayerTurn() {
        if (!isCombat || !IsPlayerTurn || playerTurn.playerAttack.DeferEndTurn() || ActionManager.IsBusy) return;
        GoToNextTurn();
	}
}
