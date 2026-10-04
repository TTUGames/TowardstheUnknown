using UnityEngine;

/// <summary>
/// Counts the errors and exceptions logged since the launch, which a player never sees (an exception in a GameAction only
/// drops the action): the options show how many, next to a button opening the folder of the Player.log, for a bug report.
/// The log starts with the version (<see cref="GameFlow"/>) and each run writes its map's seed
/// </summary>
public static class ErrorLog
{
    /// <summary>
    /// The errors and exceptions logged since the launch
    /// </summary>
    public static int Count { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Application.logMessageReceivedThreaded -= OnLog;
        Count = 0;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init() => Application.logMessageReceivedThreaded += OnLog;

    private static void OnLog(string message, string stackTrace, LogType type)
    {
        if (type is LogType.Error or LogType.Exception or LogType.Assert) Count++;
    }

    /// <summary>
    /// The folder of the Player.log (and of the screenshots and the saves), opened in the system's file browser
    /// </summary>
    public static void OpenFolder() => Application.OpenURL("file:///" + Application.persistentDataPath.Replace('\\', '/'));
}
