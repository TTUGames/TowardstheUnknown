using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    private static readonly List<GameAction> actions = new List<GameAction>();
    private static ActionManager instance;

    /// <summary>
    /// Fired when the last action of the queue is done
    /// </summary>
    public static event System.Action QueueFree;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        actions.Clear();
        QueueFree = null;
    }

	private void Awake() {
        //Static state could outlive a previous game scene
        ResetStatics();
        instance = this;
	}

	void Update()
    {
        while (actions.Count != 0) {
            GameAction action = actions[0];
            try {
                if (!action.IsStarted) action.Start();
                action.Apply();
            }
            catch (System.Exception e) {
                //A failing action would otherwise stay at the head of the queue and block the game
                Debug.LogException(e);
                action.isDone = true;
            }
            if (!action.isDone) return;
            actions.Remove(action);
            if (actions.Count == 0) {
                QueueFree?.Invoke();
                return;
            }
        }
    }

    public static void AddToBottom(GameAction action) {
        actions.Add(action);
	}

    public static void AddToTop(GameAction action) {
        actions.Insert(0, action);
	}

    public static void Clear() {
        actions.Clear();
	}

    /// <summary>
    /// Runs a coroutine for an action on the manager, which outlives the entities involved
    /// </summary>
    public static Coroutine Run(IEnumerator routine) {
        return instance.StartCoroutine(routine);
    }

    /// <summary>
    /// Calls the callback now if the queue is empty, else once it gets empty
    /// </summary>
    public static void WhenFree(System.Action callback) {
        if (!IsBusy) {
            callback();
            return;
        }
        void OnFree() {
            QueueFree -= OnFree;
            callback();
        }
        QueueFree += OnFree;
    }

    /// <summary>
    /// Completes now if the queue is empty, else once it gets empty
    /// </summary>
    public static Awaitable WaitFree()
    {
        var completion = new AwaitableCompletionSource();
        WhenFree(completion.SetResult);
        return completion.Awaitable;
    }

    public static bool IsBusy { get => actions.Count != 0; }
}
