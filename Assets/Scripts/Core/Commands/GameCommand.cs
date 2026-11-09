using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// What a player's command does (<see cref="GameCommand"/>)
/// </summary>
public enum CommandType
{
    /// <summary>Ends the deploy phase on a deploy tile (<see cref="GameCommand.cell"/>)</summary>
    Deploy,
    /// <summary>Moves to a tile in combat (<see cref="GameCommand.cell"/>)</summary>
    Move,
    /// <summary>Casts the artifact of an index (<see cref="GameCommand.artifact"/>) on a tile (<see cref="GameCommand.cell"/>)</summary>
    Cast,
    /// <summary>Ends the player's turn</summary>
    EndTurn,
    /// <summary>Takes the exit of a side (<see cref="GameCommand.direction"/>)</summary>
    Exit,
    /// <summary>Goes straight to a room (<see cref="GameCommand.target"/>, the debug shortcut F8)</summary>
    JumpTo,
    /// <summary>The player's grid as rearranged (<see cref="GameCommand.items"/>): a chest emptied, an artifact moved or turned</summary>
    Inventory,
}

/// <summary>
/// One decision of the player, as the gameplay took it: what, in which room (its position on the map) and during which of
/// the run's player turns. A run is its seed and the list of its commands (<see cref="CommandJournal"/>): replaying them on
/// the seed plays it again (<see cref="CommandReplay"/>). The tiles are cells of the room's layout
/// </summary>
[Serializable]
public class GameCommand
{
    public CommandType type;
    // The player's combat turns played in the run when it was given (CommandLog.PlayerTurn)
    public int turn;
    public Vector2Int room;
    public Vector2Int cell;
    public int artifact;
    public Direction direction;
    public Vector2Int target;
    public List<RunSave.Item> items;

    public override string ToString() => type switch
    {
        CommandType.Deploy or CommandType.Move => $"{type} {cell}",
        CommandType.Cast => $"Cast #{artifact} on {cell}",
        CommandType.Exit => $"Exit {direction}",
        CommandType.JumpTo => $"JumpTo {target}",
        CommandType.Inventory => $"Inventory ({items?.Count ?? 0} artifacts)",
        _ => type.ToString(),
    } + $" (turn {turn}, room {room})";
}

/// <summary>
/// A run's journal: what replays it, its seed, its edition and its commands, in order
/// </summary>
[Serializable]
public class CommandJournal
{
    // 2: the snapshots hold the enemies' AI state (StateSnapshot.Entity.ai)
    public const int CurrentVersion = 2;

    public int version = CurrentVersion;
    public string gameVersion;
    public int seed;
    public bool chosenSeed;
    public GameEdition edition;
    // It doesn't start with the run (a suspended run resumed without its journal): it can't be replayed
    public bool partial;
    public List<GameCommand> commands = new();
    public List<StateCheckpoint> checkpoints = new();
}

/// <summary>
/// The state of the game (<see cref="StateSnapshot"/>) at an end of the player's turn or of a combat, after the commands
/// recorded so far
/// </summary>
[Serializable]
public class StateCheckpoint
{
    // EndTurn or CombatEnd
    public string label;
    // The commands recorded before it
    public int command;
    public int turn;
    public Vector2Int room;
    public string hash;
    public string dump;

    public override string ToString() => $"{label} after command {command} (turn {turn}, room {room}) {hash}";
}
