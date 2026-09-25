using UnityEngine;

/// <summary>
/// Game-wide events, raised by gameplay and listened to by the UI
/// </summary>
public static class GameEvents
{
    /// <summary>
    /// Fired when the run ends, with true if the player won
    /// </summary>
    public static event System.Action<bool> RunEnded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        RunEnded = null;
    }

    public static void EndRun(bool isVictory) => RunEnded?.Invoke(isVictory);
}
