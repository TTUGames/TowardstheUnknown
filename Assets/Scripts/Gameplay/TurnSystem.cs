using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the turn order
/// </summary>
public class TurnSystem : MonoBehaviour
{
    private List<EntityTurn> turns = new List<EntityTurn>();
    private int currentTurn;
    private bool isPlaying = false;
    public bool IsPlaying { get => isPlaying; }

    /// <summary>
    /// Update starts the Turn System when there are at least 2 entities in the room. 
    /// TODO : Maybe replace it to have a manager notice the <c>TurnSystem</c> when a fight start instead of checking with Update.
    /// </summary>
	void Update() {
		if (!isPlaying && turns.Count > 1) {
            currentTurn = 0;
            turns[0].OnTurnLaunch();
            isPlaying = true;
		}
	}

    /// <summary>
    /// Subscribes an <c>EntityTurn</c> to the <c>TurnSystem</c>, and sets it as the first to play
    /// </summary>
    /// <param name="turn"></param>
	public void AddToStart(EntityTurn turn) {
        turns.Insert(0, turn);
	}

    /// <summary>
    /// Subscribes an <c>EntityTurn</c> to the <c>TurnSystem</c>, and sets it as the last to play
    /// </summary>
    /// <param name="turn"></param>
    public void AddToEnd(EntityTurn turn) {
        turns.Add(turn);
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
        if (turns.Count == 1) isPlaying = false;
	}

    /// <summary>
    /// Ends the current <c>EntityTurn</c> and starts the next one.
    /// </summary>
    public void GoToNextTurn() {
        turns[currentTurn].OnTurnStop();
        currentTurn = (currentTurn + 1) % turns.Count;
        turns[currentTurn].OnTurnLaunch();
	}
}
