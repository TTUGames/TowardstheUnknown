using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Records the run's journal (<see cref="CommandJournal"/>): each player's decision as the gameplay takes it (deploy, move
/// and cast in combat, end turn, exit, the grid rearranged), in a random run. It is written in commands.json of
/// <see cref="Application.persistentDataPath"/> (next to the Player.log and run.json, the folder the options open) at each end
/// of turn, room change and run end; a new run keeps the previous one's as commands-previous.json. A suspended run goes on
/// with its journal, cut where the save was written (<see cref="RunSave.Data.commandCount"/>). A replay
/// (<see cref="CommandReplay"/>) records into commands-replay.json instead
/// </summary>
public static class CommandLog
{
    private static string Folder => Application.persistentDataPath;
    public static string CurrentPath => Path.Combine(Folder, "commands.json");
    public static string PreviousPath => Path.Combine(Folder, "commands-previous.json");
    public static string ReplayPath => Path.Combine(Folder, "commands-replay.json");

    /// <summary>
    /// The run's journal, null outside a random run
    /// </summary>
    public static CommandJournal Journal { get; private set; }

    /// <summary>
    /// The player's combat turns started in the run, numbering the commands' turns
    /// </summary>
    public static int PlayerTurn { get; private set; }

    /// <summary>
    /// The run is a replay's (<see cref="CommandReplay"/>): its journal goes to <see cref="ReplayPath"/>
    /// </summary>
    public static bool Replaying { get; private set; }

    /// <summary>
    /// Fired when the state is recorded (<see cref="Checkpoint"/>): the replay compares it with the replayed journal's
    /// </summary>
    public static event Action<StateCheckpoint> CheckpointRecorded;

    // The grid as last recorded, to record it again only once changed
    private static List<RunSave.Item> lastItems;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Journal = null;
        PlayerTurn = 0;
        Replaying = false;
        lastItems = null;
        CheckpointRecorded = null;
    }

    private static string WritePath => Replaying ? ReplayPath : CurrentPath;

    /// <summary>
    /// Starts the journal of a run, once its randomness has its seed (<see cref="Map"/>): a new one, or a suspended run's
    /// own, cut at its save; none outside a random run
    /// </summary>
    public static void Begin(bool randomRun, RunSave.Data resumed)
    {
        Journal = null;
        PlayerTurn = 0;
        lastItems = null;
        Replaying = CommandReplay.IsPlaying;
        if (!randomRun) return;
        if (resumed != null && !Replaying)
        {
            CommandJournal saved = Read(CurrentPath);
            if (saved != null && saved.seed == resumed.seed && saved.commands.Count >= resumed.commandCount)
            {
                saved.commands.RemoveRange(resumed.commandCount, saved.commands.Count - resumed.commandCount);
                saved.checkpoints.RemoveAll(checkpoint => checkpoint.command > resumed.commandCount);
                Journal = saved;
                PlayerTurn = saved.commands.Count > 0 ? saved.commands[^1].turn : 0;
                lastItems = LastItems(saved) ?? Snapshot();
                return;
            }
        }
        if (!Replaying) Rotate();
        Journal = new CommandJournal
        {
            gameVersion = Application.version,
            seed = GameRandom.Seed,
            chosenSeed = GameRandom.Chosen,
            edition = Edition.Current,
            partial = resumed != null,
        };
        // The starting grid needs no command: the run gives it
        lastItems = Snapshot();
    }

    // The grid of the last Inventory command, null if none: the starting grid then
    private static List<RunSave.Item> LastItems(CommandJournal journal) =>
        journal.commands.LastOrDefault(command => command.type == CommandType.Inventory)?.items;

    /// <summary>
    /// A player's combat turn starts
    /// </summary>
    public static void CountPlayerTurn() => PlayerTurn++;

    /// <summary>
    /// Records a command, after the grid if it changed since it was last recorded; the end of a turn and an exit write the
    /// journal
    /// </summary>
    public static void Record(GameCommand command)
    {
        if (Journal == null || GameScene.Map == null) return;
        RecordInventory();
        Add(command);
        // The player's turn as it ends, before the enemies play; the state recorded writes the journal
        if (command.type == CommandType.EndTurn) Checkpoint("EndTurn");
        else if (command.type is CommandType.Exit or CommandType.JumpTo) Write();
    }

    /// <summary>
    /// Records the state of the game now (<see cref="StateSnapshot"/>) in the journal
    /// </summary>
    public static void Checkpoint(string label)
    {
        if (Journal == null || GameScene.Map == null) return;
        string dump = StateSnapshot.Capture().Dump();
        var checkpoint = new StateCheckpoint
        {
            label = label, command = Journal.commands.Count, turn = PlayerTurn, room = GameScene.Map.CurrentRoomPosition,
            hash = StateSnapshot.HashOf(dump), dump = dump,
        };
        Journal.checkpoints.Add(checkpoint);
        CheckpointRecorded?.Invoke(checkpoint);
        Write();
    }

    private static void Add(GameCommand command)
    {
        command.turn = PlayerTurn;
        command.room = GameScene.Map.CurrentRoomPosition;
        Journal.commands.Add(command);
    }

    /// <summary>
    /// Records the grid if it changed since it was last recorded (the starting grid counts as recorded)
    /// </summary>
    public static void RecordInventory()
    {
        if (Journal == null || GameScene.Map == null) return;
        List<RunSave.Item> items = Snapshot();
        if (lastItems != null && Same(items, lastItems)) return;
        lastItems = items;
        Add(new GameCommand { type = CommandType.Inventory, items = items });
    }

    /// <summary>
    /// The player's grid: each artifact's ID, slot and rotation, in the skills' order
    /// </summary>
    public static List<RunSave.Item> Snapshot() => GameScene.Player.Inventory.Data.Items
        .Select(item => new RunSave.Item { artifact = item.itemData.ID, slot = item.slot, rotation = item.rotation }).ToList();

    private static bool Same(List<RunSave.Item> a, List<RunSave.Item> b) =>
        a.Count == b.Count && a.Zip(b, (x, y) => x.artifact == y.artifact && x.slot == y.slot && x.rotation == y.rotation).All(same => same);

    /// <summary>
    /// The commands recorded, which the run's save keeps to cut the journal of the run it resumes
    /// </summary>
    public static int Count
    {
        get
        {
            RecordInventory();
            return Journal?.commands.Count ?? 0;
        }
    }

    /// <summary>
    /// Writes the journal to its file, replaced at once
    /// </summary>
    public static void Write()
    {
        if (Journal == null) return;
        try
        {
            string path = WritePath;
            File.WriteAllText(path + ".tmp", JsonUtility.ToJson(Journal, true));
            if (File.Exists(path)) File.Delete(path);
            File.Move(path + ".tmp", path);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Command journal not written: " + exception.Message);
        }
    }

    /// <summary>
    /// The run ends: its journal is written with its last commands
    /// </summary>
    public static void End()
    {
        RecordInventory();
        Write();
    }

    /// <summary>
    /// Reads a journal, null if there is none or it can't be read
    /// </summary>
    public static CommandJournal Read(string path)
    {
        try
        {
            if (!File.Exists(path)) return null;
            CommandJournal journal = JsonUtility.FromJson<CommandJournal>(File.ReadAllText(path));
            return journal != null && journal.version == CommandJournal.CurrentVersion ? journal : null;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Command journal unreadable: " + exception.Message);
            return null;
        }
    }

    // The last run's journal becomes the previous one's
    private static void Rotate()
    {
        try
        {
            if (!File.Exists(CurrentPath)) return;
            if (File.Exists(PreviousPath)) File.Delete(PreviousPath);
            File.Move(CurrentPath, PreviousPath);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Command journal not kept: " + exception.Message);
        }
    }
}
