using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActionManager : MonoBehaviour
{
    private static readonly List<Action> actions = new List<Action>();
    public static UnityEvent queueFree = new UnityEvent();

	private void Awake() {
        //Static state could outlive a previous game scene
        actions.Clear();
        queueFree.RemoveAllListeners();
	}

	void FixedUpdate()
    {
        while (actions.Count != 0) {
            Action action = actions[0];
            try {
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
                queueFree.Invoke();
                return;
            }
        }
    }

    public static void AddToBottom(Action action) {
        actions.Add(action);
	}

    public static void AddToTop(Action action) {
        actions.Insert(0, action);
	}

    public static void Clear() {
        actions.Clear();
	}

    public static bool IsBusy { get => actions.Count != 0; }
}
