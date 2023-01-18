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

    private static TurnSystem instance;

    public static TurnSystem Instance { get { 
            if (instance == null) instance = FindObjectOfType<TurnSystem>();
            return instance;
        } }

    public bool IsCombat { get => isCombat; }
    public bool IsPlayerTurn { get => turns[currentTurn] == playerTurn; }

	private void Update() {
        if (isCombat) turns[currentTurn].TurnUpdate();
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
        if (isCombat) {
            playerTurn.OnCombatStart();
            NextTurnButton.instance.EnterState(NextTurnButton.State.COMBAT);
        }
        if (FindObjectOfType<Map>() != null) {
            FindObjectOfType<Map>().CurrentRoom.LockExits(isCombat);
        }
        currentTurn = 0;
        turns[currentTurn].OnTurnLaunch();
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
            playerTurn = null;
            isCombat = false;
		}
        else if (turns.Count == 1) {
            ActionManager.queueFree.AddListener(EndCombat);
        }
    }

    private void EndCombat() {
        ActionManager.queueFree.RemoveListener(EndCombat);
        AkSoundEngine.PostEvent("SwitchExplore", gameObject);
        isCombat = false;
        if (FindObjectOfType<Map>() != null) FindObjectOfType<Map>().CurrentRoom.LockExits(false);
        foreach (EntityTurn turn in turns) turn.OnCombatEnd();
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

    public void EndPlayerTurn() {
        if (!isCombat || !IsPlayerTurn) return;
        GoToNextTurn();
	}

    /// <summary>
    /// Gets all enemies currently in combat
    /// </summary>
    /// <returns></returns>
    public List<EntityTurn> GetEnemies() {
        List<EntityTurn> enemies = new List<EntityTurn>();
        foreach(EntityTurn turn in turns) {
            if (turn.stats.type == EntityType.ENEMY) enemies.Add(turn);
		}
        return enemies;
	}
}
