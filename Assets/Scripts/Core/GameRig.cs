using UnityEngine;

/// <summary>
/// The root of <c>Managers/GameRig.prefab</c>, which holds everything a playable scene needs but its map: settings, UI, player,
/// gameplay managers, Wwise, music, snow and tools. Every playable scene is this prefab plus a map variant, so that a change
/// to the rig reaches them all. On load, before any other script, it moves its children to the scene root and goes: the game
/// runs with the same root objects as before, which <c>DontDestroyOnLoad</c> (Wwise, Steam) needs.
/// </summary>
[DefaultExecutionOrder(-32000)]
public class GameRig : MonoBehaviour
{
    private void Awake()
    {
        while (transform.childCount > 0)
            transform.GetChild(0).SetParent(null, true);
        Destroy(gameObject);
    }
}
