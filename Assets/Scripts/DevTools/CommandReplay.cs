using System.Collections;
using UnityEngine;

/// <summary>
/// Plays a run's journal again (<see cref="CommandJournal"/>, <see cref="CommandLog"/>): a new run on its seed and in its
/// edition, then each command pushed once the game is where it was given (<see cref="PlayerCommands.Ready"/>), to reproduce a
/// reported bug. The run is a chosen seed's: unranked. It deletes the suspended run, as a new run does. Driven by the
/// unity-playtest probes (<c>Replay.Start</c>, <c>Replay.Status</c>); the replay's own journal goes to commands-replay.json
/// </summary>
public class CommandReplay : MonoBehaviour
{
    private static CommandReplay current;

    /// <summary>
    /// Where the last replay is: running, done or stopped at a command, and why
    /// </summary>
    public static string Status { get; private set; } = "no replay";

    /// <summary>
    /// A replay plays
    /// </summary>
    public static bool IsPlaying => current != null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        current = null;
        Status = "no replay";
    }

    private CommandJournal journal;
    private float timeout;
    // The replayed journal's states met so far, and the first that differed
    private int checkpoints;
    private string divergence;

    /// <summary>
    /// Replays the journal of a file; false if it can't be read or doesn't start with its run
    /// </summary>
    /// <param name="timeout">Real seconds a command may wait for the game to be ready before the replay stops there</param>
    public static bool Play(string path, float timeout = 90)
    {
        CommandJournal journal = CommandLog.Read(path);
        if (journal == null || journal.partial || current != null || GameFlow.IsLoading)
        {
            Status = journal == null ? $"unreadable journal, or not of version {CommandJournal.CurrentVersion}: {path}" : journal.partial ? "partial journal: it doesn't start with its run" : "busy";
            return false;
        }
        current = new GameObject("CommandReplay").AddComponent<CommandReplay>();
        DontDestroyOnLoad(current.gameObject);
        current.journal = journal;
        current.timeout = timeout;
        current.StartCoroutine(current.Run());
        return true;
    }

    /// <summary>
    /// Stops the replay where it is; the run goes on, played by hand
    /// </summary>
    public static void Stop()
    {
        if (current == null) return;
        Status += " (stopped)";
        current.Finish();
    }

    private IEnumerator Run()
    {
        CommandLog.CheckpointRecorded += Compare;
        Edition.Set(journal.edition);
        int seed = journal.seed;
        Status = $"starting seed {seed}";
        GameFlow.StartRun(seed);
        yield return new WaitUntil(() => !GameFlow.IsLoading && GameScene.Map != null && GameScene.Map.CurrentRoom != null && GameRandom.Seed == seed);

        int count = journal.commands.Count;
        for (int i = 0; i < count; i++)
        {
            GameCommand command = journal.commands[i];
            Status = $"{i + 1}/{count} waiting for {command}";
            float until = Time.realtimeSinceStartup + timeout;
            while (!PlayerCommands.Ready(command))
            {
                if (Time.realtimeSinceStartup > until)
                {
                    Status = $"diverged at {i + 1}/{count}: {command} never ready (room {GameScene.Map?.CurrentRoomPosition}, player turn {CommandLog.PlayerTurn})";
                    Debug.LogWarning("[replay] " + Status);
                    Finish();
                    yield break;
                }
                yield return null;
            }
            // A chest opened meanwhile holds its screen open
            if (GameScene.UI.Inventory.IsOpen) GameScene.UI.Inventory.Toggle();
            if (!PlayerCommands.Execute(command))
            {
                Status = $"diverged at {i + 1}/{count}: {command} can't be played here";
                Debug.LogWarning("[replay] " + Status);
                Finish();
                yield break;
            }
            Debug.Log($"[replay] {i + 1}/{count} {command}");
            yield return null;
        }
        // The states that follow the last command (the enemies' turns, the end of the combat)
        float settle = Time.realtimeSinceStartup + timeout;
        yield return new WaitUntil(() => (!ActionManager.IsBusy && (!TurnSystem.Instance.IsCombat || TurnSystem.Instance.IsPlayerTurn))
            || Time.realtimeSinceStartup > settle);
        Status = $"done: {count} commands replayed on seed {seed}, " + (divergence ?? $"{checkpoints}/{journal.checkpoints.Count} states identical");
        Debug.Log("[replay] " + Status);
        Finish();
    }

    /// <summary>
    /// Compares a state the replay records with the replayed journal's of the same rank: the first that differs is kept, with
    /// both dumps written next to the journals (state-expected.json, state-replayed.json)
    /// </summary>
    private void Compare(StateCheckpoint replayed)
    {
        int index = checkpoints++;
        if (divergence != null) return;
        if (index >= journal.checkpoints.Count)
        {
            divergence = $"state {index + 1} has no counterpart ({replayed})";
            return;
        }
        StateCheckpoint expected = journal.checkpoints[index];
        if (expected.hash == replayed.hash) return;
        divergence = $"state diverged at {index + 1}: {expected} vs {replayed.hash}, {StateSnapshot.FirstDifference(expected.dump, replayed.dump)}";
        Debug.LogWarning("[replay] " + divergence);
        try
        {
            System.IO.File.WriteAllText(System.IO.Path.Combine(Application.persistentDataPath, "state-expected.json"), expected.dump);
            System.IO.File.WriteAllText(System.IO.Path.Combine(Application.persistentDataPath, "state-replayed.json"), replayed.dump);
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning("State dumps not written: " + exception.Message);
        }
    }

    private void Finish()
    {
        CommandLog.CheckpointRecorded -= Compare;
        CommandLog.Write();
        current = null;
        Destroy(gameObject);
    }
}
