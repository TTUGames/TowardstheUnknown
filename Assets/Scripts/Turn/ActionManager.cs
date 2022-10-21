using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    private static List<Action> actions;

	private void Awake() {
        if (actions != null) throw new System.Exception("Multiple ActionManager cannot coexist");
        actions = new List<Action>();
	}

	// Update is called once per frame
	void Update()
    {
        bool canDoAction = actions.Count != 0;
        while (canDoAction) {
            Action action = actions[0];
            action.Apply();
            if (action.isDone) {
                actions.Remove(action);
                canDoAction = actions.Count != 0;
			}
            else {
                canDoAction = false;
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
