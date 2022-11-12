using System.Collections;
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

    public bool IsCombat { get => isCombat; }

	private void Update() {
        if (!isCombat) CheckForCombatStart();
        else turns[currentTurn].TurnUpdate();
	}

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
    /// Starts the combat if it is not yet and there are multiple entities registered
    /// </summary>
    private void CheckForCombatStart() {
        if (!isCombat && turns.Count > 1) {
            isCombat = true;
            currentTurn = 0;
            turns[0].OnTurnLaunch();
        }
    }

    /// <summary>
    /// Removes an <c>EntityTurn</c> from the <c>TurnSystem</c>
    /// </summary>
    /// <param name="turn"></param>
    public void Remove(EntityTurn turn) {
        bool isCurrentTurn = (turn == turns[currentTurn]);
        turns.Remove(turn);
        if (isCurrentTurn) {
            currentTurn = currentTurn % turns.Count;
            turns[currentTurn].OnTurnLaunch();
        }
        if (turn == playerTurn) {
            isCombat = false;
		}
        else if (turns.Count == 1) {
            isCombat = false;
            playerTurn.OnCombatEnd();
		}
	}

    /// <summary>
    /// Ends the current <c>EntityTurn</c> and starts the next one.
    /// </summary>
    public void GoToNextTurn() {
        if (!isCombat) return;
        turns[currentTurn].OnTurnStop();
        currentTurn = (currentTurn + 1) % turns.Count;
        turns[currentTurn].OnTurnLaunch();
	}

    /// <summary>
    /// Gets all enemies currently in combat
    /// </summary>
    /// <returns></returns>
    public List<EntityTurn> GetEnemies() {
        List<EntityTurn> enemies = new List<EntityTurn>();
        foreach(EntityTurn turn in turns) {
            if (turn != playerTurn) enemies.Add(turn);
		}
        return enemies;
	}
}
